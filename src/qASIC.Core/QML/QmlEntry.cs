using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

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

        public override string CreateContent() =>
            $"{Path}: {QmlUtility.PrepareValueStringForExport(Value)}\n";

        public override bool ShouldParse(QmlProcessedDocument doc) =>
            doc.PeekLine().Contains(":");

        public override QmlElement Parse(QmlProcessedDocument doc)
        {
            var mainLineParts = doc.GetLine().Split(":");
            var path = mainLineParts[0];
            var relativePath = $"{doc.PathPrefix}{path}";
            var txt = string.Join(":", mainLineParts.Skip(1))
                .Trim();

            var quoteCount = 0;
            while (quoteCount < txt.Length && txt[quoteCount] == '\"')
                quoteCount++;

            //If it doesn't start with an odd amount of quotations,
            //we can just skip the rest and replace double quotations
            //with single ones
            if (quoteCount % 2 == 0)
                return new QmlEntry(path, relativePath, txt.Replace("\"\"", "\""));

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

                if (doc.FinishedReading)
                    break;

                //Move to the next line
                value.Append('\n');
                txt = doc.GetLine();
            }

            return new QmlEntry(path, relativePath, value.ToString());
        }
    }
}