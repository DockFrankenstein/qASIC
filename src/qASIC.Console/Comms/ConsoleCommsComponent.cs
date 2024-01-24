using qASIC.Communication;
using qASIC.Communication.Components;

namespace qASIC.Console.Comms
{
    public abstract class ConsoleCommsComponent : CommsComponent
    {
        public InstanceConsoleManager? ConsoleManager { get; set; }

        public override void Read(CommsComponentArgs args)
        {
            var consoleName = args.packet.ReadString();
            var console = ConsoleManager?.Get(consoleName)?.console;
            if (console == null)
            {
                args.Log("[Error] Console not registered");
                return;
            }

            ReadForConsole(args, console);
        }

        public abstract void ReadForConsole(CommsComponentArgs args, GameConsole console);

        public Packet CreateEmptyPacketForConsole(GameConsole console) =>
            CreateEmptyComponentPacket()
            .Write(console.Name);
    }
}
