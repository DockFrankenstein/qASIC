using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qASIC.Console.Parsing.Arguments
{
    public class SmartParser : ArgumentsParser
    {
        public override ConsoleArgument[] ParseString(string cmd)
        {
            return new ConsoleArgument[0];
        }
    }
}