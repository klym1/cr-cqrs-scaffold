using System.Linq;
using CR.ViewModels.Core;
using Nancy;
using ViewModels;

namespace NancyModules
{
    public class ReadModule : NancyModule
    {
        private IViewModelReader _reader;
        public ReadModule(IViewModelReader reader)
        {
            _reader = reader;

            Get["/widgets"] = parameters =>
            {
                var response = reader.Query<Widget>().ToArray();
                return Negotiate.WithModel(response);
            };

            Get["/widgets/{widgetId}"] = parameters =>
            {
                Widget widget = reader.GetByKey<Widget>(parameters.widgetId);
                if (widget == null)
                    return HttpStatusCode.NotFound;
                return Negotiate.WithModel(widget);
            };
        }
    }
}
