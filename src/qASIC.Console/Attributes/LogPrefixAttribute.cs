namespace qASIC.Console
{
    /// <summary>Attribute for adding prefixes to logs.</summary>
    /// <example>If a class that has a [LogPrefix("Settings")] attribute logs "Loaded settings" will show up as "[Settings] Loaded settings" in the console.</example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
    public class LogPrefixAttribute : Attribute
    {
        public LogPrefixAttribute(string prefix)
        {
            Prefix = prefix;
        }

        public string Prefix { get; init; }

        /// <summary>Determines if the prefix should be applied.</summary>
        public virtual bool ValidPrefix =>
            !string.IsNullOrEmpty(Prefix);

        /// <summary>Method used for applying prefix to a message.</summary>
        /// <param name="message">Logged message.</param>
        /// <returns>Returns the formatted message.</returns>
        public virtual string FormatMessage(string message) =>
            $"[{Prefix}] {message}";
    }
}