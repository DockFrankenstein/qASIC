namespace qASIC.Communication
{
    public class NetworkServerInfo : INetworkSerializable
    {
        public uint protocolVersion = Constants.PROTOCOL_VERSION;

        public virtual void Read(qPacket packet) =>
            protocolVersion = packet.ReadUInt();

        public virtual qPacket Write(qPacket packet) =>
            packet.Write(protocolVersion);
    }
}