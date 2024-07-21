namespace qASIC.Options.Menu.BuiltIn
{
    public class OptionsMenuDropdown<T> : OptionsMenuItem<T>
    {
        public OptionsMenuDropdown(string name, string displayName, params T[] values)
        {
            this.name = name;
            this.displayName = displayName;
            this.values = values;
        }

        public T[] values;
    }

    public class OptionsMenuDropdownString : OptionsMenuDropdown<string>
    {
        public OptionsMenuDropdownString(string name, string displayName, params string[] values) : base(name, displayName, values) { }
    }

    public class OptionsMenuDropdownFloat : OptionsMenuDropdown<float>
    {
        public OptionsMenuDropdownFloat(string name, string displayName, params float[] values) : base(name, displayName, values) { }
    }

    public class OptionsMenuDropdownInt : OptionsMenuDropdown<int>
    {
        public OptionsMenuDropdownInt(string name, string displayName, params int[] values) : base(name, displayName, values) { }
    }

    public class OptionsMenuDropdownBool : OptionsMenuDropdown<bool>
    {
        public OptionsMenuDropdownBool(string name, string displayName, params bool[] values) : base(name, displayName, values) { }
    }
}