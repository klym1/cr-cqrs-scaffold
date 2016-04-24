using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using CR.AggregateRepository.Core;
using CR.AggregateRepository.Persistance.EventStore;
using CR.MessageDispatch.Core;
using CR.MessageDispatch.Dispatchers.EventStore;
using CR.MessageDispatch.Dispatchers.Snapshotting.Protobuf;
using CR.ViewModels.Core;
using CR.ViewModels.Persistance.Memory;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Logary;
using Nancy;
using Nancy.Diagnostics;
using Nancy.Extensions;
using Nancy.Responses;
using Nancy.TinyIoc;
using NancyModules;
using HttpStatusCode = Nancy.HttpStatusCode;

namespace ConsoleApiHost
{
    public abstract class RestApiBootstrapper : DefaultNancyBootstrapper
    {
        private readonly Logger _logger = Logary.Logging.GetCurrentLogger();

        protected override void ApplicationStartup(TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            if (ConfigurationManager.AppSettings["RequestTracing"] != null &&
                ConfigurationManager.AppSettings["RequestTracing"].ToLower() == "true")
                StaticConfiguration.EnableRequestTracing = true;

            //does this actually happen after container config?
            var subscriber = container.Resolve<EventStoreSubscriber>();
            subscriber.Start();

            //log catchup progress to every 5s
            var sw = Stopwatch.StartNew();
            var timer = new System.Timers.Timer(5000);
            int prevTotal = 0;
            timer.Elapsed += (sender, eventArgs) =>
            {
                if (subscriber.ViewModelsReady)
                    return;

                var progress = subscriber.CatchUpPercentage;

                Logary.Logging.GetCurrentLogger().Info(progress.ToString());

                var last10s = progress.EventsProcessed - prevTotal;

                prevTotal = progress.EventsProcessed;

                Logary.Logging.GetCurrentLogger()
                    .LogFormat(LogLevel.Info, "Events per second: Average: {0:0.#}, Last 5s: {1:0.#}",
                        progress.EventsProcessed/(sw.ElapsedMilliseconds/1000), last10s/5);

            };
            timer.Start();

            pipelines.BeforeRequest.AddItemToEndOfPipeline(Before);
            pipelines.AfterRequest.AddItemToEndOfPipeline(After);

            pipelines.OnError.AddItemToEndOfPipeline(Error);
            //base.ApplicationStartup(container, pipelines);
        }

        //if viewmodels are not yet built, return 503 unavailable for all gets
        private Response Before(NancyContext nancyContext)
        {
            var builder = ApplicationContainer.Resolve<EventStoreSubscriber>();
            if (!builder.ViewModelsReady && nancyContext.Request.Method.Equals("GET"))
                return
                    new TextResponse(builder.CatchUpPercentage.ToString()).WithStatusCode(
                        HttpStatusCode.ServiceUnavailable);

            return null;
        }

        private object GetLogData(NancyContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            object Response = resp == null
                ? new object()
                : new
                {
                    resp.ContentType,
                    resp.Cookies,
                    resp.Headers,
                    resp.ReasonPhrase,
                    resp.StatusCode
                };

            return new
            {
                Request = new
                {
                    req.UserHostAddress,
                    req.Path,
                    req.Query,
                    req.Method,
                    Body = req.Body == null ? "null" : req.Body.AsString(),
                    req.Headers,
                    req.Cookies,
                    req.Form
                },
                Response
            };
        }

        private void After(NancyContext nancyContext)
        {
            if (nancyContext.Response != null)
            {
                var status = (int) nancyContext.Response.StatusCode;
                var level = LogLevel.Error;

                if (status >= 400 && status <= 600)
                {
                    var req = nancyContext.Request;

                    if (status < 500)
                        level = LogLevel.Warn;

                    var logMessage = String.Format("Responding {0} for {1} to path {2}", status, req.Method,
                        nancyContext.ToFullPath(req.Path));
                    Logary.Logging.GetCurrentLogger()
                        .Log(level, logMessage, GetLogData(nancyContext), null, null, nancyContext.ToFullPath(req.Path));
                }

                nancyContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                nancyContext.Response.Headers.Add("Access-Control-Allow-Methods",
                    "GET, POST, PUT, DELETE, OPTIONS, PATCH");
                nancyContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            }
        }

