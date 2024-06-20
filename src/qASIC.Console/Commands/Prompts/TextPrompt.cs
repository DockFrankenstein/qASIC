namespace qASIC.Console.Commands.Prompts
{
    public class TextPrompt : CommandPrompt
    {
        public override ConsoleArgument[] Prepare(CommandArgs args) =>
            new ConsoleArgument[] 
            { 
                new ConsoleArgument(args.inputString, new object[] { args.inputString }), 
            };
    }
}