using System.Text;

namespace qASIC.Options.Commands
{
    public class OptionsList : OptionsCommand
    {
        public OptionsList(OptionsManager manager) : base(manager) { }

        public override string CommandName => "optionslist";
        public override string[] Aliases => new string[] { "settingslist", "listoptions", "listsettings" };

        public override object Run(CommandArgs args)
        {
            args.CheckArgumentCount(0);

            StringBuilder txt = new StringBuilder("List of options:");

            foreach (var item in Manager.OptionsList)
                txt.Append($"\n- {item.Key}:{item.Value.Value} (default: {item.Value.DefaultValue})");

            args.logs.Log(txt.ToString());
            return null;
        }
    }
}