using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using qASIC.Communication.Components;

namespace qASIC.Communication
{
    public class qClient : IPeer
    {
        public qClient(CommsComponentCollection components, int maxConnectionAttempts = 5) : 
            this(components, IPAddress.Parse("127.0.0.1"), Constants.DEFAULT_PORT, maxConnectionAttempts)
        { }

        public qClient(CommsComponentCollection components, IPAddress address, int port, int maxConnectionAttempts = 5)
        {
            Components = components;

            Address = address;
            Port = port;

            this.maxConnectionAttempts = maxConnectionAttempts;
        }

        public enum DisconnectReason
        {
            /// <summary>Client disconnected voluntarily</summary>
            None,
            /// <summary>Server was shut down</summary>
            ServerShutdown,
            /// <summary>Couldn't connect to the server in time</summary>
            FailedToEstablishConnection,
            /// <summary>Couldn't receive network server info in time</summary>
            FailedToReceiveConnectionInformation,
            /// <summary>Client didn't receive a pong signal in time</summary>
            NoResponse,
            /// <summary>General error</summary>
            Error,
        }

        public enum State
        {
            Offline,
            Connecting,
            Pending,
            Connected,
        }

        public CommsComponentCollection Components { get; private set; }
        public NetworkServerInfo AppInfo { get; set; } = new NetworkServerInfo();

        public IPAddress Address { get; private set; }
        public int Port { get; private set; }
        public State CurrentState { get; internal set; } = State.Offline;
        public bool IsActive => CurrentState != State.Offline;

        public int maxConnectionAttempts;
        private int connectionAttempts = 0;

        public TcpClient Socket { get; private set; }
        public NetworkStream Stream { get; private set; }


        public Action<string> OnLog;
        public Action OnStart;
        public Action OnConnect;
        public Action<DisconnectReason> OnDisconnect;
        public Func<qPacket, NetworkServerInfo> ProcessAppInfo = null;

        private byte[] buffer = new byte[0];
        private System.Diagnostics.Stopwatch time = new System.Diagnostics.Stopwatch();
        private long currentTime;
        internal bool receivedPing;

        public bool logPacketSend = false;

        qPriorityQueue<KeyValuePair<Action, long>, long> eventQueue = new qPriorityQueue<KeyValuePair<Action, long>, long>();

        public void Update()
        {
            if (!IsActive) return;

            currentTime = time.ElapsedMilliseconds;

            while (eventQueue.Count > 0 && eventQueue.Peek().Value <= currentTime)
            {
                try
                {
                    eventQueue.Dequeue().Key.Invoke();
                }
                catch (Exception e)
                {
                    OnLog?.Invoke($"[Error] There was a problem in update loop, {e}");
                }
            }
        }

        public void Connect() =>
            Connect(Address, Port);

        public void Connect(IPAddress address, int port)
        {
            if (IsActive)
                throw new Exception("Cannot connect client, client is already active!");

            Address = address;
            Port = port;

            OnStart?.Invoke();
            OnLog?.Invoke("Starting client...");

            try
            {
                currentTime = 0;
                connectionAttempts = 0;
                time.Restart();

                Socket = new TcpClient()
                {
                    ReceiveBufferSize = Constants.RECEIVE_BUFFER_SIZE,
                    SendBufferSize = Constants.SEND_BUFFER_SIZE,
                    NoDelay = false,
                };

                buffer = new byte[Socket.ReceiveBufferSize];
                IAsyncResult result = Socket.BeginConnect(Address, Port, null, null);

                CurrentState = State.Connecting;
                OnLog?.Invoke("Client is active, connecting...");
                Heartbeat(result);

            }
            catch (Exception e)
            {
                OnLog?.Invoke($"[Error] Failed to connect client: {e}");
                Disconnect(DisconnectReason.Error);
            }
        }

