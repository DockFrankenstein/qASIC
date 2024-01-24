namespace qASIC.Console.Commands.BuiltIn
{
    [BuildInConsoleCommand]
    public class Echo : GameCommand
    {
        public override string CommandName => "echo";
        public override string? Description => "Echos a message.";
        public override string[] Aliases => new string[] { "print" };

        public override object? Run(CommandArgs args)
        {
            args.CheckArgumentCount(1);
            args.console.Log(args[1].arg);
            return null;
        }
    }
}