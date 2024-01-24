namespace qASIC.Communication.Components
{
    public class CC_Debug : CommsComponent
    {
        public override void Read(CommsComponentArgs args)
        {
            switch (args.packetType)
            {
                case PacketType.Server:
                    args.server!.OnLog?.Invoke($"Received debug message");
                    break;
                case PacketType.Client:
                    args.client!.OnLog?.Invoke($"Received debug message");
                    break;
            }
        }
    }
}