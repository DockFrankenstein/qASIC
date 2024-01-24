namespace qASIC.Console
{
    public class CommandArgsCountException : GameCommandException
    {
        public CommandArgsCountException() { }
        public CommandArgsCountException(int inputArgsCount, int minArgsCount, int maxArgsCount)
        {
            this.inputArgsCount = inputArgsCount;
            this.minArgsCount = minArgsCount;
            this.maxArgsCount = maxArgsCount;
        }

        int inputArgsCount;
        int minArgsCount;
        int maxArgsCount;

        public override string Message
        {
            get
            {
                if (inputArgsCount < minArgsCount)
                    return "Not enough arguments";

                if (inputArgsCount > maxArgsCount)
                    return "Too many arguments";

                return "Invalid argument count";
            }
        }
    }
}