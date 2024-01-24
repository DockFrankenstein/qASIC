using qASIC.Console.Parsing.Values;

namespace qASIC.Console.Parsing.Arguments
{
    public abstract class ArgumentsParser
    {
        public ArgumentsParser() { }

        public List<ValueParser> ValueParsers { get; set; } = new List<ValueParser>()
        {
            new IntParser(),
            new UIntParser(),
            new FloatParser(),
            new DoubleParser(),
            new DecimalParser(),
            new LongParser(),
            new UlongParser(),
            new ByteParser(),
            new SByteParser(),
            new ShortParser(),
            new UShortParser(),
            new BoolParser(),
            new StringParser(),
        };

        public abstract ConsoleArgument[] ParseString(string cmd);

        protected object[] ParseArgument(string arg)
        {
            List<object> parsedArgs = new List<object>();
            foreach (var parser in ValueParsers)
                if (parser.TryParse(arg, out object? result) && result != null)
                    parsedArgs.Add(result);

            return parsedArgs.ToArray();
        }
    }
}