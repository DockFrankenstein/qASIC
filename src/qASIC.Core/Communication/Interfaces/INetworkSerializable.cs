namespace qASIC.Communication
{
    public interface INetworkSerializable
    {
        void Read(Packet packet);
        Packet Write(Packet packet);
    }
}