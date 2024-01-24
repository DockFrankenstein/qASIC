using qASIC.Communication;

namespace qASIC.Console.Comms
{
    public class CC_ConsoleRegister : ConsoleCommsComponent
    {
        public override void Read(CommsComponentArgs args)
        {
            if (args.packetType != PacketType.Client) return;
            var consoleName = args.packet.ReadString();

            var logs = new List<Log>();
            int logCount = args.packet.ReadInt();
            for (int i = 0; i < logCount; i++)
                logs.Add(args.packet.ReadNetworkSerializable<Log>());

            var console = new GameConsole(consoleName)
            {
                Logs = logs,
                Theme = args.packet.ReadNetworkSerializable<GameConsoleTheme>(),
            };

            ConsoleManager?.RegisterConsole(console);
        }

        public override void ReadForConsole(CommsComponentArgs args, GameConsole console) =>
            throw new NotImplementedException();

        public static Packet CreatePacket(GameConsole console)
        {
            var packet = new CC_ConsoleRegister().CreateEmptyPacketForConsole(console);

            packet.Write(console.Logs.Count);

            foreach (var log in console.Logs)
                packet.Write(log);

            packet.Write(console.Theme);

            return packet;
        }
    }
}
