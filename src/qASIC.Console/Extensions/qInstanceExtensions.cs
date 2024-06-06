namespace qASIC.Console
{
    public static class qInstanceExtensions
    {
        public static InstanceConsoleManager UseConsole(this qInstance instance)
        {
            instance.AppInfo.RegisterSystem(GameConsole.SYSTEM_NAME, GameConsole.SYSTEM_VERSION);
            var consoleManager = new InstanceConsoleManager(instance.RemoteInspectorServer);
            instance.Services.Add(consoleManager);
            return consoleManager;
        }

        public static InstanceConsoleManager GetConsoleInstanceManager(this qInstance instance) =>
            instance.Services.Get<InstanceConsoleManager>();

        public static void RegisterConsoleInstance(this qInstance instance, GameConsole console) =>
            instance.GetConsoleInstanceManager()
                .RegisterConsole(console);
    }
}
