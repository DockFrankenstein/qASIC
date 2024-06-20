using qASIC.Communication;
using qASIC;
using qASIC.Console;

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

            var commands = new qASIC.Console.Commands.GameCommandList()
                .FindBuiltInCommands()
                .FindAttributeCommands<InspectorCommand>();

            GConsole = new GameConsole(QasicInstance, "MAIN", commands);
            GConsole.ForConsoleApplication();
            GConsole.IncludeStackTraceInUnknownCommandExceptions = true;
            GConsole.Targets.Register(this);

            AppDomain.CurrentDomain.ProcessExit += OnApplicationClose;

            QasicInstance.cc_log.OnReceiveLog += Cc_log_OnReceiveLog;

            client = new qClient(QasicInstance.RemoteInspectorComponents)
            {
                AppInfo = appInfo,
            };

            client.OnDisconnect += Client_OnDisconnect;
            client.OnConnect += Client_OnConnect;
            client.OnStart += Client_OnStart;
            client.OnLog += a => GConsole.Log($"[Client] {a}", new qColor(179, 255, 254));

            consoleManager = new InstanceConsoleManager(client);
            consoleManager.CC_Log.OnRead += CC_Log_OnRead;
            consoleManager.OnConsoleRegister += ConsoleManager_OnConsoleRegister;
        }

        const int UPDATE_FREQUENCY = 200;

        public qClient client = null;

        public qInstance QasicInstance = null;
        public GameConsole GConsole = null;

        public InstanceConsoleManager consoleManager;

        bool AutoConnect { get; set; } = false;

        public GameConsole SelectedConsole { get; private set; }

        [LogColor(GenericColor.White)]
        public void Run(string[] args)
        {
            QasicInstance.Start();

            new Task(async () =>
            {
                while (true)
                    await Update();
            }).Start();

            GConsole.Log($"Created update loop, update frequency: {UPDATE_FREQUENCY}ms");

            GConsole.Log("-----------------------------------------------------");
            GConsole.Log("Type '.help' to list all commands");

            while (true)
            {
                var cmd = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(cmd))
                    continue;

                if (cmd.StartsWith("."))
                {
                    GConsole.Execute(cmd.Substring(1, cmd.Length - 1));
                    continue;
                }

                if (client.CurrentState != qClient.State.Connected)
                {
                    GConsole.LogError("Currently not connected to any application. Use '.' prefix to run commands for this application!");
                    continue;
                }

                if (SelectedConsole == null)
                {
                    GConsole.LogError("No console selected!");
                    continue;
                }

                consoleManager.Get(SelectedConsole.Name).SendCommand(cmd);
            }
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
            Connect(false);

        [InspectorCommand("connect")]
        private void Connect(bool autoconnect) =>
            Connect(client?.Port ?? Constants.DEFAULT_PORT, autoconnect);

        [InspectorCommand("connect")]
        private void Connect(int port) =>
            Connect(port, false);

        [InspectorCommand("connect")]
        private void Connect(int port, bool autoconnect)
        {
            AutoConnect = autoconnect;
            GConsole?.Log($"Auto connect: {autoconnect}");

            if (client!.IsActive)
            {
                GConsole?.LogError("Client is already active, this application doesn't support multiple client instances!");
                return;
            }

            client.Connect(client.Address, port);
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
        }
    }
}