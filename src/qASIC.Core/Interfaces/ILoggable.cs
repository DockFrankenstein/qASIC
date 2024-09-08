using System.Collections.Generic;

namespace qASIC.Core.Interfaces
{
    public interface ILoggable
    {
        LogManager Logs { get; set; }

        IEnumerable<ILoggable> Loggables { get; }
    }
}