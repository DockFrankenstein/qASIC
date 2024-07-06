using qASIC.Communication;
using System;

namespace qASIC.Console.Comms
{
    public class CC_ConsoleLog : ConsoleCommsComponent
    {
        public event Action<GameConsole, qLog> OnRead;

        public override void ReadForConsole(CommsComponentArgs args, GameConsole console)
        {
            if (args.packetType != PacketType.Client)
                return;

            var log = args.packet.ReadNetworkSerializable<qLog>();

            if (args.packet.HasBytesFor(sizeof(int)))
            {
                var index = args.packet.ReadInt();
                if (console.Logs.IndexInRange(index))
                {
                    log = console.Logs[index].GetDataFromOther(log);
                    OnRead?.Invoke(console, log);
                    return;
                }
            }

            console.Logs.Add(log);
            OnRead?.Invoke(console, log);
        }

        public static qPacket BuildPacket(GameConsole console, qLog log, bool updatingLog)
        {
            var packet = new CC_ConsoleLog().CreateEmptyPacketForConsole(console)
                .Write(log);

            var index = console.Logs.IndexOf(log);
            if (updatingLog && index != -1)
                packet.Write(index);

            return packet;
        }
    }
}