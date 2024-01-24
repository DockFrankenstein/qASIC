namespace qASIC.Communication.Components
{
    public class CC_ConnectData : CommsComponent
    {
        public event Action<NetworkServerInfo>? OnRead;

        public override void Read(CommsComponentArgs args)
        {
            switch (args.packetType)
            {
                case PacketType.Server:
                    args.server!.Send(args.targetServerClient!, CreateServerResponsePacket(args.server!));
                    break;
                case PacketType.Client:
                    var info = args.client!.ProcessAppInfo == null ?
                        (NetworkServerInfo)args.packet.ReadNetworkSerializable(args.client!.AppInfo) :
                        args.client!.ProcessAppInfo(args.packet);

                    if (info.protocolVersion > Constants.PROTOCOL_VERSION)
                    {
                        args.client?.OnLog?.Invoke($"Server uses a newer version of the communication protocol that is currently unsupported by this application. Please update communication library version to latest!");
                        args.client?.Disconnect();
                        return;
                    }

                    args.client?.OnLog?.Invoke($"Connected to project using protocol version: {info.protocolVersion}");
                    args.client!.AppInfo = info;
                    args.client!.CurrentState = Client.State.Connected;
                    args.client!.receivedPing = true;
                    args.client!.OnLog?.Invoke("Client connected");
                    args.client!.OnConnect?.Invoke();

                    OnRead?.Invoke(info);
                    break;
            }
        }

        public static Packet CreateClientConfirmationPacket() =>
            new CC_ConnectData().CreateEmptyComponentPacket();

        public static Packet CreateServerResponsePacket(Server server) =>
            new CC_ConnectData().CreateEmptyComponentPacket()
            .Write(server.AppInfo);
    }
}