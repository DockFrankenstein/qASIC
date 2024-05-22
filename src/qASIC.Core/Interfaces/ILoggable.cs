namespace qASIC.Core.Interfaces
{
    public interface ILoggable
    {
        LogManager Logs { get; set; }

        ILoggable[] Loggables { get; }
    }
}