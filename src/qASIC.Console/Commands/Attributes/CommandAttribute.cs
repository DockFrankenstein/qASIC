namespace qASIC.Console
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; init; }
        public bool UseRegisteredTargets { get; init; } = true;
    }
}