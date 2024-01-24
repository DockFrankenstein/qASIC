using qASIC.Communication.Components;
using qASIC.Communication;

namespace qASIC.Console.Comms
{
    public class CC_ExecuteCommand : ConsoleCommsComponent
    {
        public override void ReadForConsole(CommsComponentArgs args, GameConsole console)
        {
            if (args.packetType != PacketType.Server)
                return;

            console.Execute(args.packet.ReadString());
        }

        public static Packet BuildPacket(GameConsole console, string input) =>
            new CC_ExecuteCommand().CreateEmptyPacketForConsole(console)
            .Write(input);
    }
}