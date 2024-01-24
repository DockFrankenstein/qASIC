using qASIC.Communication;

namespace qASIC.Console
{
    public class GameConsoleTheme : INetworkSerializable
    {
        public static GameConsoleTheme Default =>
            new GameConsoleTheme();

        public Color defaultColor = Color.White;
        public Color warningColor = Color.Yellow;
        public Color errorColor = Color.Red;

        public Dictionary<string, Color> customColors = new Dictionary<string, Color>();

        public Color GetLogColor(Log log)
        {
            switch (log.colorTag)
            {
                case null:
                    return log.color;
                case qDebug.DEFAULT_COLOR_TAG:
                    return defaultColor;
                case qDebug.WARNING_COLOR_TAG:
                    return warningColor;
                case qDebug.ERROR_COLOR_TAG:
                    return errorColor;
                default:
                    if (customColors.ContainsKey(log.colorTag))
                        return customColors[log.colorTag];

                    break;
            }

            return defaultColor;
        }

        public void Read(Packet packet)
        {
            customColors.Clear();
            int colorCount = packet.ReadInt();
            for (int i = 0; i < colorCount; i++)
                customColors.Add(packet.ReadString(), packet.ReadNetworkSerializable<Color>());

            defaultColor = packet.ReadNetworkSerializable<Color>();
            warningColor = packet.ReadNetworkSerializable<Color>();
            errorColor = packet.ReadNetworkSerializable<Color>();
        }

        public Packet Write(Packet packet)
        {
            packet = packet
                .Write(customColors.Count);

            foreach (var item in customColors)
            {
                packet.Write(item.Key);
                packet.Write(item.Value);
            }

            packet = packet.Write(defaultColor)
                .Write(warningColor)
                .Write(errorColor);

            return packet;
        }
    }
}