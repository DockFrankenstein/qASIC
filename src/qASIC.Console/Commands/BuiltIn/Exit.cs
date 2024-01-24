namespace qASIC.Console.Commands.BuiltIn
{
    [BuildInConsoleCommand]
    public class Exit : GameCommand
    {
        public override string CommandName => "exit";
        public override string? Description => "Closes the application.";
        public override string[] Aliases => new string[] { "quit" };

        public override object? Run(CommandArgs args)
        {
            args.CheckArgumentCount(0);
            args.console.Log("Goodbye");
            Environment.Exit(0);
            return null;
        }
    }
}