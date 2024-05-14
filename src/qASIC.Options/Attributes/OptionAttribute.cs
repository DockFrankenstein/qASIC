using System;

namespace qASIC.Options
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
    public class OptionAttribute : Attribute
    {
        public OptionAttribute() : this(null) { }

        public OptionAttribute(string name)
        {
            Name = name;
        }

        public OptionAttribute(string name, object defaultValue) : this(name)
        {
            HasDefaultValue = true;
            DefaultValue = defaultValue;
        }

        public OptionAttribute(object defaultValue) : this(null, defaultValue) { }

        public string Name { get; private set; }

        public bool HasDefaultValue { get; private set; }
        public object DefaultValue { get; private set; }
    }
}