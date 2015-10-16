using System;

namespace Events
{
    public class WidgetCreated
    {
        public Guid WidgetId { get; private set; }
        public string Name { get; private set; }
        public DateTime CreatedDateTime { get; private set; }

        public WidgetCreated(Guid widgetId, string name, DateTime createdDateTime)
        {
            WidgetId = widgetId;
            Name = name;
            CreatedDateTime = createdDateTime;
        }
    }
}
