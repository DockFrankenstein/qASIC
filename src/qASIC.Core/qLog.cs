using qASIC.Communication;
using System;

namespace qASIC
{
    public enum LogType : byte
    {
        Application,
        User,
        Internal,
        Clear,
    }

    public class qLog : INetworkSerializable
    {
        public qLog() { }

        public qLog(DateTime time, string message) : this(time, message, qDebug.DEFAULT_COLOR_TAG) { }

        public qLog(DateTime time, string message, qColor color) : this(time, message, LogType.Application, color) { }
        public qLog(DateTime time, string message, string colorTag) : this(time, message, LogType.Application, colorTag) { }

        public qLog(DateTime time, string message, LogType logType, qColor color)
        {
            this.time = time;
            this.message = message;
            this.logType = logType;
            this.color = color;
            colorTag = null;
        }

        public qLog(DateTime time, string message, LogType logType, string colorTag)
        {
            this.time = time;
            this.message = message;
            this.logType = logType;
            this.colorTag = colorTag;
        }

        public DateTime time;
        public string message = string.Empty;
        public LogType logType = LogType.Application;
        public string colorTag = qDebug.DEFAULT_COLOR_TAG;
        public qColor color = qColor.White;

        public static qLog CreateNow(string message, qColor color) =>
            new qLog(DateTime.Now, message, color);

        public static qLog CreateNow(string message, string colorTag) =>
            new qLog(DateTime.Now, message, colorTag);

        public static qLog CreateNow(string message, LogType logType, qColor color) =>
            new qLog(DateTime.Now, message, logType, color);

        public static qLog CreateNow(string message, LogType logType, string colorTag) =>
            new qLog(DateTime.Now, message, logType, colorTag);

        public override string ToString() =>
            $"[{time:HH:mm:ss}] [{logType}] {message}";

        public Packet Write(Packet packet) =>
            packet
            .Write(time.Ticks)
            .Write(message)
            .Write((byte)logType)
            .Write(colorTag == null)
            .Write(colorTag ?? string.Empty)
            .Write(color);
        
        public void Read(Packet packet)
        {
            time = new DateTime(packet.ReadLong());
            message = packet.ReadString();
            logType = (LogType)packet.ReadByte();

            bool nullColorTag = packet.ReadBool();
            colorTag = packet.ReadString();
            if (nullColorTag)
                colorTag = null;

            color = packet.ReadNetworkSerializable<qColor>();
        }
    }
}