using GameLog = qASIC.qLog;
using System.Diagnostics;
using qASIC.Console.Commands;
using System.Reflection;
using qASIC.Console.Parsing.Arguments;
using System;
using System.Collections.Generic;

namespace qASIC.Console
{
    public class GameConsole
    {
        public const string SYSTEM_NAME = "qASIC.Console";
        public const string SYSTEM_VERSION = "1.0.0";

        public GameConsole() : this(Guid.NewGuid().ToString()) { }

        public GameConsole(string name)
        {
            Name = name;

            qDebug.OnLog += QDebug_OnLog;
        }

        private void QDebug_OnLog(GameLog log)
        {
            if (LogQDebug)
                Log(log, 4, false);
        }

        public string Name { get; private set; }

        public event Action<GameLog> OnLog;

        public List<GameLog> Logs { get; internal set; } = new List<GameLog>();

        public GameCommandList CommandList { get; set; }
        public ArgumentsParser CommandParser { get; set; }

        public GameConsoleTheme Theme { get; set; } = GameConsoleTheme.Default;

        /// <summary>Should the console log messages from <see cref="qDebug"/>></summary>
        public bool LogQDebug { get; set; } = true;
        
        /// <summary>Determines if console should try looking for attributes that can change log messages and colors.</summary>
        public bool UseLogModifierAttributes { get; set; } = true;

        /// <summary>Determines if it should include exceptions when logging normal errors with executing commands.</summary>
        public bool IncludeStackTraceInCommandExceptions { get; set; } = true;

        /// <summary>Determines if it should include exceptions when logging unknown errors with executing commands.</summary>
        public bool IncludeStackTraceInUnknownCommandExceptions { get; set; } = false;

        #region Registering targets
        public List<object> Targets { get; } = new List<object>();

        public void RegisterTarget(object target)
        {
            if (Targets.Contains(target)) return;
            Targets.Add(target);
        }

        public void DeregisterTarget(object target)
        {
            if (!Targets.Contains(target)) return;
            Targets.Remove(target);
        }
        #endregion

        #region Executing
        /// <summary>Can the console execute commands using <see cref="Execute(string)"/>.</summary>
        public bool CanParseAndExecute =>
            CommandList != null && CommandParser != null;

        /// <summary>Can the console execute commands using <see cref="Execute(string[])"/>.</summary>
        public bool CanExecute =>
            CommandList != null;

        /// <summary>Executes a command.</summary>
        /// <param name="cmd">Command text that will be parsed and executed.</param>
        public void Execute(string cmd)
        {
            if (CommandParser == null)
                throw new Exception("Cannot parse commands with no parser!");

            var args = CommandParser.ParseString(cmd);
            Execute(args);
        }

        /// <summary>Executes a command.</summary>
        /// <param name="args">Parsed arguments.</param>
        public void Execute(ConsoleArgument[] args)
        {
            if (CommandList == null)
                throw new Exception("Cannot execute commands with no command list!");

            if (args.Length == 0)
                return;

            var commandName = args[0].arg.ToLower();

            if (!CommandList.TryGetCommand(commandName, out IGameCommand command))
            {
                LogError($"Command {commandName} doesn't exist");
                return;
            }

            var commandArgs = new CommandArgs()
            {
                commandName = commandName,
                args = args,
                console = this,
            };

            Execute(commandName, () => command!.Run(commandArgs));
        }

        public object Execute(string commandName, Func<object> command)
        {
            try
            {
                var output = command.Invoke();

                if (output != null)
                    Log($"Command returned '{output}'");

                return output;
            }
            catch (GameCommandException e)
            {
                LogError(e.ToString(IncludeStackTraceInCommandExceptions));
            }
            catch (Exception e)
            {
                LogError(IncludeStackTraceInUnknownCommandExceptions ?
                    $"There was an error while executing command '{commandName}': {e}" :
                    $"There was an error while executing command '{commandName}'.");
            }

            return null;
        }
        #endregion

        #region Logging
        /// <summary>Logs a message to the console.</summary>
        /// <param name="message">Message to log.</param>
        /// <param name="stackTraceIndex">Index used for gathering log customization attributes.</param>
        public void Log(string message, int stackTraceIndex = 2) =>
            Log(GameLog.CreateNow(message, qDebug.DEFAULT_COLOR_TAG), stackTraceIndex, false);

