using GameLog = qASIC.qLog;
using System.Diagnostics;
using qASIC.Console.Commands;
using System.Reflection;
using qASIC.Console.Parsing.Arguments;
using System;
using System.Collections.Generic;
using qASIC.Console.Commands.Prompts;
using System.Linq;
using qASIC.Core;

namespace qASIC.Console
{
    public class GameConsole : IService
    {
        public const string SYSTEM_NAME = "qASIC.Console";
        public const string SYSTEM_VERSION = "1.0.0";

        public GameConsole(GameCommandList commandList = null, ArgumentsParser parser = null) :
            this(Guid.NewGuid().ToString(), commandList, parser) { }

        public GameConsole(string name, GameCommandList commandList = null, ArgumentsParser parser = null) :
            this(null, name, commandList, parser) { }

        public GameConsole(qInstance instance, GameCommandList commandList = null, ArgumentsParser parser = null) :
            this(instance, Guid.NewGuid().ToString(), commandList, parser) { }

        public GameConsole(qInstance instance, string name, GameCommandList commandList = null, ArgumentsParser parser = null)
        {
            Instance = instance;

            Name = name;
            CommandList = commandList ?? new GameCommandList()
                .FindBuiltInCommands()
                .FindCommands()
                .FindAttributeCommands();

            CommandParser = parser ?? new QuashParser();

            qDebug.OnLog += QDebug_OnLog;
        }

        private void QDebug_OnLog(GameLog log)
        {
            if (LogQDebug)
                Log(log, 4, false);
        }

        /// <summary>Main static instance of <see cref="GameConsole"/> that was set using <see cref="SetAsMain"/>.</summary>
        public static GameConsole Main { get; private set; }
        /// <summary>Sets this instance as main to make it accessible from property <see cref="Main"/>.</summary>
        /// <returns>Returns itself.</returns>
        public GameConsole SetAsMain()
        {
            Main = this;
            return this;
        }

        private qInstance _instance;
        public qInstance Instance
        {
            get => _instance;
            set
            {
                var oldGetLogsFromInstance = GetLogsFromInstance;
                GetLogsFromInstance = false;

                Targets.StopSyncingWithOther(_instance?.RegisteredObjects);
                _instance = value;
                Targets.SyncWithOther(_instance?.RegisteredObjects);

                GetLogsFromInstance = oldGetLogsFromInstance;
            }
        }

        public string Name { get; private set; }

        public event Action<GameLog> OnLog;
        public event Action<GameLog> OnUpdateLog;

        public List<GameLog> Logs { get; internal set; } = new List<GameLog>();

        public GameCommandList CommandList { get; set; }
        public ArgumentsParser CommandParser { get; set; }

        public IGameCommand CurrentCommand { get; private set; } = null;
        public object ReturnedValue { get; private set; } = null;

        public GameConsoleTheme Theme { get; set; } = GameConsoleTheme.Default;

        /// <summary>Should the console log messages from <see cref="qDebug"/>.</summary>
        public bool LogQDebug { get; set; } = true;
        
        /// <summary>Determines if console should try looking for attributes that can change log messages and colors.</summary>
        public bool UseLogModifierAttributes { get; set; } = true;

        /// <summary>Determines if it should include exceptions when logging normal errors with executing commands.</summary>
        public bool IncludeStackTraceInCommandExceptions { get; set; } = true;

        /// <summary>Determines if it should include exceptions when logging unknown errors with executing commands.</summary>
        public bool IncludeStackTraceInUnknownCommandExceptions { get; set; } = false;

        /// <summary>Initializes reflections. This will happen automatically when reflections are needed, but it can cause lag, so it's better to do it once when the application launches.</summary>
        public void InitializeReflections() =>
            ConsoleReflections.Initialize();

        #region Registering targets
        public qRegisteredObjects Targets { get; private set; } = new qRegisteredObjects();
        #endregion

        #region Executing
        /// <summary>Can the console execute commands using <see cref="Execute(string)"/>.</summary>
        public bool CanParseAndExecute =>
            CommandList != null && CommandParser != null;

        /// <summary>Can the console execute commands using <see cref="Execute(ConsoleArgument[])"/>.</summary>
        public bool CanExecute =>
            CommandList != null;

