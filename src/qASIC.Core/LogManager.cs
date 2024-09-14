﻿using qASIC.Core;
using System;

namespace qASIC
{
    public class LogManager
    {
        public LogManager() : this(qDebug.DEFAULT_COLOR_TAG, qDebug.WARNING_COLOR_TAG, qDebug.ERROR_COLOR_TAG) { }
        public LogManager(string defaultColorTag) : this(defaultColorTag, qDebug.WARNING_COLOR_TAG, qDebug.ERROR_COLOR_TAG) { }
        public LogManager(string defaultColorTag, string warningColor, string errorColor)
        {
            DefaultColorTag = defaultColorTag;
        }

        public string DefaultColorTag { get; set; }
        public string WarningColorTag { get; set; }
        public string ErrorColorTag { get; set; }

        #region Logging
        public event Action<qLog> OnLog;

        public void Log(qLog log)
        {
            OnLog?.Invoke(log);
        }

        public void Log(string message, string colorTag)
        {
            OnLog?.Invoke(qLog.CreateNow(message, colorTag));
        }

        public void Log(string message, qColor color)
        {
            OnLog?.Invoke(qLog.CreateNow(message, color));
        }

        public void Log(string message)
        {
            OnLog?.Invoke(qLog.CreateNow(message, DefaultColorTag));
        }

        public void LogWarning(string message)
        {
            OnLog?.Invoke(qLog.CreateNow(message, WarningColorTag));
        }

        public void LogError(string message)
        {
            OnLog?.Invoke(qLog.CreateNow(message, ErrorColorTag));
        }
        #endregion

        #region Loggables
        /// <summary>Subscribes to messages from a <see cref="IHasLogs"/>.</summary>
        /// <param name="loggable">The loggable to register.</param>
        /// <returns>Returns itself.</returns>
        public LogManager RegisterLoggable(IHasLogs loggable)
        {
            loggable.Logs.OnLog += Log;
            return this;
        }

        /// <summary>Unsubscribes from messages from a <see cref="IHasLogs"/>.</summary>
        /// <param name="loggable">The loggable to deregister.</param>
        /// <returns>Returns itself.</returns>
        public LogManager UnregisterLoggable(IHasLogs loggable)
        {
            loggable.Logs.OnLog -= Log;
            return this;
        }
        #endregion
    }
}