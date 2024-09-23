using qASIC.Parsing;
using System.Collections.Generic;

namespace qASIC.Console.Parsing.Arguments
{
    public abstract class ArgumentsParser
    {
        public ArgumentsParser() { }

        public List<ValueParser> ValueParsers { get; set; } = new List<ValueParser>(ValueParser.CreateStandardParserArray());

        public abstract CommandArgument[] ParseString(string cmd);

        protected object[] ParseArgument(string arg)
        {
            List<object> parsedArgs = new List<object>();
            foreach (var parser in ValueParsers)
                if (parser.TryParse(arg, out object result) && result != null)
                    parsedArgs.Add(result);

            return parsedArgs.ToArray();
        }
    }
}