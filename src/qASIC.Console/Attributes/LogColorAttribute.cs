namespace qASIC.Console
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method)]
    public class LogColorAttribute : Attribute
    {
        public LogColorAttribute(string colorTag)
        {
            ColorTag = colorTag;
        }

        public LogColorAttribute(GenericColor color)
        {
            Color = Color.GetGenericColor(color);
        }

        public LogColorAttribute(byte red, byte green, byte blue) : this(red, green, blue, 0) { }
        public LogColorAttribute(byte red, byte green, byte blue, byte alpha)
        {
            Color = new Color(red, green, blue, alpha);
        }

        public string? ColorTag { get; init; } = null;
        public Color Color { get; init; }
    }
}