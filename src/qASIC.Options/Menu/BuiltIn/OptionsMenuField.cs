namespace qASIC.Options.Menu
{
    public class OptionsMenuField<T> : OptionsMenuItem<T>
    {
        public OptionsMenuField(string name, string displayName)
        {
            this.name = name;
            this.displayName = displayName;
        }
    }

    public class OptionsMenuFieldString : OptionsMenuField<string>
    {
        public OptionsMenuFieldString(string name, string displayName) : base(name, displayName) { }
    }

    public class OptionsMenuFieldFloat : OptionsMenuField<float>
    { 
        public OptionsMenuFieldFloat(string name, string displayName) : base(name, displayName) { }
    }

    public class OptionsMenuFieldInt : OptionsMenuField<int>
    { 
        public OptionsMenuFieldInt(string name, string displayName) : base(name, displayName) { }
    }

}