namespace qASIC.Options.Menu
{
    public class OptionsMenuToggle : OptionsMenuItem<bool>
    {
        public OptionsMenuToggle(string name, string displayName)
        {
            this.name = name;
            this.displayName = displayName;
        }
    }
}