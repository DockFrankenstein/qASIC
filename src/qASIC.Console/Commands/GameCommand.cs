using qASIC.Core;

namespace qASIC.Console.Commands
{
    public abstract class GameCommand : IGameCommand, IHasLogs
    {
        public abstract string CommandName { get; }

        public virtual string[] Aliases => new string[0];

        public virtual string Description => null;

        public virtual string DetailedDescription => null;

        public LogManager Logs { get; set; } = new LogManager();

        public abstract object Run(CommandArgs args);
    }
}