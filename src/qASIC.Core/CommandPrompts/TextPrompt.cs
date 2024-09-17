namespace qASIC.CommandPrompts
{
    public class TextPrompt : CommandPrompt
    {
        public string Text { get; private set; }

        public override ConsoleArgument[] Prepare(CommandArgs args)
        {
            Text = args.inputString;
            return new ConsoleArgument[]
            {
                new ConsoleArgument(args.inputString, new object[] { args.inputString }),
            };
        }
    }
}