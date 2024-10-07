using System;
using System.Collections;
using System.Collections.Generic;

namespace qASIC.Console
{
    public class GameLogManager : LogManager, IEnumerable<qLog>
    {
        public GameLogManager() : this(new List<qLog>()) { }
        public GameLogManager(IEnumerable<qLog> logs)
        {
            Logs = new List<qLog>(logs);
        }

        #region Logging
        public List<qLog> Logs { get; private set; }

        public event Action<qLog> OnUpdateLog;

        protected void InvokeOnUpdateLog(qLog log) =>
            OnUpdateLog?.Invoke(log);

        public override void Log(qLog log)
        {
            if (Logs.Contains(log))
            {
                InvokeOnUpdateLog(log);
                return;
            }

            Logs.Add(log);
            InvokeOnLog(log);
        }
        #endregion

        public IEnumerator<qLog> GetEnumerator() =>
            Logs.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}