        /// <summary>Executes a command.</summary>
        /// <param name="cmd">Command text that will be parsed and executed.</param>
        public object Execute(string cmd)
        {
            var args = new ConsoleArgument[0];

            if (!(ReturnedValue is CommandPrompt prompt) || prompt.ParseArguments)
            {
                if (CommandParser == null)
                    throw new Exception("Cannot parse commands with no parser!");

                args = CommandParser.ParseString(cmd);
            }

            var commandArgs = new CommandArgs()
            {
                inputString = cmd,
                commandName = CurrentCommand?.CommandName ?? (args.Length == 0 ? null : args[0].arg),
                args = args,
                console = this,
            };

            return Execute(commandArgs);
        }

        /// <summary>Executes a command.</summary>
        /// <param name="args">Command arguments.</param>
        public object Execute(CommandArgs args)
        {
            //Before
            if (CurrentCommand == null)
            {
                if (CommandList == null)
                    throw new Exception("Cannot execute commands with no command list!");

                if (!CommandList.TryGetCommand(args.commandName, out IGameCommand command))
                {
                    LogError($"Command {args.commandName} doesn't exist");
                    return null;
                }

                CurrentCommand = command;
            }

            if (ReturnedValue is CommandPrompt prompt)
            {
                args.prompt = prompt;

                if (!prompt.CanExecute(args))
                    return null;

                args.args = prompt.Prepare(args);
            }

            //Executing
            ReturnedValue = Execute(CurrentCommand.CommandName, () => CurrentCommand.Run(args));

            //After
            if (ReturnedValue is CommandPrompt)
            {             
                return ReturnedValue;
            }

            CurrentCommand = null;
            return ReturnedValue;
        }

        public object Execute(string commandName, Func<object> command, bool logOutput = true)
        {
            try
            {
                var output = command.Invoke();

                if (logOutput && output != null && !(output is CommandPrompt))
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

        #region Registering Loggables
        private bool _getLogsFromInstance = true;
        /// <summary>Whenever to log messages from <see cref="Instance"/>.</summary>
        public bool GetLogsFromInstance
        {
            get => _getLogsFromInstance;
            set
            {
                if (_getLogsFromInstance == value) return;
                _getLogsFromInstance = value;

                if (_instance == null) return;

                switch (_getLogsFromInstance)
                {
                    case true:
                        RegisterLoggable(_instance);
                        break;
                    case false:
                        DeregisterLoggable(_instance);
                        break;
                }
            }
        }

        /// <summary>Registers to messages from an <see cref="IHasLogs"/> and it's children.</summary>
        /// <param name="loggable">Loggable to register.</param>
        public void RegisterLoggable(IHasLogs loggable) =>
            loggable.Logs.OnLog += a => Log(a);

        /// <summary>Deregisters from messages from an <see cref="IHasLogs"/> and it's children.</summary>
        /// <param name="loggable">Loggable to deregister.</param>
        public void DeregisterLoggable(IHasLogs loggable) =>
            loggable.Logs.OnLog -= a => Log(a);
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
            if (Logs.Contains(log))
            {
                OnUpdateLog?.Invoke(log);
                return;
            }

            if (UseLogModifierAttributes && !overwriteColor)
            {
                var stackTrace = new StackTrace();
                var stackFrame = stackTrace.GetFrame(stackTraceIndex);

                var method = stackFrame?.GetMethod();
                var declaringType = method?.DeclaringType;

                if (TryGetColorAttributeOfTrace(method, declaringType, out var colorAttr))
                {
                    log.colorTag = colorAttr.ColorTag;
                    log.color = colorAttr.Color;
                }

                if (TryGetPrefixAttributeOfTrace(method, declaringType, out var prefixAttr))
                {
                    log.message = prefixAttr.FormatMessage(log.message);
                }
            }

            Logs.Add(log);
            OnLog?.Invoke(log);
        }

        /// <summary>Clears the console. Previous logs will still be there, but they won't show up in the output.</summary>
        public void Clear() =>
            Log(GameLog.CreateNow(string.Empty, LogType.Clear, qDebug.DEFAULT_COLOR_TAG));

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