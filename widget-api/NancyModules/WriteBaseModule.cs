using System;
using CR.AggregateRepository.Core.Exceptions;
using Nancy;

namespace NancyModules
{
    public class WriteBaseModule : NancyModule
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public WriteBaseModule(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        protected Response DispatchCommand(object command, HttpStatusCode responseCode)
        {
            try
            {
                _commandDispatcher.Dispatch(command);
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