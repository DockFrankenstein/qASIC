using qASIC.Communication.Components;

namespace qASIC.Communication
{
    public interface IPeer
    {
        void Send(qPacket packet);
        CommsComponentCollection Components { get; }
    }
}