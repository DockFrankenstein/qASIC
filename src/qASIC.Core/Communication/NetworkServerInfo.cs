namespace qASIC.Communication
{
    public class NetworkServerInfo : INetworkSerializable
    {
        public uint protocolVersion = Constants.PROTOCOL_VERSION;

        public virtual void Read(Packet packet) =>
            protocolVersion = packet.ReadUInt();

        public virtual Packet Write(Packet packet) =>
            packet.Write(protocolVersion);
    }
}