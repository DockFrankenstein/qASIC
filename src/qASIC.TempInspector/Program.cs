using qASIC.Communication;
using System.Net;
using qASIC;
using qASIC.Console;
using System;
using Pastel;

namespace qASICRemote
{
    internal class Program
    {
        const int UPDATE_FREQUENCY = 200;

        public static qClient? client = null;

        public static qInstance? QasicInstance = null;
        public static GameConsole? GConsole = null;

        public static InstanceConsoleManager? consoleManager;

        static bool AutoConnect { get; set; } = false;

        private static void Main(string[] args)
        {
            QasicInstance = new qInstance()
            {
                autoStartRemoteInspectorServer = false,
            };

            var commands = new qASIC.Console.Commands.GameCommandList()
                .FindBuiltInCommands()
                .FindAttributeCommands()
                .FindCommands();

            GConsole = new GameConsole("Main")
            {
                CommandList = commands,
                CommandParser = new qASIC.Console.Parsing.Arguments.QuashParser(),
            };

            AppDomain.CurrentDomain.ProcessExit += OnApplicationClose;

            QasicInstance.cc_log.OnReceiveLog += Cc_log_OnReceiveLog;

            client = new qClient(QasicInstance.RemoteInspectorComponents, IPAddress.Parse("127.0.0.1"), Constants.DEFAULT_PORT)
            {
                AppInfo = new RemoteAppInfo(),
            };

            client.OnDisconnect += Client_OnDisconnect;
            client.OnConnect += Client_OnConnect;
            client.OnStart += Client_OnStart;
            client.OnLog += a => GConsole.Log($"[Client] {a}");

            consoleManager = new InstanceConsoleManager(client);
            consoleManager.CC_Log.OnRead += CC_Log_OnRead;
            consoleManager.OnConsoleRegister += ConsoleManager_OnConsoleRegister;

            new Task(async () =>
            {
                while (true)
                    await Update();
            }).Start();

            GConsole.OnLog += GConsole_OnLog;

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

                if (consoleManager.Count() == 0)
                {
                    GConsole.LogError("Application has no consoles registered!");
                    continue;
                }

                consoleManager.First().SendCommand(cmd);
            }
        }

        private static void ConsoleManager_OnConsoleRegister(GameConsole console)
        {
            foreach (var log in console.Logs)
                CC_Log_OnRead(console, log);
        }

        private static void CC_Log_OnRead(GameConsole console, qLog log)
        {
            log.message = $"[R:{console.Name}] {log.message}";
            GConsole?.Log(log);
        }

        private static void Client_OnStart()
        {
            Console.Clear();
        }

        [Command("disconnect", "dc", Description = "Disconnects from connected application.")]
        public static void Disconnect()
        {
            if (client?.IsActive != true)
            {
                GConsole?.LogError("Client is not active!");
                return;
            }

            client.Disconnect();
        }

        [Command("connect", "cn", Description = "Connects to an application.")]
        private static void Connect() =>
            Connect(false);

        [Command("connect")]
        private static void Connect(bool autoconnect) =>
            Connect(client?.Port ?? Constants.DEFAULT_PORT, autoconnect);

        [Command("connect")]
        private static void Connect(int port) =>
            Connect(port, false);

        [Command("connect")]
        private static void Connect(int port, bool autoconnect)
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

        [Command("listconsoles", "lc", Description = "Lists all registered consoles.")]
        private static void ListConsoles()
        {
            var consoles = consoleManager?
                .Select(x => $"\n- {x.Console.Name}") ??
                new List<string>();

            GConsole?.Log($"Registered consoles: {string.Join(string.Empty, consoles)}");
        }

        private static void GConsole_OnLog(qLog log)
        {
            if (log.logType == LogType.Clear)
            {
                Console.Clear();
                return;
            }

            var color = GConsole?.GetLogColor(log) ?? qColor.White;
            Console.WriteLine($"[Output] [{log.logType}] {log.message}".Pastel(System.Drawing.Color.FromArgb(color.red, color.green, color.blue)));
        }

        private static void Cc_log_OnReceiveLog(qLog log, PacketType packetType)
        {
            log.message = $"[qDebug]{log.message}";
            GConsole?.Log(log);
        }

        private static void Client_OnConnect()
        {
            var appInfo = (RemoteAppInfo)client!.AppInfo;
            GConsole?.Log($"Connected to '{appInfo.projectName}' v{appInfo.version} made with '{appInfo.engine}' v{appInfo.engineVersion} using protocol version {appInfo.protocolVersion}");

            var systems = appInfo.systems
                .Select(x => $"\n- {x.name} v{x.version}");
            
            GConsole?.Log($"Used systems by projects:{string.Join(string.Empty, systems)}");
        }

        private static void Client_OnDisconnect(qClient.DisconnectReason reason)
        {
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

        public static async Task Update()
        {
            client!.Update();
            await Task.Delay(UPDATE_FREQUENCY);
        }

        private static void OnApplicationClose(object? sender, EventArgs e)
        {
            client?.Disconnect();
        }
    }
}