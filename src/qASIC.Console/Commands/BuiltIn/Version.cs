using System;
using System.Text;

namespace qASIC.Console.Commands.BuiltIn
{
    [BuiltInCommandTarget]
    public class Version : GameCommand
    {
        public override string CommandName => "version";
        public override string Description => "Displays current project version.";
        public override string[] Aliases => new string[] { "info", "about" };

        public event Func<RemoteAppInfo, string> GetInfoString =  (a) =>
        {
            var txt = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(a.projectName))
                txt.Append($"{a.projectName} ");

            if (!string.IsNullOrWhiteSpace(a.version))
                txt.Append($"v{a.version} ");

            if (!string.IsNullOrWhiteSpace(a.engine))
                txt.Append($", made with {a.engine} ");

            if (!string.IsNullOrWhiteSpace(a.engineVersion))
                txt.Append($"v{a.engineVersion} ");

            return txt.ToString().TrimStart(',').Trim();
        };

        public override object Run(CommandArgs args)
        {
            args.CheckArgumentCount(0);

            var appInfo = args.console.Instance?.AppInfo;

            if (appInfo == null)
            {
                args.console.LogError("No version information is supplied.");
                return null;
            }

            args.console.Log(GetInfoString(appInfo));
            return null;
        }
    }
}