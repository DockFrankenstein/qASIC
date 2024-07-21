using System;

namespace qASIC.Options.Menu
{
    [Serializable]
    public abstract class OptionsMenuItem
    {
        public string name;
        public string displayName;

        public abstract Type ValueType { get; }
    }

    public abstract class OptionsMenuItem<T> : OptionsMenuItem
    {
        public override Type ValueType => typeof(T);
    }
}
