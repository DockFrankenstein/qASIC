using System;

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
            Color = qColor.GetGenericColor(color);
        }

        public LogColorAttribute(byte red, byte green, byte blue) : this(red, green, blue, 0) { }
        public LogColorAttribute(byte red, byte green, byte blue, byte alpha)
        {
            Color = new qColor(red, green, blue, alpha);
        }

        public string ColorTag { get; private set; } = null;
        public qColor Color { get; private set; }
    }
}