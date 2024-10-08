﻿namespace qASIC.Console.Commands.BuiltIn
{
    public class Echo : GameCommand
    {
        public override string CommandName => "echo";
        public override string Description => "Echos a message.";
        public override string[] Aliases => new string[] { "print" };

        public override object Run(GameCommandArgs args)
        {
            args.CheckArgumentCount(1);
            Logs.Log(args[1].arg);
            return null;
        }
    }
}