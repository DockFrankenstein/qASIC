using qASIC.Communication;
using System;

using SysColor = System.Drawing.Color;

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
    public struct qColor : INetworkSerializable
    {
        public qColor(byte red, byte green, byte blue) : this(red, green, blue, 255) { }

        public qColor(byte red, byte green, byte blue, byte alpha)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
            this.alpha = alpha;
        }

        public static qColor Clear => new qColor(0, 0, 0, 0);
        public static qColor Black => new qColor(0, 0, 0);
        public static qColor White => new qColor(255, 255, 255);
        public static qColor Red => new qColor(255, 0, 0);
        public static qColor Green => new qColor(0, 255, 0);
        public static qColor Yellow => new qColor(255, 255, 0);
        public static qColor DarkBlue => new qColor(0, 0, 255);
        public static qColor Blue => new qColor(0, 255, 255);
        public static qColor Purple => new qColor(255, 0, 255);

        public byte red;
        public byte green;
        public byte blue;
        public byte alpha;

        public static qColor GetGenericColor(GenericColor color) =>
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

        public void Read(qPacket packet)
        {
            red = packet.ReadByte();
            green = packet.ReadByte();
            blue = packet.ReadByte();
            alpha = packet.ReadByte();
        }

        public qPacket Write(qPacket packet) =>
            packet
            .Write(red)
            .Write(green)
            .Write(blue)
            .Write(alpha);

        public static bool operator ==(qColor? a, qColor? b) =>
            a?.Equals(b) ?? (a is null && b is null);

        public static bool operator !=(qColor? left, qColor? right) =>
            !(left == right);

        public override bool Equals(object obj)
        {
            if (!(obj is qColor color))
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

        public SysColor ToSystem() =>
            SysColor.FromArgb(alpha, red, green, blue);
    }
}