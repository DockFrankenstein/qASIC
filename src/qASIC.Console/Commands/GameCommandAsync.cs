using System.Threading.Tasks;

namespace qASIC.Console.Commands
{
    public abstract class GameCommandAsync : GameCommand
    {
        public override object Run(GameCommandArgs args) =>
            RunAsync(args);

        public abstract Task<object> RunAsync(GameCommandArgs args);
    }
}