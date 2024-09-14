namespace qASIC.Communication.Components
{
    public class CC_Debug : CommsComponent
    {
        public override void Read(CommsComponentArgs args)
        {
            switch (args.packetType)
            {
                case PacketType.Server:
                    args.server.Logs.Log($"Received debug message");
                    break;
                case PacketType.Client:
                    args.client.Logs.Log($"Received debug message");
                    break;
            }
        }
    }
}