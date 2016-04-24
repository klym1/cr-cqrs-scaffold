using System;
using System.Collections.Generic;
using CommandHandlers;
using CR.AggregateRepository.Core;
using CR.MessageDispatch.Core;
using CR.MessageDispatch.Dispatchers.EventStore;
using CR.ViewModels.Core;
using CR.ViewModels.Persistance.Memory;
using Denormalizers;
using EventStore.ClientAPI;
using Nancy.TinyIoc;
using ViewModels;

namespace ConsoleApiHost
{
    public class Bootstrapper : RestApiBootstrapper
    {
        protected override List<IDispatcher<object>> GetCommandDispatchers(TinyIoCContainer container)
        {
            var widgetCommandHandler = new WidgetCommandHandler(container.Resolve<IAggregateRepository>());

            var typedRegistry = new MessageHandlerRegistry<Type>();
            typedRegistry.AddByConvention(widgetCommandHandler);

            var commandsDispatcher = new RawMessageDispatcher<object>(typedRegistry);

            return new List<IDispatcher<object>> { commandsDispatcher };
        } 

        protected override List<IDispatcher<ResolvedEvent>> GetDispatchers(TinyIoCContainer container)
        {
            var widgetDenormalizer = new WidgetDenormalizer(container.Resolve<IViewModelWriter>());
            var typedRegistry = new MessageHandlerRegistry<Type>();

            typedRegistry.AddByConvention(widgetDenormalizer);

            var aggregateDispatcher = new EventStoreAggregateEventDispatcher(typedRegistry);

            return new List<IDispatcher<ResolvedEvent>>() { aggregateDispatcher };
        }

        protected override IEnumerable<object> GetPairs(InMemoryViewModelRepository repo)
        {
            foreach (var item in (IDictionary<string, Widget>)repo.EntityCollections[typeof(Widget)])
                yield return item;
        }

        protected override void ViewModelLoaded(InMemoryViewModelRepository repo)
        {
            if (repo.EntityCollections.Count == 0)
                return;

            foreach (var item in (IDictionary<string, Widget>)repo.EntityCollections[typeof(Widget)])
            {
                item.Value.CreatedAt = DateTime.SpecifyKind(item.Value.CreatedAt, DateTimeKind.Local);
            }
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register<IWidgetCommandHandler, WidgetCommandHandler>();
            base.ConfigureApplicationContainer(container);
        }
    }
}