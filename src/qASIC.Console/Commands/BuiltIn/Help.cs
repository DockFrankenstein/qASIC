﻿using System.Text;
using System;
using System.Linq;

namespace qASIC.Console.Commands.BuiltIn
{
    public class Help : GameCommand
    {
        public override string CommandName => "help";
        public override string Description => "Displays a list of all avaliable commands.";

        public bool MultiplePages { get; set; } = true;
        public bool AllowDetailedDescription { get; set; } = true;
        public int PageCommandLimit { get; set; } = 16;

        public override object Run(GameCommandArgs args)
        {
            //Ignore page argument if multipage and detailed description is off
            if (!MultiplePages)
                args.CheckArgumentCount(0);

            args.CheckArgumentCount(0, 1);

            string targetCommand = null;
            int index = 0;

            //help <index>
            if (args.Length == 2)
            {
                switch (args[1].CanGetValue<int>())
                {
                    case true:
                        index = args[1].GetValue<int>();
                        break;
                    case false:
                        targetCommand = args[1].arg;
                        break;
                }
            }

            var commandList = args.console.CommandList;
            var commands = commandList.ToList();
            if (targetCommand != null)
            {
                if (!commandList.TryGetCommand(targetCommand, out ICommand command) || command == null)
                    throw new CommandException($"Command '{targetCommand}' does not exist!");

                if (command.DetailedDescription == null && command.Description == null)
                {
                    Logs.Log($"No detailed help avaliable for command '{targetCommand}'");
                    return null;
                }

                Logs.Log($"Help for command '{command.CommandName}': {command.DetailedDescription ?? command.Description}", "info");
                return null;
            }

            var startIndex = PageCommandLimit * index;

            if (startIndex >= commands.Count)
                throw new CommandException("Page index out of range");

            StringBuilder stringBuilder = new StringBuilder(MultiplePages ? 
                $"List of avaliable commands, page: {index} \n" :
                "List of avaliable commands \n");

            for (int i = index * PageCommandLimit; i < Math.Max(index * (PageCommandLimit + 1), commands.Count); i++)
                stringBuilder.AppendLine($"{commands[i].CommandName} - {commands[i].Description ?? "No description"}");

            Logs.Log(stringBuilder.ToString(), "info");

            return null;
        }
    }
}