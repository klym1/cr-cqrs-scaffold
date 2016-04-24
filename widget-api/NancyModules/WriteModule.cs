using System;
using CommandHandlers;
using Commands;
using Nancy;
using Nancy.ModelBinding;
using NancyModules.RequestModels;

namespace NancyModules
{
    public class WriteModule : WriteBaseModule
    {
        public WriteModule(ICommandDispatcher dispatcher) : base(dispatcher)
        {
            Post["/widgets/"] = parameters =>
            {
                var postData = this.Bind<WidgetPostModel>();
                var createWidgetCommand = new CreateWidget(Guid.NewGuid(), postData.Name);
                var respone = DispatchCommand(createWidgetCommand, HttpStatusCode.Created);
                respone.Headers.Add("location", $"/widgets/{createWidgetCommand.WidgetId}");
                return respone;
            };
        }
    }
}
