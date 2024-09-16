using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System;

namespace qASIC.Communication.Discovery
{
    public class DiscoveryServer
    {
        /// <param name="targetServer">Server to be broadcasted.</param>
        /// <param name="port">Port used for discovery. It has to be the same as on the <see cref="DiscoveryClient"/>!</param>
        public DiscoveryServer(qServer targetServer, int port = Constants.DEFAULT_DISCOVERY_PORT)
        {
            TargetServer = targetServer;
            Port = port;
        }

        private Thread _thread;
        private CancellationTokenSource _cancel;

        /// <summary>Update frequency in miliseconds.</summary>
        public int UpdateFrequency { get; set; } = 200;

        /// <summary>Is the server active?</summary>
        public bool IsActive { get; private set; }

        /// <summary>Port used for discovery. It has to be the same as on the <see cref="DiscoveryClient"/>!</summary>
        public int Port { get; private set; }
        /// <param name="targetServer">Server to be broadcasted.</param>
        public qServer TargetServer { get; private set; }

        /// <summary>Should broadcast using the IPv4 protocol?</summary>
        public bool UseIPv4 { get; set; } = true;
        /// <summary>Should broadcast using the IPv6 protocol?</summary>
        public bool UseIPv6 { get; set; } = false;

        /// <summary>Changes <see cref="Port"/> when the server is inactive.</summary>
        /// <param name="port">The new port.</param>
        /// <exception cref="Exception">Gets thrown when the server is already active.</exception>
        public void ChangePort(int port)
        {
            if (IsActive)
                throw new Exception("Cannot change port, the discovery server is already active!");

            Port = port;
        }

        /// <summary>Starts broadcasting server information.</summary>
        /// <exception cref="Exception">Gets thrown when the server is already active.</exception>
        public void Start()
        {
            if (IsActive)
                throw new Exception("Cannot start discovery server, server is already active!");

            IsActive = true;
            _thread = new Thread(Process);
            _cancel = new CancellationTokenSource();
            _thread.Start();
        }

        /// <summary>Stops broadcasting server information.</summary>
        /// <exception cref="Exception">Gets thrown when the server is already inactive.</exception>
        public void Stop()
        {
            if (!IsActive)
                throw new Exception("Cannot stop discovery server, server is already inactive!");

            IsActive = false;
            _cancel.Cancel();
            _thread.Join();
            _cancel = null;
            _thread = null;
        }

        void Process()
        {
            var sockets = new Dictionary<IPAddress, Socket>();
            var ip6link = IPAddress.Parse("ff02::1");

            var endPoint4 = new IPEndPoint(IPAddress.Broadcast, Port);
            var endPoint6 = new IPEndPoint(ip6link, Port);

            while (!_cancel.Token.WaitHandle.WaitOne(UpdateFrequency))
            {
                var addresses = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(x => x.OperationalStatus == OperationalStatus.Up || x.OperationalStatus == OperationalStatus.Unknown)
                    .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                    .Select(x => x.Address)
                    .Where(x => (UseIPv4 && x.AddressFamily == AddressFamily.InterNetwork) || 
                        (UseIPv6 && x.AddressFamily == AddressFamily.InterNetworkV6));

                var added = addresses.Except(sockets.Select(x => x.Key));
                var removed = sockets.Select(x => x.Key).Except(addresses);

                foreach (var item in removed)
                {
                    sockets[item].Dispose();
                    sockets.Remove(item);
                }

                foreach (var item in added)
                {
                    var is6 = item.AddressFamily == AddressFamily.InterNetworkV6;

                    var socket = new Socket(item.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
                    {
                        EnableBroadcast = true,
                        ExclusiveAddressUse = false,
                    };

                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                    if (is6)
                        socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(ip6link));

                    socket.Bind(new IPEndPoint(item, Port));

                    sockets.Add(item, socket);
                }

                var identity = new qPacket()
                    .Write(TargetServer.Port)
                    .Write(TargetServer.AppInfo)
                    .ToArray();

                foreach (var item in sockets)
                {
                    try
                    {
                        var is6 = item.Key.AddressFamily == AddressFamily.InterNetworkV6;
                        item.Value.SendTo(identity, is6 ? endPoint6 : endPoint4);
                    }
                    catch { }
                }
            }

            foreach (var item in sockets.Values)
                item.Dispose();
        }
    }
}