        void Heartbeat(IAsyncResult result)
        {
            if (!IsActive)
                return;

            try
            {
                switch (CurrentState)
                {
                    case State.Connecting:
                        if (Socket!.Connected)
                        {
                            Socket.EndConnect(result);
                            Stream = Socket.GetStream();
                            Stream.BeginRead(buffer, 0, Socket.ReceiveBufferSize, OnDataReceived, null);

                            Send(CC_ConnectData.CreateClientConfirmationPacket());

                            CurrentState = State.Pending;
                            OnLog?.Invoke($"Connection established, waiting for connection confirmation");
                            break;
                        }

                        if (connectionAttempts >= maxConnectionAttempts)
                        {
                            OnLog?.Invoke($"Couldn't establish connection");
                            DisconnectLocal(DisconnectReason.FailedToEstablishConnection);
                            return;
                        }

                        OnLog?.Invoke($"Connection attempt: {connectionAttempts}");
                        connectionAttempts++;
                        break;
                    case State.Pending:
                        if (connectionAttempts >= maxConnectionAttempts)
                        {
                            OnLog?.Invoke($"Failed to receive connection confirmation.");
                            Disconnect(DisconnectReason.FailedToReceiveConnectionInformation);
                            return;
                        }

                        connectionAttempts++;
                        break;
                    case State.Connected:
                        if (!receivedPing)
                        {
                            OnLog?.Invoke($"Server didn't respond, disconnecting...");
                            DisconnectLocal(DisconnectReason.NoResponse);
                            return;
                        }

                        receivedPing = false;
                        Send(new CC_Ping().CreateEmptyComponentPacket());
                        break;
                }
            }
            catch (Exception e)
            {
                OnLog?.Invoke($"[Error] Failed to execute update loop: {e}");
            }

            ExecuteLater(1000, () => Heartbeat(result));
        }

        void OnDataReceived(IAsyncResult result)
        {
            if (!IsActive) return;

            try
            {
                if (Stream?.CanRead != true)
                {
                    OnLog?.Invoke("[Error] Stream couldn't be read");
                    return;
                }

                int length = Stream.EndRead(result);

                if (length <= 0)
                {
                    OnLog?.Invoke("[Error] Stream is empty");
                    return;
                }

                byte[] temp = new byte[length];
                Array.Copy(buffer, temp, length);

                var packet = new qPacket(temp);

                Components.HandlePacketForClient(this, packet);

                if (IsActive)
                    Stream.BeginRead(buffer, 0, Socket!.ReceiveBufferSize, OnDataReceived, null);
            }
            catch (Exception e)
            {
                OnLog?.Invoke($"[Error] There was an error while processing data: {e}");
            }
        }

        public void Send(qPacket packet)
        {
            try
            {
                if (logPacketSend)
                    OnLog?.Invoke($"Sending to server - {packet}");

                if (Stream?.CanWrite != true)
                    return;

                var data = packet.ToArray();
                Stream?.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                OnLog?.Invoke($"[Error] There was a problem while sending: {e}");
            }
        }

        public void Disconnect(DisconnectReason reason = DisconnectReason.None)
        {
            Send(new CC_Disconnect().CreateEmptyComponentPacket());
            DisconnectLocal(reason);
        }

        public void DisconnectLocal(DisconnectReason reason = DisconnectReason.None)
        {
            CurrentState = State.Offline;

            try
            {
                Stream?.Close();
                Socket?.Close();
                time.Stop();
                eventQueue.Clear();
                currentTime = 0;

                OnLog?.Invoke("Client disconnected");
                OnDisconnect?.Invoke(reason);
            }
            catch (Exception e)
            {
                OnLog?.Invoke($"[Error] There was a problem while disconnecting. Please restart application! {e}");
            }
        }

        internal void ExecuteLater(long inMs, Action delayedAction)
        {
            var t = currentTime + inMs;
            eventQueue.Enqueue(new KeyValuePair<Action, long>(delayedAction, t), t);
        }
    }
}