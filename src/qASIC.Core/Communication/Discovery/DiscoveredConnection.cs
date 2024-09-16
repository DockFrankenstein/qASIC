using System.Net;

namespace qASIC.Communication.Discovery
{
    public class DiscoveredConnection
    {
        /// <param name="address">Address of the discovered server.</param>
        /// <param name="port">Packet containing identity information of the discovered server.</param>
        /// <param name="identity">Packet containing identity information of the discovered server.</param>
        public DiscoveredConnection(IPAddress address, int port, qPacket identity)
        {
            Address = address;
            Port = port;
            Identity = identity;
        }

        /// <summary>Address of the discovered server.</summary>
        public IPAddress Address { get; set; }
        /// <summary>Port of the discovered server.</summary>
        public int Port { get; set; }
        /// <summary>Packet containing identity information of the discovered server.</summary>
        public qPacket Identity { get; set; }

        /// <summary>Amount of pings missed from the discovered server.</summary>
        public int MissedPings { get; set; }
    }
}