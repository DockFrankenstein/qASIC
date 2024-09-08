using qASIC.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace qASIC
{
    public static class ILoggableExtensions
    {
        /// <summary>Gets an <see cref="IEnumerator{ILoggable}"/> of the root and all loggables contained within it.</summary>
        /// <param name="root">The root loggable.</param>
        /// <returns>Returns an <see cref="IEnumerator{ILoggable}"/> of the root and all loggables contained within it.</returns>
        public static IEnumerable<ILoggable> GetAllLoggables(this ILoggable root)
        {
            IEnumerable<ILoggable> list = new ILoggable[]
            {
                root
            };

            foreach (var item in root.Loggables)
                list.Concat(item.GetAllLoggables());

            return list;
        }

        /// <summary>Registers an on log event in the root loggable and every other one within it.</summary>
        /// <param name="root">The root loggable.</param>
        /// <param name="onLog">The event to register.</param>
        public static void RegisterOnLogEvent(this ILoggable root, Action<qLog> onLog)
        {
            var items = root.GetAllLoggables();
            foreach (var item in items)
                item.Logs.OnLog += onLog;
        }

        /// <summary>Unregisters an on log event in the root loggable and every other one within it.</summary>
        /// <param name="root">The root loggable.</param>
        /// <param name="onLog">The event to deregister.</param>
        public static void DeregisterOnLogEvent(this ILoggable root, Action<qLog> onLog)
        {
            var items = root.GetAllLoggables();
            foreach (var item in items)
                item.Logs.OnLog -= onLog;
        }
    }
}