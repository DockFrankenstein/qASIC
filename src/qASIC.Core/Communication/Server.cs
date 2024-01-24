using System.Net;
using System.Net.Sockets;
using qASIC.Communication.Components;

namespace qASIC.Communication
{
    public class Server : IPeer
    {
        public Server(CommsComponentCollection components, int port)
        {
            Components = components;

            Port = port;

            Listener = new TcpListener(IPAddress.Loopback, Port);
        }

        public CommsComponentCollection Components { get; private set; }
        public NetworkServerInfo AppInfo { get; set; } = new NetworkServerInfo();

        public int Port { get; private set; }
        public bool IsActive { get; private set; } = false;

        public List<Client> Clients { get; private set; } = new List<Client>();

        public TcpListener Listener { get; private set; }

        public event Action<Client> OnClientConnect;
        public Action<string>? OnLog;

        int nextClientId;
        public bool logPacketSend = false;

        public void Start()
        {
            if (IsActive)
                throw new Exception("Cannot start server, server is already active!");

            OnLog?.Invoke("Starting server...");
            Listener.Start();
            Listener.BeginAcceptTcpClient(new AsyncCallback(HandleClientConnect), null);

            nextClientId = 0;
            IsActive = true;

            OnLog?.Invoke("Server is now active!");
        }

        public void Stop(bool notifyClients = true)
        {
            if (!IsActive)
                throw new Exception("Server is already stopped!");

            //Send disconnect message to clients
            switch (notifyClients)
            {
                case true:
                    while (Clients.Count > 0)
                        DisconnectClient(Clients[0]);

                    break;
                case false:
                    while (Clients.Count > 0)
                        DisconnectClientLocal(Clients[0]);

                    break;
            }

            OnLog?.Invoke("Stopping server...");
            Listener.Stop();

            IsActive = false;

            OnLog?.Invoke("Stopped server");
        }

        public void DisconnectClient(Client client)
        {
            Send(client, new CC_Disconnect().CreateEmptyComponentPacket());
            DisconnectClientLocal(client);
        }

        public void DisconnectClientLocal(Client client)
        {
            client.DisconnectLocal();
            Clients.Remove(client);
        }

        public void ChangePort(int port)
        {
            if (IsActive)
                throw new Exception("Cannot change credentials when server is active!");

            Port = port;
        }

        #region Callbacks
        private void HandleClientConnect(IAsyncResult result)
        {
            if (!IsActive)
                return;

            try
            {
                var clientSocket = Listener.EndAcceptTcpClient(result);
                clientSocket.NoDelay = false;
                clientSocket.ReceiveBufferSize = Constants.RECEIVE_BUFFER_SIZE;
                clientSocket.SendBufferSize = Constants.SEND_BUFFER_SIZE;

                Listener.BeginAcceptTcpClient(new AsyncCallback(HandleClientConnect), null);

                Client newClient = new Client(nextClientId, clientSocket, HandleDataReceive, OnLog);
                Clients.Add(newClient);
                newClient.Initialize();

                nextClientId++;

                OnLog?.Invoke($"Client connected id: '{newClient.id}'");
                OnClientConnect?.Invoke(newClient);
            }
            catch (Exception e)
            {
                OnLog?.Invoke($"[Error] There was an error while connecting client: {e}");
            }
        }

        private void HandleDataReceive(OnServerReceiveDataArgs args)
        {
            byte[] buffer = (byte[])args.data.Clone();

            var packet = new Packet(buffer);

            Components.HandlePacketForServer(this, args.client, packet);
        }
        #endregion

        #region Send
        public void Send(Client client, Packet packet)
        {
            try
            {
                if (logPacketSend)
                    OnLog?.Invoke($"Sending to client id:{client.id} - {packet}");

                var data = packet.ToArray();
                client.Stream.BeginWrite(data, 0, data.Length, null, null);
            }
            catch (Exception e)
            {
                OnLog?.Invoke($"[Error] There was an error while sending data to client '{client.id}': {e}");
            }
        }

        public void SendToAll(Packet packet)
        {
            for (int i = 0; i < Clients.Count; i++)
                if (Clients[i] != null)
                    Send(Clients[i], packet);
        }

        void IPeer.Send(Packet packet) =>
            SendToAll(packet);
        #endregion


        public class Client
        {
            public Client(int id, TcpClient socket, Action<OnServerReceiveDataArgs> onDataReceive, Action<string>? onLog = null)
            {
                this.id = id;

                Socket = socket;
                Stream = socket.GetStream();
                buffer = new byte[Socket.ReceiveBufferSize];

                OnDataReceive = onDataReceive;
                OnLog = onLog;
            }

            public int id;
            public bool IsActive { get; private set; }

            public TcpClient Socket { get; private set; }
            public NetworkStream Stream { get; private set; }

            public Action<string>? OnLog;
            public event Action<OnServerReceiveDataArgs>? OnDataReceive;

            private byte[] buffer;

            public void Initialize()
            {
                IsActive = true;
                Stream.BeginRead(buffer, 0, Socket.ReceiveBufferSize, HandleReceiveData, null);
            }

            public void DisconnectLocal()
            {
                Stream.Close();
                Socket.Close();

                IsActive = false;
                OnLog?.Invoke($"Client id: {id} has been disconnected locally");
            }

            private void HandleReceiveData(IAsyncResult result)
            {
                try
                {
                    //FIXME: when the server stops client id:0 IsActive is still set to true,
                    //even though it was changed in the stop method. If you disconnect and
                    //reconnect, which assigns a new id, the error doesn't appear
                    if (!IsActive || !Stream.CanRead)
                        return;

                    int byteLength = Stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        OnLog?.Invoke($"Couldn't process data for client id '{id}'");
                        return;
                    }

                    byte[] tempBuffer = new byte[byteLength];
                    Array.Copy(buffer, tempBuffer, byteLength);

                    OnDataReceive?.Invoke(new OnServerReceiveDataArgs(this, tempBuffer));

                    if (IsActive)
                        Stream.BeginRead(buffer, 0, Socket.ReceiveBufferSize, HandleReceiveData, null);
                }
                catch (Exception e)
                {
                    OnLog?.Invoke($"Unexpected error while processing data: {e}");
                }
            }
        }
    }
}