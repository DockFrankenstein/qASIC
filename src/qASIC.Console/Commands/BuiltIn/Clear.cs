namespace qASIC.Console.Commands.BuiltIn
{
    [BuiltInCommandTarget]
    public class Clear : GameCommand
    {
        public override string CommandName => "clear";
        public override string Description => "Clears the console.";
        public override string[] Aliases => new string[] { "cls", "clr" };

        public override object Run(CommandArgs args)
        {
            args.CheckArgumentCount(0);
            args.console.Log(qLog.CreateNow(string.Empty, LogType.Clear, qColor.Clear));
            return null;
        }
    }
}