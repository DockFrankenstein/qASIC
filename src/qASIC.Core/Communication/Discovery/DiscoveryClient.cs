using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace qASIC.Communication.Discovery
{
    public class DiscoveryClient
    {
        /// <param name="port">Port used for discovery. It has to be the same as on the <see cref="DiscoveryServer"/>!</param>
        public DiscoveryClient(int port = Constants.DEFAULT_DISCOVERY_PORT)
        {
            Port = port;
        }

        private Thread _processThread;
        private Thread _checkThread;
        private CancellationTokenSource _cancel;

        /// <summary>Update frequency in miliseconds.</summary>
        public int UpdateFrequency { get; set; } = 200;
        /// <summary>The maximum amount of missed pings allowed before removing a <see cref="DiscoveredConnection"/> from <see cref="Discovered"/>.</summary>
        public int MaxMissedPings { get; set; } = 3;

        /// <summary>Is the client active?</summary>
        public bool IsActive { get; private set; }

        /// <summary>Port used for discovery. It has to be the same as on the <see cref="DiscoveryServer"/>!</summary>
        public int Port { get; private set; }

        /// <summary>Invoked when a new connection gets discovered.</summary>
        public event Action<DiscoveredConnection> OnDiscover;
        /// <summary>Invoked when a connection gets removed from the list of discovered.</summary>
        public event Action<DiscoveredConnection> OnRemoved;

        /// <summary>Should search for servers using the IPv4 protocol?</summary>
        public bool UseIPv4 { get; set; } = true;
        /// <summary>Should search for servers using the IPv6 protocol?</summary>
        public bool UseIPv6 { get; set; } = false;

        /// <summary>List of discovered connections.</summary>
        public List<DiscoveredConnection> Discovered { get; private set; } = new List<DiscoveredConnection>();

        /// <summary>Changes <see cref="Port"/> when the client is inactive.</summary>
        /// <param name="port">The new port.</param>
        /// <exception cref="Exception">Gets thrown when the client is already active.</exception>
        public void ChangePort(int port)
        {
            if (IsActive)
                throw new Exception("Cannot change port, the discovery client is already active!");

            Port = port;
        }

        /// <summary>Starts searching for connections.</summary>
        /// <exception cref="Exception">Gets thrown when the client is already active.</exception>
        public void Start()
        {
            if (IsActive)
                throw new Exception("Cannot start discovery client, client is already active!");

            IsActive = true;
            Discovered = new List<DiscoveredConnection>();
            _processThread = new Thread(Process);
            _checkThread = new Thread(Check);
            _cancel = new CancellationTokenSource();
            _processThread.Start();
            _checkThread.Start();
        }

        /// <summary>Stops searching for connections.</summary>
        /// <exception cref="Exception">Gets thrown when the client is already inactive.</exception>
        public void Stop()
        {
            if (!IsActive)
                throw new Exception("Cannot stop discovery client, client is already inactive!");

            IsActive = false;
            _cancel.Cancel();
            _processThread.Join();
            _cancel = null;
            _processThread = null;
            _checkThread = null;
        }

        void Process()
        {
            var sockets = new List<Socket>();

            if (Socket.OSSupportsIPv4 && UseIPv4)
                sockets.Add(CreateSocket());

            if (Socket.OSSupportsIPv6 && UseIPv6)
                sockets.Add(CreateSocket(true));

            var buffer = new byte[1024];

            if (sockets.Count == 0)
            {
                Stop();
                return;
            }

            while (!_cancel.IsCancellationRequested)
            {
                var checkRead = sockets.ToList();
                var checkError = new List<Socket>();
                Socket.Select(checkRead, null, checkError, -1);

                foreach (var socket in checkRead)
                {
                    var endpoint = socket.LocalEndPoint;
                    var count = socket.ReceiveFrom(buffer, ref endpoint);

                    var ipEndpoint = (IPEndPoint)endpoint;
                    var identity = new qPacket(buffer);
                    var port = identity.ReadInt();
                    identity.RemoveReadBytes();

                    var alreadyAdded = Discovered.Where(x => x.Identity.bytes.SequenceEqual(identity.bytes) && x.Port == port);

                    foreach (var item in alreadyAdded)
                        item.MissedPings = 0;

                    if (alreadyAdded.Any())
                        continue;

                    var connection = new DiscoveredConnection(ipEndpoint.Address, port, identity);
                    Discovered.Add(connection);
                    OnDiscover?.Invoke(connection);
                }
            }

            foreach (var item in sockets)
                item.Dispose();
        }

        Socket CreateSocket(bool is6 = false)
        {
            var addressFamily = is6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;

            var socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                EnableBroadcast = true,
                ExclusiveAddressUse = false,
            };

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            if (is6)
                socket.SetSocketOption(SocketOptionLevel.IPv6, 
                    SocketOptionName.AddMembership, 
                    new IPv6MulticastOption(IPAddress.Parse("ff02::1")));

            socket.Bind(new IPEndPoint(is6 ? IPAddress.IPv6Any : IPAddress.Any, Port));

            return socket;
        }

        void Check()
        {
            while (!_cancel.Token.WaitHandle.WaitOne(UpdateFrequency))
            {
                var toRemove = new List<DiscoveredConnection>();

                foreach (var item in Discovered)
                {
                    item.MissedPings++;

                    if (item.MissedPings >= MaxMissedPings)
                        toRemove.Add(item);
                }

                foreach (var item in toRemove)
                {
                    Discovered.Remove(item);
                    OnRemoved?.Invoke(item);
                }
            }
        }
    }
}