        private Response Error(NancyContext nancyContext, Exception ex)
        {
            var req = nancyContext.Request;
            var logMessage = string.Format("An error occurred handling {0} to {1}", nancyContext.Request.Method,
                nancyContext.ToFullPath(req.Path));

            Logary.Logging.GetCurrentLogger()
                .Log(LogLevel.Error, logMessage, GetLogData(nancyContext), null, ex, nancyContext.ToFullPath(req.Path));
            return nancyContext.Response;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            var viewmodel = new InMemoryViewModelRepository();
            var snapshotPath = ConfigurationManager.AppSettings["snapshotPath"];
            var snapshotVersion = ConfigurationManager.AppSettings["snapshotVersion"];
            var streamName = ConfigurationManager.AppSettings["streamName"];

            var snapshottingDispatcher = new ProtoBufSnapshottingResolvedEventDispatcher(() =>
            {
                var repo = (container.Resolve<IViewModelReader>() as InMemoryViewModelRepository);
                return GetPairs(repo);
            }, snapshotPath, snapshotVersion);

            var items = snapshottingDispatcher.LoadObjects();
            int? startingEvent = snapshottingDispatcher.LoadCheckpoint();
            //add the items to the viewmodel
            foreach (var dynamicItem in items.Select(item => item as dynamic))
                viewmodel.Add(dynamicItem.Key, dynamicItem.Value);

            ViewModelLoaded(viewmodel);
            //event store
            var esLogger = new EventStoreLogaryLogger();
            var conn = SetUpEventStoreConnection("EventStoreConnection", esLogger);
            container.Register(conn);

            //aggregate repository
            container.Register<IAggregateRepository, EventStoreAggregateRepository>();

            container.Register<IViewModelReader>(viewmodel);
            container.Register<IViewModelWriter>(viewmodel);

            var multiplexingDispatcher = new MultiplexingDispatcher<ResolvedEvent>(GetDispatchers(container).ToArray());
            var commandDispatcher = new CommmandDispatcher(new MultiplexingDispatcher<object>(GetCommandDispatchers(container).ToArray()));

            snapshottingDispatcher.InnerDispatcher = multiplexingDispatcher;

            var loggingDispatcher = new ErrorLoggingDispatcher(snapshottingDispatcher, false);

            container.Register<IDispatcher<ResolvedEvent>>(loggingDispatcher);

            var subscriber = new EventStoreSubscriber(conn, loggingDispatcher, streamName, esLogger, startingEvent, 4096, 70000);
            
            container.Register(subscriber);
            container.Register<ICommandDispatcher>(commandDispatcher);
        }

        protected abstract void ViewModelLoaded(InMemoryViewModelRepository repo);

        protected abstract List<IDispatcher<ResolvedEvent>> GetDispatchers(TinyIoCContainer container);
        protected abstract List<IDispatcher<object>> GetCommandDispatchers(TinyIoCContainer container);

        protected abstract IEnumerable<object> GetPairs(InMemoryViewModelRepository repo);

        private IEventStoreConnection SetUpEventStoreConnection(string connectionStringName, ILogger esLogger)
        {
            var eventStoreConnectionString =
                new Uri(ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString);
            var settings =
                ConnectionSettings.Create().UseCustomLogger(esLogger).KeepReconnecting().LimitRetriesForOperationTo(10);
            var eventStoreClient = EventStoreConnection.Create(settings, eventStoreConnectionString);
            eventStoreClient.Closed += EventStoreClientOnClosed;
            eventStoreClient.ErrorOccurred += EventStoreClientOnErrorOccurred;
            eventStoreClient.Connected += EventStoreClientOnConnected;
            eventStoreClient.Disconnected += EventStoreClientOnDisconnected;
            eventStoreClient.Reconnecting += EventStoreClientOnReconnecting;
            eventStoreClient.ConnectAsync().Wait();

            return eventStoreClient;
        }

        private void EventStoreClientOnReconnecting(object sender, ClientReconnectingEventArgs clientReconnectingEventArgs)
        {
            _logger.Warn("Event Store reconnecting");
        }

        private void EventStoreClientOnClosed(object sender, ClientClosedEventArgs clientClosedEventArgs)
        {
            _logger.Info("Event Store connection closed");
        }
        
        private void EventStoreClientOnDisconnected(object sender, ClientConnectionEventArgs clientConnectionEventArgs)
        {
            _logger.Error(
                $"Event Store disconnected from endpoint {clientConnectionEventArgs.RemoteEndPoint.Address}:{clientConnectionEventArgs.RemoteEndPoint.Port}");
        }

        private void EventStoreClientOnConnected(object sender, ClientConnectionEventArgs clientConnectionEventArgs)
        {
            _logger.Info(
                $"Event Store connected {clientConnectionEventArgs.RemoteEndPoint.Address}:{clientConnectionEventArgs.RemoteEndPoint.Port}");
        }

        private void EventStoreClientOnErrorOccurred(object sender, ClientErrorEventArgs clientErrorEventArgs)
        {
            if (clientErrorEventArgs.Exception != null)
            {
                _logger.ErrorException("Event Store error occurred", clientErrorEventArgs.Exception);
            }
            else
            {
                _logger.Error("Unknown Event Store error");
            }
        }
    }
}