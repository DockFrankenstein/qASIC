using qASIC.Communication;
using qASIC;
using qASIC.Console;
using qASIC.Console.Commands.Prompts;
using qASIC.Communication.Discovery;
using System.Text;
using System.Net;
using qASIC.Console.Commands.Attributes;
using qASIC.Console.Commands;

namespace qASICRemote
{
    public class InspectorCommand : CommandAttribute
    {
        public InspectorCommand(string name) : base(name) { }
        public InspectorCommand(string name, params string[] aliases) : base(name, aliases) { }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var inspector = new Inspector();
            inspector.Run(args);
        }
    }

    [LogColor(255, 192, 179)]
    public class Inspector
    {
        public Inspector()
        {
            var appInfo = new RemoteAppInfo()
            {
                projectName = "qRemote Inspector (Simple)",
                version = "1.0.0",
            };

            QasicInstance = new qInstance(appInfo)
            {
                autoStartRemoteInspectorServer = false,
            };

            var commands = new GameCommandList()
                .AddBuiltInCommands()
                .AddCommand(new ConnectionsListCommand(this))
                .FindAttributeCommands<InspectorCommand>();

            GConsole = new GameConsole(QasicInstance, "MAIN", commands);
            GConsole.Targets.Register(this);

            Interface = new SystemConsoleUI(GConsole);
            Interface.CanExecute += Interface_CanExecute;

            AppDomain.CurrentDomain.ProcessExit += OnApplicationClose;

            QasicInstance.cc_log.OnReceiveLog += Cc_log_OnReceiveLog;

            client = new qClient(QasicInstance.RemoteInspectorComponents)
            {
                AppInfo = appInfo,
            };

            client.OnDisconnect += Client_OnDisconnect;
            client.OnConnect += Client_OnConnect;
            client.OnStart += Client_OnStart;
            client.Logs.OnLog += a => GConsole.Log($"[Client] {a.message}", new qColor(179, 255, 254));

            DiscoveryClient = new DiscoveryClient(52148);
            DiscoveryClient.OnDiscover += args =>
            {
                GConsole.Log($"Server discovered, address: {args.Address}:{args.Port}, identity: {args.Identity.ReadNetworkSerializable<RemoteAppInfo>()}");
                args.Identity.ResetPosition();
            };
            DiscoveryClient.OnRemoved += args =>
            {
                GConsole.Log($"Server removed, address: {args.Address}:{args.Port}, identity: {args.Identity.ReadNetworkSerializable<RemoteAppInfo>()}");
                args.Identity.ResetPosition();
            };

            consoleManager = new InstanceConsoleManager(client);
            consoleManager.CC_Log.OnRead += CC_Log_OnRead;
            consoleManager.OnConsoleRegister += ConsoleManager_OnConsoleRegister;
        }

        const int UPDATE_FREQUENCY = 200;

        public qClient client = null;

        public qInstance QasicInstance { get; private set; } = null;
        public GameConsole GConsole { get; private set; } = null;
        public SystemConsoleUI Interface { get; private set; } = null;
        public DiscoveryClient DiscoveryClient { get; private set; } = null;

        public InstanceConsoleManager consoleManager;

        bool AutoConnect { get; set; } = false;

        public GameConsole SelectedConsole { get; private set; }

        [LogColor(GenericColor.White)]
        public void Run(string[] args)
        {
            QasicInstance.Start();
            DiscoveryClient.Start();

            new Task(async () =>
            {
                while (true)
                    await Update();
            }).Start();

            GConsole.Log($"Created update loop, update frequency: {UPDATE_FREQUENCY}ms");

            GConsole.Log("-----------------------------------------------------");
            GConsole.Log("Type '.help' to list all commands");

            Interface.StartReading();
        }

        private bool Interface_CanExecute(string cmd)
        {
            var forceUseGConsole = GConsole.ReturnedValue is CommandPrompt;

            if (forceUseGConsole)
                return true;

            if (cmd.StartsWith("."))
            {
                GConsole.Execute(cmd.Substring(1, cmd.Length - 1));
                return false;
            }

            if (client.CurrentState != qClient.State.Connected)
            {
                GConsole.LogError("Currently not connected to any application. Use '.' prefix to run commands for this application!");
                return false;
            }

            if (SelectedConsole == null)
            {
                GConsole.LogError("No console selected!");
                return false;
            }

            consoleManager.Get(SelectedConsole.Name).SendCommand(cmd);
            return false;
        }

        private void ConsoleManager_OnConsoleRegister(GameConsole console)
        {
            if (SelectedConsole == null)
                SelectedConsole = console;

            if (SelectedConsole == console)
                foreach (var log in console.Logs)
                    CC_Log_OnRead(console, log);
        }

        private void CC_Log_OnRead(GameConsole console, qLog log)
        {
            if (SelectedConsole != console) return;
            log.message = $"[R:{console.Name}] {log.message}";
            GConsole?.Log(log);
        }

        private void Client_OnStart()
        {
            GConsole.Clear();
        }

        [InspectorCommand("disconnect", "dc", Description = "Disconnects from connected application.")]
        public void Disconnect()
        {
            if (client?.IsActive != true)
            {
                GConsole?.LogError("Client is not active!");
                return;
            }

            client.Disconnect();
        }

        [InspectorCommand("connect", "cn", Description = "Connects to an application.")]
        private void Connect() =>
            Connect("127.0.0.1");

        [InspectorCommand("connect")]
        private void Connect(int port) =>
            Connect($"127.0.0.1:{port}");

        [InspectorCommand("connect")]
        private void Connect(string address)
        {
            var addressParts = address.Split(":");

            int port = client.Port;
            if (addressParts.Length > 2 ||
                !IPAddress.TryParse(addressParts[0], out IPAddress finalAddress) ||
                (addressParts.Length == 2 && !int.TryParse(addressParts[1], out port)))
                throw new GameCommandException($"Could not parse address '{address}'");

            if (client!.IsActive)
                throw new GameCommandException("Client is already active, this application doesn't support multiple client instances!");

            client.Connect(finalAddress, port);
        }

        [InspectorCommand("listconsoles", "lc", Description = "Lists all registered consoles.")]
        private void ListConsoles()
        {
            TextTree tree = TextTree.Fancy;
            TextTreeItem root = new TextTreeItem("Registered consoles:");
            var consoles = consoleManager.ToArray();
            for (int i = 0; i < consoles.Length; i++)
                root.Add($"{i}: {consoles[i].Console.Name}");

            GConsole?.Log(tree.GenerateTree(root));
        }

        [InspectorCommand("selectedconsole", "sc", Description = "Console that's currently selected.")]
        private string Cmd_SelectedConsoleIndex()
        {
            if (SelectedConsole == null)
                throw new GameCommandException("No console is selected");

            return SelectedConsole.Name;
        }

        [InspectorCommand("selectedconsole")]
        private void Cmd_SelectedConsoleIndex(CommandArgs args, string val)
        {
            var console = consoleManager.Where(x => x.Console.Name == val)
                .FirstOrDefault()?.Console;

            if (console == null)
                throw new GameCommandException("Console does not exist!");

            SelectedConsole = console;
            args.console.Log($"Selected console '{SelectedConsole.Name}'.");
        }

        [InspectorCommand("selectedconsole")]
        private void Cmd_SelectedConsoleIndex(CommandArgs args, int index)
        {
            var consoles = consoleManager.ToArray();

            if (!consoles.IndexInRange(index))
                throw new GameCommandException("Console index is out of range!");

            Cmd_SelectedConsoleIndex(args, consoles[index].Console.Name);
        }

        private void Cc_log_OnReceiveLog(qLog log, PacketType packetType)
        {
            log.message = $"[qDebug] {log.message}";
            GConsole?.Log(log);
        }

        [LogColor(GenericColor.White)]
        private void Client_OnConnect()
        {
            var appInfo = (RemoteAppInfo)client!.AppInfo;
            GConsole?.Log($"Connected to '{appInfo.projectName}' v{appInfo.version} made with '{appInfo.engine}' v{appInfo.engineVersion} using protocol version {appInfo.protocolVersion}");

            var systems = appInfo.systems
                .Select(x => $"\n- {x.name} v{x.version}");
            
            GConsole?.Log($"Used systems by projects:{string.Join(string.Empty, systems)}");
        }

        private void Client_OnDisconnect(qClient.DisconnectReason reason)
        {
            SelectedConsole = null;

            switch (reason)
            {
                case qClient.DisconnectReason.None:
                    return;
                default:
                    if (AutoConnect)
                        client?.Connect();
                    break;
            }
        }

        public async Task Update()
        {
            client.Update();
            await Task.Delay(UPDATE_FREQUENCY);
        }

        private void OnApplicationClose(object sender, EventArgs e)
        {
            client?.Disconnect();
            DiscoveryClient?.Stop();
        }

        class ConnectionsListCommand : GameCommand
        {
            public ConnectionsListCommand(Inspector inspector)
            {
                Inspector = inspector;
            }

            public override string CommandName => "connectionslist";
            public override string[] Aliases => new string[] { "cl" };

            public override string Description => "Shows a list of discovered connections and allows to connect to them.";

            Inspector Inspector { get; set; }

            qLog log;
            int index;

            KeyPrompt navigationPrompt = new KeyPrompt();

            public override object Run(CommandArgs args)
            {
                if (log == null || args.console.ReturnedValue == null)
                {
                    log = qLog.CreateNow("");
                    index = 0;
                }

                StringBuilder logTxt = new StringBuilder("Navigate with arrows, left arrow to exit");
                bool final = false;

                if (args.console.ReturnedValue == navigationPrompt)
                {
                    switch (navigationPrompt.Key)
                    {
                        case KeyPrompt.NavigationKey.Cancel:
                        case KeyPrompt.NavigationKey.Left:
                            final = true;
                            break;
                        case KeyPrompt.NavigationKey.Up:
                            index = Math.Clamp(index - 1, 0, Inspector.DiscoveryClient.Discovered.Count - 1);
                            break;
                        case KeyPrompt.NavigationKey.Down:
                            index = Math.Clamp(index + 1, 0, Inspector.DiscoveryClient.Discovered.Count - 1);
                            break;
                        case KeyPrompt.NavigationKey.Right:
                        case KeyPrompt.NavigationKey.Confirm:
                            var targetConn = Inspector.DiscoveryClient.Discovered[index];
                            Inspector.client.Connect(targetConn.Address, targetConn.Port);
                            final = true;
                            break;
                    }
                }

                for (int i = 0; i < Inspector.DiscoveryClient.Discovered.Count; i++)
                {
                    logTxt.Append("\n");
                    logTxt.Append(index == i ? (final ? "]" : ">") : " ");
                    logTxt.Append(" ");
                    var conn = Inspector.DiscoveryClient.Discovered[i];
                    var info = conn.Identity.ReadNetworkSerializable<RemoteAppInfo>();
                    conn.Identity.ResetPosition();

                    logTxt.Append($" {conn.Address}:{conn.Port} - {(string.IsNullOrWhiteSpace(info.projectName) ? "UNKNOWN" : info.projectName)}");

                    if (!string.IsNullOrWhiteSpace(info.version))
                        logTxt.Append($" v{info.version}");
                }

                log.message = logTxt.ToString();

                args.console.Log(log);
                return final ? 
                    null : 
                    navigationPrompt;
            }
        }
    }
}