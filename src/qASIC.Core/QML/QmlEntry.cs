using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace qASIC.QML
{
    public class QmlEntry : QmlElement
    {
        public QmlEntry() : base() { }
        public QmlEntry(string relativePath, string value) : this(relativePath, relativePath, value) { }

        public QmlEntry(string path, string relativePath, string value)
        {
            Path = path;
            RelativePath = relativePath;
            Value = value;
        }

        public string Path { get; set; }
        public string RelativePath { get; set; }

        private string value;
        public string Value
        {
            get => value;
            set => this.value = QmlUtility.FormatString(value);
        }

        public bool IsArrayItem { get; set; }
        public bool IsArrayStart { get; set; }

        public T GetValue<T>() =>
            (T)GetValue(typeof(T));

        public object GetValue(Type type)
        {
            try
            {
                return Convert.ChangeType(Value, type);
            }
            catch
            {
                return null;
            }
        }

        public override string CreateContent()
        {
            if (IsArrayStart)
                return $"{RelativePath}|\n";

            if (IsArrayItem)
                return $"* {QmlUtility.PrepareValueStringForExport(Value)}\n";

            return $"{RelativePath}: {QmlUtility.PrepareValueStringForExport(Value)}\n";
        }

        public override bool ShouldParse(QmlProcessedDocument processed, QmlDocument doc)
        {
            var line = processed.PeekLine();
            return line.Contains(':') || line.TrimEnd().EndsWith('|') || line.TrimStart().StartsWith('*');
        }

        public override void Parse(QmlProcessedDocument processed, QmlDocument doc)
        {
            var line = processed.GetLine();

            bool isArrayStart = line.TrimEnd().EndsWith('|');
            bool isArrayItem = line.TrimStart().StartsWith('*');

            var relativePath = string.Empty;
            var txt = string.Empty;
            var group = doc.GetLastElementOfType<QmlGroupBorder>();

            if (isArrayStart)
            {
                relativePath = line.TrimEnd();
                relativePath = relativePath.Substring(0, relativePath.Length - 1);
            }

            if (!isArrayStart && isArrayItem)
            {
                var item = doc.Where(x => x is QmlEntry e && !e.IsArrayItem)
                    .LastOrDefault() as QmlEntry;

                //If there is no array start or any entry before, ignore
                if (item == null)
                    return;

                relativePath = item.RelativePath;
                txt = line.TrimStart();
                txt = txt.Substring(1, txt.Length - 1)
                    .Trim();
            }

            if (!isArrayStart && !isArrayItem)
            {
                var mainLineParts = line.Split(":");
                relativePath = mainLineParts[0];
                txt = string.Join(":", mainLineParts.Skip(1))
                    .Trim();
            }

            var path = relativePath;

            if (group != null && !group.IsEnding)
                path = $"{group.Path}.{relativePath}";

            doc.AddElement(new QmlEntry(path, relativePath, isArrayStart ? string.Empty : GetValue(processed, txt))
            {
                IsArrayStart = isArrayStart,
                IsArrayItem = isArrayItem,
            });
        }

        string GetValue(QmlProcessedDocument processed, string txt)
        {
            var quoteCount = 0;
            while (quoteCount < txt.Length && txt[quoteCount] == '\"')
                quoteCount++;

            //If it doesn't start with an odd amount of quotations,
            //we can just skip the rest and replace double quotations
            //with single ones
            if (quoteCount % 2 == 0)
                return txt.Replace("\"\"", "\"");

            txt = txt.Substring(1, txt.Length - 1);

            var value = new StringBuilder();
            while (true)
            {
                int emptyCount = 0;
                var txtParts = txt.Split('\"');

                for (int i = 0; i < txtParts.Length; i++)
                {
                    bool empty = string.IsNullOrEmpty(txtParts[i]);

                    if (empty)
                    {
                        emptyCount++;

                        //If two empty parts are after each other,
                        //we can reset the count and add a "
                        if (emptyCount == 2)
                        {
                            value.Append('\"');
                            emptyCount = 0;
                        }

                        continue;
                    }

                    //If it's not empty

                    if (i != 0) //Ignore if first
                    {
                        //If two non-empty parts were after each other,
                        //it means there was a single quotation mark
                        //between them that marks the end
                        if (emptyCount == 0)
                        {
                            //Add empty count to properly exit loop
                            emptyCount++;
                            break;
                        }

                        value.Append('\"');
                    }

                    emptyCount = 0;
                    value.Append(txtParts[i]);
                }

                if (emptyCount == 1)
                    break;

                if (processed.FinishedReading)
                    break;

                //Move to the next line
                value.Append('\n');
                txt = processed.GetLine();
            }

            return value.ToString();
        }
    }
}