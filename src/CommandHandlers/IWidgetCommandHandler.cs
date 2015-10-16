using Commands;
using CR.MessageDispatch.Core;

namespace CommandHandlers
{
    public interface IWidgetCommandHandler : IConsume<CreateWidget>
    {
    }
}