namespace qASIC.Options
{
    public class OptionReference<T>
    {
        public OptionReference(OptionsManager manager, string optionName)
        {
            this.manager = manager;
            OptionName = optionName;
        }

        OptionsManager manager;

        public string OptionName { get; private set; }

        public T Value
        {
            get => manager.GetOption<T>(OptionName);
            set => manager.SetOption(OptionName, value);
        }

        public static explicit operator T(OptionReference<T> reference) =>
            reference.Value;
    }
}