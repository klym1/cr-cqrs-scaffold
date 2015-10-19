using System;
using CR.MessageDispatch.Core;
using EventStore.ClientAPI;
using Logary;

namespace ConsoleApiHost
{
    public class ErrorLoggingDispatcher : IDispatcher<ResolvedEvent>
    {
        private readonly IDispatcher<ResolvedEvent> _innerDispatcher;
        private readonly bool _exitOnError;
        private readonly Logger _logger = Logary.Logging.GetCurrentLogger();

        public ErrorLoggingDispatcher(IDispatcher<ResolvedEvent> innerDispatcher, bool exitOnError)
        {
            _innerDispatcher = innerDispatcher;
            _exitOnError = exitOnError;
        }

        public void Dispatch(ResolvedEvent message)
        {
            try
            {
                _innerDispatcher.Dispatch(message);
            }
            catch (Exception ex)
            {
                var logMessage = String.Format("Error while processing {0} {1}@{2}", message.Event.EventType, message.OriginalEventNumber, message.OriginalStreamId);
                _logger.ErrorException(logMessage, ex);

                if (_exitOnError)
                    Environment.Exit(1);
            }
        }
    }
}