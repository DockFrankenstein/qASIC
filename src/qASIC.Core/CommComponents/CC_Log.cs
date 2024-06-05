using qASIC.Communication;
using qASIC.Communication.Components;
using System;

namespace qASIC.CommComponents
{
    public class CC_Log : CommsComponent
    {
        public event Action<qLog, PacketType> OnReceiveLog;

        public override void Read(CommsComponentArgs args)
        {
            var log = args.packet.ReadNetworkSerializable<qLog>();
            OnReceiveLog?.Invoke(log, args.packetType);
        }

        public static qPacket BuildLogPacket(qLog log) =>
            new CC_Log().CreateEmptyComponentPacket()
            .Write(log);
    }
}