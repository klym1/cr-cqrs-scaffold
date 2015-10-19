using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands
{
    public class CreateWidget
    {
        public Guid WidgetId { get; set; }

        public string Name { get; set; }

        public CreateWidget(Guid widgetId, string name)
        {
            WidgetId = widgetId;
            Name = name;
        }
    }
}
