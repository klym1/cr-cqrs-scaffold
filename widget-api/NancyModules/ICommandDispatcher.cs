using System.Collections.Generic;
using CR.MessageDispatch.Core;

namespace NancyModules
{
    public interface ICommandDispatcher : IDispatcher<object>
    {
        
    }

    //just to distinguish from other dispatchers
    public class CommmandDispatcher : ICommandDispatcher
    {
        private readonly IDispatcher<object> _dispatcher;

        public CommmandDispatcher(IDispatcher<object> dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Dispatch(object message)
        {
            _dispatcher.Dispatch(message);
        }
    }
}