namespace qASIC.Console
{
    public class CommandParseException : GameCommandException
    {
        public CommandParseException(Type type, string arg)
        {
            this.type = type;
            this.arg = arg;
        }

        Type type;
        string arg;

        public override string Message =>
            $"Unable to parse '{arg}' to {type}";
    }
}