using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CR.AggregateRepository.Core;
using Events;
using TimeKeeping;

namespace Domain
{
    public struct WidgetId
    {
        private readonly Guid _widgetId;

        public WidgetId(Guid widgetId)
        {
            _widgetId = widgetId;
        }

        public override string ToString()
        {
            return $"widget-{_widgetId}";
        }
    }

    public class WidgetAggregate : AggregateBase
    {
        private Guid WidgetId { get; set; }

        public override object Id => new WidgetId(WidgetId);

        public WidgetAggregate(Guid widgetId, string name)
        {
            var now = Clock.Now;
            RaiseEvent(new WidgetCreated(widgetId, name, now));
        }

        public void Apply(WidgetCreated e)
        {
            WidgetId = e.WidgetId;
        }

        //paramless constructor needed for rebuilding from event stream
        private WidgetAggregate() {}
    }
}
