using System.Net;

namespace qASIC.Communication.Discovery
{
    public sealed class OnDiscoveredArgs
    {
        public IPAddress Address { get; set; }
        public int Port { get; set; }
        public qPacket IdentityPacket { get; set; }
    }
}