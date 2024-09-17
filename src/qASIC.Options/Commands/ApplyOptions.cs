namespace qASIC.Options.Commands
{
    public class ApplyOptions : OptionsCommand
    {
        public ApplyOptions(OptionsManager manager) : base(manager) { }

        public override string CommandName => "applyoptions";
        public override string[] Aliases => new string[] { "applysettings", "optionsapply", "settingsapply" };
        public override string Description => "Saves options to disk.";

        public override object Run(CommandArgs args)
        {
            args.CheckArgumentCount(0);
            Manager.Apply();
            return null;
        }
    }
}
