namespace qASIC.Console.Commands.Prompts
{
    public class InputPrompt : CommandPrompt
    {
        public override bool ParseArguments => true;

        public override ConsoleArgument[] Prepare(CommandArgs args) =>
            args.args;
    }
}