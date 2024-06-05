using qASIC.Communication;
using System.Collections.Generic;

namespace qASIC.Console
{
    public class GameConsoleTheme : INetworkSerializable
    {
        public static GameConsoleTheme Default =>
            new GameConsoleTheme();

        public qColor defaultColor = qColor.White;
        public qColor warningColor = qColor.Yellow;
        public qColor errorColor = qColor.Red;

        public Dictionary<string, qColor> customColors = new Dictionary<string, qColor>()
        {
            ["settings"] = new qColor(0, 0, 255),
            ["settings_set"] = new qColor(0, 0, 255),
            ["settings_set_multiple"] = new qColor(0, 0, 255),
            ["settings_ensure_targets"] = new qColor(0, 0, 255),
        };

        public qColor this[string s]
        {
            get
            {
                switch (s)
                {
                    case qDebug.DEFAULT_COLOR_TAG:
                        return defaultColor;
                    case qDebug.WARNING_COLOR_TAG:
                        return warningColor;
                    case qDebug.ERROR_COLOR_TAG:
                        return errorColor;
                    default:
                        return customColors.TryGetValue(s, out var cl) ? cl : defaultColor;
                }
            }
            set
            {
                if (customColors.ContainsKey(s))
                {
                    customColors[s] = value;
                    return;
                }

                customColors.Add(s, value);
            }
        }

        public qColor GetLogColor(qLog log)
        {
            if (log.colorTag == null)
                return log.color;

            return this[log.colorTag];
        }

        public void Read(qPacket packet)
        {
            customColors.Clear();
            int colorCount = packet.ReadInt();
            for (int i = 0; i < colorCount; i++)
                customColors.Add(packet.ReadString(), packet.ReadNetworkSerializable<qColor>());

            defaultColor = packet.ReadNetworkSerializable<qColor>();
            warningColor = packet.ReadNetworkSerializable<qColor>();
            errorColor = packet.ReadNetworkSerializable<qColor>();
        }

        public qPacket Write(qPacket packet)
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