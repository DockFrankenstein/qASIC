using System.Collections.Generic;

namespace qASIC
{
    public interface ICommandList
    {
        void AddCommand(ICommand command);
        void AddCommandRange(IEnumerable<ICommand> commands);
    }
}
