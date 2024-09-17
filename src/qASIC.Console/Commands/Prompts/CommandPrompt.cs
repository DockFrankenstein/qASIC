namespace qASIC.Console.Commands.Prompts
{
    public abstract class CommandPrompt
    {
        public virtual bool CanExecute(GameCommandArgs args) =>
            true;

        public virtual bool ParseArguments => false;

        public abstract ConsoleArgument[] Prepare(GameCommandArgs args);
    }
}