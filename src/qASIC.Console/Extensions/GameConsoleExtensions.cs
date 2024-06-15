using SysConsole = System.Console;

namespace qASIC.Console
{
    public static class GameConsoleExtensions
    {
        /// <summary>Makes the console work with <see cref="System.Console"/>.</summary>
        /// <param name="console">Console to register.</param>
        /// <param name="logFormat">Format of a log, where:<list type="bullet">
        /// <item>{0} - <see cref="qLog.message"/></item>
        /// <item>{1} - <see cref="qLog.time"/></item>
        /// <item>{2} - <see cref="qLog.logType"/></item>
        /// </list></param>
        /// <param name="timeFormat">String used for formatting <see cref="qLog.time"/>.</param>
        public static GameConsole ForConsoleApplication(this GameConsole console, string logFormat = "[{1}] [{2}] {0}", string timeFormat = "HH:mm:ss.fff")
        {
            console.OnLog += (log) =>
            {
                if (log.logType == LogType.Clear)
                {
                    SysConsole.Clear();
                    return;
                }

                var color = console.GetLogColor(log);
                var content = string.Format(logFormat, log.message, log.time.ToString(timeFormat), log.logType);

                SysConsole.WriteLine($"\u001b[38;2;{color.red};{color.green};{color.blue}m{content}\u001b[0m");
            };

            return console;
        }
    }
}