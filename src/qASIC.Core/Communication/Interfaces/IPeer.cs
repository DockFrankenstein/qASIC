using qASIC.Communication.Components;

namespace qASIC.Communication
{
    public interface IPeer
    {
        void Send(Packet packet);
        CommsComponentCollection Components { get; }
    }
}