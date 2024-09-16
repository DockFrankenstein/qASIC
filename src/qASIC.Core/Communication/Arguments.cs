using System.Diagnostics;

namespace qASIC.Communication
{
    public class OnServerReceiveDataArgs
    {
        public OnServerReceiveDataArgs(qServer.Client client, byte[] buffer)
        {
            this.client = client;
            data = buffer;
        }

        public qServer.Client client;
        public byte[] data;
    }

    public class CommsComponentArgs
    {
        public CommsComponentArgs(PacketType packetType, qPacket packet)
        {
            this.packet = packet;
            this.packetType = packetType;
        }

        public PacketType packetType;
        public qPacket packet;
        public qClient client;
        public qServer server;

        public qServer.Client targetServerClient;

        public LogManager Logs
        {
            get
            {
                switch (packetType)
                {
                    case PacketType.Server:
                        return server.Logs;
                    case PacketType.Client:
                        return client.Logs;
                    default:
                        return null;
                }
            }
        }
    }
}