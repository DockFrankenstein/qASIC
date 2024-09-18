using qASIC.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace qASIC.QML
{
    public static class QmlUtility
    {
        public static string FormatString(string s) =>
            (s ?? string.Empty)
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");

        public static string PrepareValueStringForExport(string s) =>
            (s.Contains("\n") || s.StartsWith(" ") || s.EndsWith(" ")) ?
            $"\"{s.Replace("\"", "\"\"")}\"" :
            s.Replace("\"", "\"\"");

        public static List<ValueParser> Parsers { get; private set; } = new List<ValueParser>(ValueParser.CreateStandardParserArray());

        public static T ParseValue<T>(string value, T defaultValue) =>
            (T)ParseValue(typeof(T), value, defaultValue);

        public static object ParseValue(Type type, string value, object defaultValue)
        {
            if (TryParseValue(type, value, defaultValue, out object obj))
                return obj;

            return defaultValue;
        }

        public static bool TryParseValue<T>(string value, T defaultValue, out T result)
        {
            var b = TryParseValue(typeof(T), value, defaultValue, out object obj);
            result = (T)obj;
            return b;
        }

        public static bool TryParseValue(Type type, string value, object defaultValue, out object result)
        {
            result = null;
            var targets = Parsers.Where(x => x.ValueType == type);

            foreach (var target in targets)
            {
                if (!target.TryParse(value, out result)) continue;
                return true;
            }

            result = defaultValue;
            return false;
        }
    }
}