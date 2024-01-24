namespace qASIC.Console.Commands.BuiltIn
{
    [BuildInConsoleCommand]
    public class Version : GameCommand
    {
        public override string CommandName => "version";
        public override string? Description => "Displays current project version.";
        public override string[] Aliases => new string[] { "info", "about" };

        public override object? Run(CommandArgs args)
        {
            args.CheckArgumentCount(0);
            args.console.Log("TODO: do that");
            return null;
        }
    }
}