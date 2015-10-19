using System;
using System.Runtime.Serialization;

namespace ViewModels
{
    [Serializable]
    [DataContract]
    public class Widget
    {
        public Guid WidgetId { get; set; }
        public string WidgetName { get; set; }
        public DateTime CreatedAt { get; set; }

        public Widget(Guid widgetId, string widgetName, DateTime createdAt)
        {
            WidgetId = widgetId;
            WidgetName = widgetName;
            CreatedAt = createdAt;
        }

        public Widget() {}
    }
}
