using qASIC.Communication;

namespace qASIC.Console.Comms
{
    public class CC_ConsoleDeregister : ConsoleCommsComponent
    {
        public override void ReadForConsole(CommsComponentArgs args, GameConsole console)
        {
            ConsoleManager?.DeregisterConsole(console);
        }
    }
}