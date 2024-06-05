using System;

namespace qASIC.Console
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string name) : this(name, null) { }

        public CommandAttribute(string name, params string[] aliases)
        {
            Name = name;
            Aliases = aliases;
        }

        public string Name { get; private set; }
        public string[] Aliases { get; private set; } = null;
        public string Description { get; set; } = null;
        public string DetailedDescription { get; set; } = null;
        public bool UseRegisteredTargets { get; protected set; } = true;
    }
}