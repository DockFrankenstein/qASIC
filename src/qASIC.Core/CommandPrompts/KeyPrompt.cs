using System.Collections.Generic;
using System.Linq;

namespace qASIC.CommandPrompts
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

        public static readonly Map<string, NavigationKey> keyNames = new Map<string, NavigationKey>(new Dictionary<string, NavigationKey>()
        {
            [""] = NavigationKey.None,
            ["up"] = NavigationKey.Up,
            ["down"] = NavigationKey.Down,
            ["left"] = NavigationKey.Left,
            ["right"] = NavigationKey.Right,
            ["confirm"] = NavigationKey.Confirm,
            ["cancel"] = NavigationKey.Cancel,
        });

        public NavigationKey Key { get; private set; } = NavigationKey.None;

        public override bool CanExecute(CommandArgs args) =>
            args.inputString.Length > 0;

        public override ConsoleArgument[] Prepare(CommandArgs args)
        {
            string s = args.inputString.First().ToString();

            if (keyNames.Forward.TryGetValue(args.inputString.ToLower(), out var key))
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