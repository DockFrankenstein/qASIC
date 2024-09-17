using System.Text;

namespace qASIC.QML
{
    public class QmlSerializer
    {
        public QmlElement[] DeserializeElements { get; set; } = new QmlElement[]
        {
            new QmlSpace(),
            new QmlComment(),
            new QmlGroupBorder(),
            new QmlEntry(),
        };

        public string Serialize(QmlDocument document)
        {
            var txt = new StringBuilder();

            foreach (var element in document)
                txt.Append(element.CreateContent());

            var finalTxt = txt.ToString();

            if (finalTxt.EndsWith("\n"))
                finalTxt = finalTxt.Substring(0, finalTxt.Length - 1);

            return finalTxt;
        }

        public QmlDocument Deserialize(string txt)
        {
            var processed = new QmlProcessedDocument(txt);
            var doc = new QmlDocument();

            while (!processed.FinishedReading)
            {
                bool parsed = false;
                foreach (var item in DeserializeElements)
                {
                    if (!item.ShouldParse(processed, doc)) continue;
                    item.Parse(processed, doc);
                    parsed = true;
                    break;
                }

                if (!parsed)
                    processed.GetLine();
            }

            return doc;
        }
    }
}