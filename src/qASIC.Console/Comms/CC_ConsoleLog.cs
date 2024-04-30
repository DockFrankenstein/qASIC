using qASIC.Communication.Components;
using qASIC.Communication;

namespace qASIC.Console.Comms
{
    public class CC_ConsoleLog : ConsoleCommsComponent
    {
        public event Action<GameConsole, qLog>? OnRead;

        public override void ReadForConsole(CommsComponentArgs args, GameConsole console)
        {
            if (args.packetType != PacketType.Client)
                return;

            var log = args.packet.ReadNetworkSerializable<qLog>();
            console.Logs.Add(log);
            OnRead?.Invoke(console, log);
        }

        public static Packet BuildPacket(GameConsole console, qLog log) =>
            new CC_ConsoleLog().CreateEmptyPacketForConsole(console)
            .Write(log);
    }
}