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

        public bool IsEnding =>
            string.IsNullOrWhiteSpace(Path);

        public override string CreateContent() =>
            IsEnding ?
            "---\n" :
            $"--- {Path} ---\n";

        public override bool ShouldParse(QmlProcessedDocument processed, QmlDocument doc) =>
            processed.PeekLine().StartsWith('-');

        public override void Parse(QmlProcessedDocument processed, QmlDocument doc)
        {
            var line = processed.GetLine()
                .Trim('-')
                .Trim();

            if (string.IsNullOrEmpty(line))
            {
                doc.AddElement(new QmlGroupBorder());
                return;
            }

            doc.AddElement(new QmlGroupBorder(line));
        }
    }
}