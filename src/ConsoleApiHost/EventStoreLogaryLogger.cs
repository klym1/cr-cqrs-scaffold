using System;
using EventStore.ClientAPI;
using Logary;

namespace ConsoleApiHost
{
    public class EventStoreLogaryLogger : ILogger
    {
        private readonly Logger _logger = Logary.Logging.GetLoggerByName("EventStore.ClientApi");

        public void Error(string format, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                _logger.Error(format);
            }
            else
            {
                _logger.LogFormat(LogLevel.Error, format, args);
            }
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            var message = String.Format(format, args);
            _logger.ErrorException(message, ex);
        }

        public void Info(string format, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                _logger.Info(format);
            }
            else
            {
                _logger.LogFormat(LogLevel.Info, format, args);
            }
        }

        public void Info(Exception ex, string format, params object[] args)
        {
            var message = String.Format(format, args);
            _logger.InfoException(message, ex);
        }

        public void Debug(string format, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                _logger.Debug(format);
            }
            else
            {
                _logger.LogFormat(LogLevel.Debug, format, args);
            }
        }

        public void Debug(Exception ex, string format, params object[] args)
        {
            var message = String.Format(format, args);
            _logger.DebugException(message, ex);
        }
    }
}