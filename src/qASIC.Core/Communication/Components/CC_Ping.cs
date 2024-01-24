namespace qASIC.Communication.Components
{
    public class CC_Ping : CommsComponent
    {
        public override void Read(CommsComponentArgs args)
        {
            switch (args.packetType)
            {
                case PacketType.Server:
                    args.server!.Send(args.targetServerClient!, CreateEmptyComponentPacket());
                    break;
                case PacketType.Client:
                    args.client!.receivedPing = true;
                    break;
            }
        }
    }
}