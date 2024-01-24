using qASIC.Communication;

namespace qASIC
{
    public enum GenericColor
    {
        Clear,
        Black,
        White,
        Red,
        Green,
        Yellow,
        DarkBlue,
        Blue,
        Purple,
    }

    [Serializable]
    public struct Color : INetworkSerializable
    {
        public Color(byte red, byte green, byte blue) : this(red, green, blue, 255) { }

        public Color(byte red, byte green, byte blue, byte alpha)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
            this.alpha = alpha;
        }

        public static Color Clear => new Color(0, 0, 0, 0);
        public static Color Black => new Color(0, 0, 0);
        public static Color White => new Color(255, 255, 255);
        public static Color Red => new Color(255, 0, 0);
        public static Color Green => new Color(0, 255, 0);
        public static Color Yellow => new Color(255, 255, 0);
        public static Color DarkBlue => new Color(0, 0, 255);
        public static Color Blue => new Color(0, 255, 255);
        public static Color Purple => new Color(255, 0, 255);

        public byte red;
        public byte green;
        public byte blue;
        public byte alpha;

        public static Color GetGenericColor(GenericColor color) =>
            color switch
            {
                GenericColor.Clear => Clear,
                GenericColor.Black => Black,
                GenericColor.White => White,
                GenericColor.Red => Red,
                GenericColor.Green => Green,
                GenericColor.Yellow => Yellow,
                GenericColor.DarkBlue => DarkBlue,
                GenericColor.Blue => Blue,
                GenericColor.Purple => Purple,
                _ => Clear,
            };

        public void Read(Packet packet)
        {
            red = packet.ReadByte();
            green = packet.ReadByte();
            blue = packet.ReadByte();
            alpha = packet.ReadByte();
        }

        public Packet Write(Packet packet) =>
            packet
            .Write(red)
            .Write(green)
            .Write(blue)
            .Write(alpha);

        public static bool operator ==(Color? a, Color? b) =>
            a?.Equals(b) ?? (a is null && b is null);

        public static bool operator !=(Color? left, Color? right) =>
            !(left == right);

        public override bool Equals(object? obj)
        {
            if (obj is not Color color)
                return false;

            return red == color.red &&
                green == color.green &&
                blue == color.blue &&
                alpha == color.alpha;
        }

        public override string ToString() =>
            $"Color({red}, {green}, {blue}, {alpha})";

        public override int GetHashCode() =>
            ToString().GetHashCode();
    }
}