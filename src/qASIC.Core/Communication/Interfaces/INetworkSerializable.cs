namespace qASIC.Communication
{
    public interface INetworkSerializable
    {
        void Read(qPacket packet);
        qPacket Write(qPacket packet);
    }
}