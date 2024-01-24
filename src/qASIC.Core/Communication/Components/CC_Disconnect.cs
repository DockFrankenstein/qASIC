namespace qASIC.Communication.Components
{
    public class CC_Disconnect : CommsComponent
    {
        public override void Read(CommsComponentArgs args)
        {
            switch (args.packetType)
            {
                case PacketType.Server:
                    args.server!.DisconnectClientLocal(args.targetServerClient!);
                    break;
                case PacketType.Client:
                    args.client!.DisconnectLocal(Client.DisconnectReason.ServerShutdown);
                    break;
            }
        }
    }
}