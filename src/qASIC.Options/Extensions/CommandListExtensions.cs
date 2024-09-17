namespace qASIC.Options
{
    public static class CommandListExtensions
    {
        /// <summary>Adds all built-in Options System commands to the list.</summary>
        /// <param name="list">List to add commands to.</param>
        /// <param name="manager">Manager for which commands are for.</param>
        public static ICommandList AddBuiltInOptionsCommands(this ICommandList list, OptionsManager manager) =>
            list.AddCommand(new Commands.OptionsList(manager))
                .AddCommand(new Commands.ChangeOption(manager))
                .AddCommand(new Commands.ApplyOptions(manager))
                .AddCommand(new Commands.RevertOptions(manager));
    }
}