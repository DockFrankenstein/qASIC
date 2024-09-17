using System;

namespace qASIC.Console.Commands.BuiltIn
{
    public class Exit : GameCommand
    {
        public override string CommandName => "exit";
        public override string Description => "Closes the application.";
        public override string[] Aliases => new string[] { "quit" };

        public override object Run(GameCommandArgs args)
        {
            args.CheckArgumentCount(0);
            Logs.Log("Goodbye");
            Environment.Exit(0);
            return null;
        }
    }
}