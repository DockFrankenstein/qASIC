namespace qASIC.Options.Commands
{
    public abstract class OptionsCommand : ICommand
    {
        public OptionsCommand(OptionsManager manager)
        {
            Manager = manager;
        }

        protected OptionsManager Manager { get; private set; }

        public abstract string CommandName { get; }

        public virtual string[] Aliases { get; }

        public virtual string Description { get; }

        public virtual string DetailedDescription { get; }

        public abstract object Run(CommandArgs args);
    }
}