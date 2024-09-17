using qASIC.CommandPrompts;
using System;
using System.Collections.Generic;
using System.Linq;
using SysConsole = System.Console;

namespace qASIC.Console
{
    /// <summary>Class responsible for displaying and reading information from a Console Application window for a <see cref="GameConsole"/> instance.</summary>
    public class SystemConsoleUI
    {
        public SystemConsoleUI() : this(new GameConsole("MAIN")) { }

        public SystemConsoleUI(qInstance instance) : this (new GameConsole(instance, "MAIN")) { }

        public SystemConsoleUI(GameConsole console)
        {
            Console = console;
        }

        GameConsole _console;
        /// <summary>Console which will be used by the interface.</summary>
        public GameConsole Console
        {
            get => _console;
            set
            {
                if (_console == value) return;

                if (_console != null)
                {
                    _console.OnLog -= Console_OnLog;
                    _console.OnUpdateLog -= Console_OnUpdateLog;
                }

                VisibleLogs.Clear();
                _console = value;

                if (_console != null)
                {
                    _console.OnLog += Console_OnLog;
                    _console.OnUpdateLog += Console_OnUpdateLog;
                }
            }
        }

        void Console_OnLog(qLog log)
        {
            if (log.logType == LogType.Clear)
            {
                SysConsole.Clear();
                VisibleLogs.Clear();
                return;
            }

            var txt = CreateLogText(log);
            VisibleLogs.Add(log, new LogData()
            {
                consoleTop = SysConsole.CursorTop,
                emptyString = CreateEmptyStringForLog(log),
            });

            SysConsole.WriteLine(ColorText(txt, Console.GetLogColor(log)));
        }

        void Console_OnUpdateLog(qLog log)
        {
            //Ignore if clear
            if (log.logType == LogType.Clear)
                return;

            //Ignore if it's not displayed
            if (!VisibleLogs.ContainsKey(log))
                return;

            var txt = CreateLogText(log);

            var top = SysConsole.CursorTop;
            var left = SysConsole.CursorLeft;

            var logData = VisibleLogs[log];

            SysConsole.CursorTop = logData.consoleTop;
            SysConsole.CursorLeft = 0;

            //Override with garbage data, not sure why it doesn't
            //work with spaces
            SysConsole.Write(logData.emptyString.Replace(' ', '@'));

            SysConsole.CursorTop = logData.consoleTop;
            SysConsole.CursorLeft = 0;

            //Clear
            SysConsole.Write(logData.emptyString);

            SysConsole.CursorTop = logData.consoleTop;
            SysConsole.CursorLeft = 0;

            //Write new log
            SysConsole.Write(ColorText(txt, Console.GetLogColor(log)));

            if (SysConsole.CursorTop >= top)
                top = SysConsole.CursorTop + 1;

            SysConsole.CursorTop = top;
            SysConsole.CursorLeft = left;
        }

        /// <summary>Format of a log, where:
        /// <list type="bullet">
        /// <item>{0} - <see cref="qLog.message"/></item>
        /// <item>{1} - <see cref="qLog.time"/></item>
        /// <item>{2} - <see cref="qLog.logType"/></item>
        /// </list>
        /// </summary>
        public string LogFormat { get; set; } = "[{1}] [{2}] {0}";

        /// <summary>String used for formatting <see cref="qLog.time"/>.</summary>
        public string TimeFormat { get; set; } = "HH:mm:ss.fff";

        /// <summary>Determines if user input should be read in <see cref="StartReading(bool)"/>. By setting this to false, interface will stop reading after the next command.</summary>
        public bool CanRead { get; set; }

        private Dictionary<qLog, LogData> VisibleLogs { get; set; } = new Dictionary<qLog, LogData>();

        /// <summary>Gets invoked before <see cref="Execute"/>. If false, command will not be executed.</summary>
        public event Func<string, bool> CanExecute;

        /// <summary>Starts reading user input from the console window.</summary>
        /// <param name="readOnce">If true, reading will not be repeated.</param>
        public void StartReading(bool readOnce = false)
        {
            CanRead = !readOnce;

            while (CanRead)
            {
                string cmd;
                switch (Console.ReturnedValue)
                {
                    case KeyPrompt keyPrompt:
                        var key = SysConsole.ReadKey();

                        var promptKey = key.Key switch
                        {
                            ConsoleKey.UpArrow => KeyPrompt.NavigationKey.Up,
                            ConsoleKey.DownArrow => KeyPrompt.NavigationKey.Down,
                            ConsoleKey.LeftArrow => KeyPrompt.NavigationKey.Left,
                            ConsoleKey.RightArrow => KeyPrompt.NavigationKey.Right,
                            ConsoleKey.Enter => KeyPrompt.NavigationKey.Confirm,
                            ConsoleKey.Escape => KeyPrompt.NavigationKey.Cancel,
                            _ => KeyPrompt.NavigationKey.None,
                        };

                        cmd = KeyPrompt.keyNames.Backward[promptKey];

                        if (promptKey == KeyPrompt.NavigationKey.None)
                        {
                            if (!char.IsSymbol(key.KeyChar))
                            {
                                cmd = string.Empty;
                                break;
                            }

                            cmd = key.KeyChar.ToString();
                        }

                        break;
                    default:
                        cmd = SysConsole.ReadLine();
                        break;
                }

                if (CanExecute?.Invoke(cmd) == false)
                    continue;

                Console.Execute(cmd);
            }
        }

        protected string CreateLogText(qLog log) =>
            string.Format(LogFormat, log.message, log.time.ToString(TimeFormat), log.logType);

        protected string ColorText(string txt, qColor color) =>
            $"\u001b[38;2;{color.red};{color.green};{color.blue}m{txt}\u001b[0m";
        //txt;

        protected string CreateEmptyStringForLog(qLog log) =>
            ColorText(new string (CreateLogText(log).Select(x => char.IsControl(x) ? x : ' ').ToArray()), log.color);

        class LogData
        {
            public int consoleTop;
            public string emptyString;
        }
    }
}