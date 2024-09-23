namespace qASIC.CommandPrompts
{
    public class TextPrompt : CommandPrompt
    {
        public string Text { get; private set; }

        public override CommandArgument[] Prepare(CommandArgs args)
        {
            Text = args.inputString;
            return new CommandArgument[]
            {
                new CommandArgument(args.inputString, new object[] { args.inputString }),
            };
        }
    }
}