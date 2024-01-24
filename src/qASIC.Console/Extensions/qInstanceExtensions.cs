using qASIC.Console.Comms;

namespace qASIC.Console
{
    public static class qInstanceExtensions
    {
        public static InstanceConsoleManager UseConsole(this qInstance instance)
        {
            instance.AppInfo.RegisterSystem(GameConsole.SYSTEM_NAME, GameConsole.SYSTEM_VERSION);
            var consoleManager = new InstanceConsoleManager(instance.RemoteInspectorServer);
            return consoleManager;
        }
    }
}