        /// <summary>Logs a warning message to the console.</summary>
        /// <param name="message">Message to log.</param>
        /// <param name="stackTraceIndex">Index used for gathering log customization attributes.</param>
        public void LogWarning(string message, int stackTraceIndex = 2) =>
            Log(GameLog.CreateNow(message, qDebug.WARNING_COLOR_TAG), stackTraceIndex);

        /// <summary>Logs an error message to the console.</summary>
        /// <param name="message">Message to log.</param>
        /// <param name="stackTraceIndex">Index used for gathering log customization attributes.</param>
        public void LogError(string message, int stackTraceIndex = 2) =>
            Log(GameLog.CreateNow(message, qDebug.ERROR_COLOR_TAG), stackTraceIndex);

        /// <summary>Logs a message to the console with a color.</summary>
        /// <param name="message">Message to log.</param>
        /// <param name="color">Message color.</param>
        /// <param name="stackTraceIndex">Index used for gathering log customization attributes.</param>
        public void Log(string message, qColor color, int stackTraceIndex = 2) =>
            Log(GameLog.CreateNow(message, color), stackTraceIndex);

        /// <summary>Logs a message to the console with a color.</summary>
        /// <param name="message">Message to log.</param>
        /// <param name="colorTag">Message color.</param>
        /// <param name="stackTraceIndex">Index used for gathering log customization attributes.</param>
        public void Log(string message, string colorTag, int stackTraceIndex = 2) =>
            Log(GameLog.CreateNow(message, colorTag), stackTraceIndex);

        /// <summary>Logs a log to the console.</summary>
        /// <param name="stackTraceIndex">Index used for gathering log customization attributes.</param>
        /// <param name="overwriteColor">If true, the console will not check for color attributes.</param>
        public void Log(GameLog log, int stackTraceIndex = 2, bool overwriteColor = true)
        {
            if (UseLogModifierAttributes && !overwriteColor)
            {
                var stackTrace = new StackTrace();
                var stackFrame = stackTrace.GetFrame(stackTraceIndex);

                var method = stackFrame?.GetMethod();
                var declaringType = method?.DeclaringType;

                if (TryGetColorAttributeOfTrace(method, declaringType, out var colorAttr))
                {
                    log.colorTag = colorAttr!.ColorTag;
                    log.color = colorAttr!.Color;
                }

                if (TryGetPrefixAttributeOfTrace(method, declaringType, out var prefixAttr))
                {
                    log.message = prefixAttr!.FormatMessage(log.message);
                }
            }

            Logs.Add(log);
            OnLog?.Invoke(log);
        }

        public qColor GetLogColor(GameLog log) =>
            Theme.GetLogColor(log);

        static bool TryGetPrefixAttributeOfTrace(MethodBase method, Type declaringType, out LogPrefixAttribute attribute)
        {
            attribute = null;

            if (method != null &&
                ConsoleReflections.PrefixAttributeMethods.TryGetValue(ConsoleReflections.CreateMethodId(method), out var methodAttr))
            {
                attribute = methodAttr!;
                return true;
            }

            if (declaringType != null &&
                ConsoleReflections.PrefixAttributeDeclaringTypes.TryGetValue(ConsoleReflections.CreateTypeId(declaringType), out var declaringTypeAttr))
            {
                attribute = declaringTypeAttr!;
                return true;
            }

            return false;
        }

        static bool TryGetColorAttributeOfTrace(MethodBase method, Type declaringType, out LogColorAttribute attribute)
        {
            attribute = null;

            if (method != null &&
                ConsoleReflections.ColorAttributeMethods.TryGetValue(ConsoleReflections.CreateMethodId(method), out var methodAttr))
            {
                attribute = methodAttr!;
                return true;
            }

            if (declaringType != null &&
                ConsoleReflections.ColorAttributeDeclaringTypes.TryGetValue(ConsoleReflections.CreateTypeId(declaringType), out var declaringTypeAttr))
            {
                attribute = declaringTypeAttr!;
                return true;
            }

            return false;
        }
        #endregion
    }
}