using System;
using System.Text;

namespace qASIC.QML
{
    public class QmlComment : QmlElement
    {
        public QmlComment() : base() { }
        public QmlComment(string comment)
        {
            Comment = comment;
        }

        private string comment;
        public string Comment
        {
            get => comment;
            set => comment = QmlUtility.FormatString(value);
        }

        public override string CreateContent() =>
            $"# {Comment.Replace("\n", "\n# ")}\n";

        public override bool ShouldParse(QmlProcessedDocument processed, QmlDocument doc) =>
            processed.PeekLine().Trim().StartsWith("#");

        public override void Parse(QmlProcessedDocument processed, QmlDocument doc)
        {
            var comment = new StringBuilder();
            while (processed.PeekLine().TrimStart().StartsWith("#"))
            {
                var line = processed.GetLine().TrimStart();
                var trimCount = Math.Min(2, line.Length);
                line = line.Substring(trimCount, line.Length - trimCount);
                comment.Append($"\n{line}");
            }

            var txt = comment.ToString();
            doc.AddElement(new QmlComment(txt.Substring(1, txt.Length - 1)));
        }
    }
}