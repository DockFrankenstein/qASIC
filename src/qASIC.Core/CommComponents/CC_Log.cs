using qASIC.Communication;
using qASIC.Communication.Components;

namespace qASIC.CommComponents
{
    public class CC_Log : CommsComponent
    {
        public event Action<Log, PacketType>? OnReceiveLog;

        public override void Read(CommsComponentArgs args)
        {
            var log = args.packet.ReadNetworkSerializable<Log>();
            OnReceiveLog?.Invoke(log, args.packetType);
        }

        public static Packet BuildLogPacket(Log log) =>
            new CC_Log().CreateEmptyComponentPacket()
            .Write(log);
    }
}