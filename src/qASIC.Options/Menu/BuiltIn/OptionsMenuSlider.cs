namespace qASIC.Options.Menu
{
    public class OptionsMenuSlider<T> : OptionsMenuItem<T>
    {
        public OptionsMenuSlider(string name, string displayName, T minValue, T maxValue)
        {
            this.name = name;
            this.displayName = displayName;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public T minValue;
        public T maxValue;
    }

    public class OptionsMenuSliderFloat : OptionsMenuSlider<float> 
    { 
        public OptionsMenuSliderFloat(string name, string displayName, float minValue, float maxValue) : base(name, displayName, minValue, maxValue) { }
    }

    public class OptionsMenuSliderInt : OptionsMenuSlider<int> 
    {
        public OptionsMenuSliderInt(string name, string displayName, int minValue, int maxValue) : base(name, displayName, minValue, maxValue) { }
    }
}