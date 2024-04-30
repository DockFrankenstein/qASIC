using qASIC.Communication;

namespace qASIC.Console
{
    public class GameConsoleTheme : INetworkSerializable
    {
        public static GameConsoleTheme Default =>
            new GameConsoleTheme();

        public qColor defaultColor = qColor.White;
        public qColor warningColor = qColor.Yellow;
        public qColor errorColor = qColor.Red;

        public Dictionary<string, qColor> customColors = new Dictionary<string, qColor>();

        public qColor GetLogColor(qLog log)
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
                customColors.Add(packet.ReadString(), packet.ReadNetworkSerializable<qColor>());

            defaultColor = packet.ReadNetworkSerializable<qColor>();
            warningColor = packet.ReadNetworkSerializable<qColor>();
            errorColor = packet.ReadNetworkSerializable<qColor>();
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