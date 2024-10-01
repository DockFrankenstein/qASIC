namespace qASIC.Console
{
    public class GameCommandArgs : CommandArgs
    {
        public GameCommandArgs() { }
        public GameCommandArgs(CommandArgs args) : base(args) { }

        public GameConsole console;
    }
}