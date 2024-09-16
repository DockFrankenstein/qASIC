namespace qASIC.QML
{
    public class QmlGroupBorder : QmlElement
    {
        public QmlGroupBorder() : base() { }
        public QmlGroupBorder(string path)
        {
            Path = path;
        }

        public string Path { get; set; }

        public override string CreateContent() =>
            string.IsNullOrWhiteSpace(Path) ?
            "---\n" :
            $"--- {Path} ---\n";

        public override bool ShouldParse(QmlProcessedDocument doc) =>
            doc.PeekLine().StartsWith('-');

        public override QmlElement Parse(QmlProcessedDocument doc)
        {
            var line = doc.GetLine()
                .Trim('-')
                .Trim();

            if (string.IsNullOrEmpty(line))
            {
                doc.PathPrefix = string.Empty;
                return new QmlGroupBorder();
            }

            doc.PathPrefix = $"{line}.";
            return new QmlGroupBorder(line);
        }
    }
}