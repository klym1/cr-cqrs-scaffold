using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CR.MessageDispatch.Core;
using CR.ViewModels.Core;
using Events;
using ViewModels;

namespace Denormalizers
{
    public class WidgetDenormalizer : IConsume<WidgetCreated>
    {
        private IViewModelWriter _repository;

        public WidgetDenormalizer(IViewModelWriter repository)
        {
            _repository = repository;
        }

        public void Handle(WidgetCreated message)
        {
            _repository.Add(message.WidgetId.ToString(), new Widget(message.WidgetId, message.Name, message.CreatedDateTime));
        }
    }
}
