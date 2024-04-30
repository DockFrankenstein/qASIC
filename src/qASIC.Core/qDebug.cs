using System;

using GameLog = qASIC.qLog;

namespace qASIC
{
    public static partial class qDebug
    {
        public const string DEFAULT_COLOR_TAG = "default";
        public const string WARNING_COLOR_TAG = "warning";
        public const string ERROR_COLOR_TAG = "error";
        
        public static event Action<GameLog> OnLog;

        public static void Log(object message) =>
            OnLog?.Invoke(GameLog.CreateNow(message?.ToString() ?? "NULL", DEFAULT_COLOR_TAG));

        public static void LogWarning(object message) =>
            OnLog?.Invoke(GameLog.CreateNow(message?.ToString() ?? "NULL", WARNING_COLOR_TAG));

        public static void LogError(object message) =>
            OnLog?.Invoke(GameLog.CreateNow(message?.ToString() ?? "NULL", ERROR_COLOR_TAG));

        public static void Log(object message, string colorTag) =>
            OnLog?.Invoke(GameLog.CreateNow(message?.ToString() ?? "NULL", colorTag));

        public static void Log(object message, qColor color) =>
            OnLog?.Invoke(GameLog.CreateNow(message?.ToString() ?? "NULL", color));
    }
}