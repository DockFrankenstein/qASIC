using qASIC.CommandPrompts;
using System;
using System.Collections.Generic;
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
            Dictionary<qLog, KeyValuePair<int, int>> consoleLines = new Dictionary<qLog, KeyValuePair<int, int>>();

            console.Logs.OnUpdateLog += (log) =>
            {
                //Ignore if clear
                if (log.logType == LogType.Clear)
                    return;

                //Ignore if it's not displayed
                if (!consoleLines.ContainsKey(log))
                    return;

                var txt = CreateLogText(log);

                var top = SysConsole.CursorTop;
                var left = SysConsole.CursorLeft;

                var logData = consoleLines[log];

                SysConsole.CursorTop = logData.Key;
                SysConsole.CursorLeft = 0;

                SysConsole.Write(new string(' ', logData.Value));

                SysConsole.CursorTop = logData.Key;
                SysConsole.CursorLeft = 0;

                consoleLines[log] = new KeyValuePair<int, int>(logData.Key, txt.Length);

                SysConsole.Write(ColorText(txt, console.GetLogColor(log)));

                SysConsole.CursorTop = top;
                SysConsole.CursorLeft = left;
            };

            console.Logs.OnLog += (log) =>
            {
                if (log.logType == LogType.Clear)
                {
                    SysConsole.Clear();
                    consoleLines.Clear();
                    return;
                }

                var txt = CreateLogText(log);
                consoleLines.Add(log, new KeyValuePair<int, int>(SysConsole.CursorTop, txt.Length));

                SysConsole.WriteLine(ColorText(txt, console.GetLogColor(log)));
            };

            return console;


            string CreateLogText(qLog log) =>
                string.Format(logFormat, log.message, log.time.ToString(timeFormat), log.logType);

            string ColorText(string txt, qColor color) =>
                $"\u001b[38;2;{color.red};{color.green};{color.blue}m{txt}\u001b[0m";
        }

        public static string ReadConsoleApplication(this GameConsole console)
        {
            string cmd;
            switch (console.ReturnedValue)
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

                    if (promptKey != KeyPrompt.NavigationKey.None)
                    {
                        if (!char.IsSymbol(key.KeyChar))
                            return string.Empty;

                        cmd = key.KeyChar.ToString();
                    }

                    break;
                default:
                    cmd = SysConsole.ReadLine();
                    break;
            }

            return cmd;
        }

        public static void ExecuteConsoleApplicationLine(this GameConsole console)
        {
            string cmd = console.ReadConsoleApplication();
            console.Execute(cmd);
        }
    }
}