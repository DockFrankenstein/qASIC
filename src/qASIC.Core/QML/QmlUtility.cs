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
    }
}