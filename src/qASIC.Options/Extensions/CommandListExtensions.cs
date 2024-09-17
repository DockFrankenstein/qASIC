namespace qASIC.Options
{
    public static class CommandListExtensions
    {
        /// <summary>Adds all built-in Options System commands to the list.</summary>
        /// <param name="list">List to add commands to.</param>
        /// <param name="manager">Manager for which commands are for.</param>
        public static void AddBuiltInOptionsCommands(this ICommandList list, OptionsManager manager)
        {
            list.AddCommand(new Commands.OptionsList(manager));
            list.AddCommand(new Commands.ChangeOption(manager));
        }
    }
}