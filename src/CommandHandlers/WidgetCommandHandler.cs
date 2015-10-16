using System.Runtime.InteropServices;
using Commands;
using CR.AggregateRepository.Core;
using CR.MessageDispatch.Core;
using Domain;

namespace CommandHandlers
{
    public class WidgetCommandHandler : IWidgetCommandHandler
    {
        private readonly IAggregateRepository _aggregateRepository;

        public WidgetCommandHandler(IAggregateRepository aggregateRepository)
        {
            _aggregateRepository = aggregateRepository;
        }

        public void Handle(CreateWidget message)
        {
            var widgetAggregate = new WidgetAggregate(message.WidgetId, message.Name);
            _aggregateRepository.Save(widgetAggregate);
        }
    }
}