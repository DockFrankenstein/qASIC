using System;
using System.Collections.Generic;

namespace qASIC
{
    public interface ICommandList : IEnumerable<ICommand>
    {
        ICommandList AddCommand(ICommand command);
        ICommandList AddCommandRange(IEnumerable<ICommand> commands);

        event Action<IEnumerable<ICommand>> OnCommandsAdded;

        public bool TryGetCommand(string commandName, out ICommand command);
    }
}
