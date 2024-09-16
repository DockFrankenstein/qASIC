using System.Collections;
using System.Collections.Generic;

namespace qASIC.QML
{
    public class QmlProcessedDocument : IEnumerable<string>
    {
        public QmlProcessedDocument(string txt) : this(QmlUtility.FormatString(txt).Split("\n")) { }
        public QmlProcessedDocument(string[] lines)
        {
            Lines = lines;
        }

        public string[] Lines { get; set; }
        public int Position { get; set; }

        public string PathPrefix { get; set; }

        public bool FinishedReading =>
            Position >= Lines.Length;

        public string GetLine()
        {
            var line = PeekLine();
            Position++;
            return line;
        }

        public string PeekLine() =>
            Lines[Position];

        public IEnumerator<string> GetEnumerator() =>
            Lines.GetEnumerator() as IEnumerator<string>;

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}