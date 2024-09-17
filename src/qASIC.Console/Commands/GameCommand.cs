using qASIC.Core;

namespace qASIC.Console.Commands
{
    public abstract class GameCommand : ICommand, IHasLogs
    {
        public abstract string CommandName { get; }

        public virtual string[] Aliases => new string[0];

        public virtual string Description => null;

        public virtual string DetailedDescription => null;

        public LogManager Logs { get; set; } = new LogManager();

        public object Run(CommandArgs args) =>
            Run(args as GameCommandArgs);

        public abstract object Run(GameCommandArgs args);
    }
}