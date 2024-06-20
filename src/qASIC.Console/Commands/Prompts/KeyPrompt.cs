using System.Collections.Generic;
using System.Linq;

namespace qASIC.Console.Commands.Prompts
{
    public class KeyPrompt : CommandPrompt
    {
        public enum NavigationKey
        {
            None,
            Up,
            Down,
            Left,
            Right,
            Confirm,
            Cancel,
        }

        static readonly Dictionary<string, NavigationKey> namesToKeys = new Dictionary<string, NavigationKey>()
        {
            ["up"] = NavigationKey.Up,
            ["down"] = NavigationKey.Down,
            ["left"] = NavigationKey.Left,
            ["right"] = NavigationKey.Right,
            ["confirm"] = NavigationKey.Confirm,
            ["cancel"] = NavigationKey.Cancel,
        };

        public NavigationKey Key { get; private set; } = NavigationKey.None;

        public override bool CanExecute(CommandArgs args) =>
            args.inputString.Length > 0;

        public override ConsoleArgument[] Prepare(CommandArgs args)
        {
            string s = args.inputString.First().ToString();

            if (namesToKeys.TryGetValue(args.inputString.ToLower(), out var key))
            {
                Key = key;
                s = args.inputString.ToLower();
            }

            var values = s.Length == 1 ?
                new object[] { s[0], s } :
                new object[] { s };

            return new ConsoleArgument[]
            {
                new ConsoleArgument(s, values),
            };
        }
    }
}