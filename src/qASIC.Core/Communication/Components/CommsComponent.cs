namespace qASIC.Communication.Components
{
    public abstract class CommsComponent
    {
        public virtual string GetId() =>
            GetType().Name;

        public abstract void Read(CommsComponentArgs args);

        public qPacket CreateEmptyComponentPacket() =>
            new qPacket().Write(GetId());
    }
}