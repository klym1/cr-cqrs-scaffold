using System;
using CommandHandlers;
using Commands;
using CR.AggregateRepository.Core.Exceptions;
using Nancy;
using Nancy.ModelBinding;
using NancyModules.RequestModels;

namespace NancyModules
{
    public class WriteModule : NancyModule
    {
        private IWidgetCommandHandler _handler;

        public WriteModule(IWidgetCommandHandler handler)
        {
            _handler = handler;
            Post["/widgets/"] = parameters =>
            {
                var postData = this.Bind<WidgetPostModel>();
                var createWidgetCommand = new CreateWidget(Guid.NewGuid(), postData.Name);
                var respone = DispatchCommand(createWidgetCommand, HttpStatusCode.Created);
                respone.Headers.Add("location", $"/widgets/{createWidgetCommand.WidgetId}");
                return respone;
            };
        }

        private Response DispatchCommand(object command, HttpStatusCode responseCode)
        {
            try
            {
                ((dynamic)_handler).Handle((dynamic)command);
            }
            catch (InvalidOperationException ex)
            {
                return Response.AsText(ex.Message).WithStatusCode(HttpStatusCode.BadRequest);
            }
            catch (AggregateNotFoundException)
            {
                return HttpStatusCode.NotFound;
            }
            catch (Exception ex)
            {
                return Response.AsText(ex.Message).WithStatusCode(HttpStatusCode.InternalServerError);
            }

            return responseCode;
        }
    }
}
