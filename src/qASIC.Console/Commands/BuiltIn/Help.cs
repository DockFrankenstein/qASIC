using System.Text;

namespace qASIC.Console.Commands.BuiltIn
{
    [BuildInConsoleCommand]
    public class Help : GameCommand
    {
        public override string CommandName => "help";
        public override string? Description => "Displays a list of all avaliable commands.";

        public bool MultiplePages { get; set; } = true;
        public bool AllowDetailedDescription { get; set; } = true;
        public int PageCommandLimit { get; set; } = 16;

        public override object? Run(CommandArgs args)
        {
            //Ignore page argument if multipage and detailed description is off
            if (!MultiplePages)
                args.CheckArgumentCount(0);

            args.CheckArgumentCount(0, 1);

            string? targetCommand = null;
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

            var commandList = args.console.CommandList!;
            if (targetCommand != null)
            {
                if (!commandList.TryGetCommand(targetCommand, out IGameCommand? command) || command == null)
                    throw new GameCommandException($"Command '{targetCommand}' does not exist!");

                if (command.DetailedDescription == null && command.Description == null)
                {
                    args.console.Log($"No detailed help avaliable for command '{targetCommand}'");
                    return null;
                }

                args.console.Log($"Help for command '{command.CommandName}': {command.DetailedDescription ?? command.Description}", "info");
                return null;
            }

            var startIndex = PageCommandLimit * index;

            if (startIndex >= commandList.Length)
                throw new GameCommandException("Page index out of range");

            StringBuilder stringBuilder = new StringBuilder(MultiplePages ? 
                $"List of avaliable commands, page: {index} \n" :
                "List of avaliable commands \n");

            for (int i = index * PageCommandLimit; i < Math.Max(index * (PageCommandLimit + 1), commandList.Length); i++)
                stringBuilder.AppendLine($"{commandList[i].CommandName} - {commandList[i].Description ?? "No description"}");

            args.console.Log(stringBuilder.ToString(), "info");

            return null;
        }
    }
}