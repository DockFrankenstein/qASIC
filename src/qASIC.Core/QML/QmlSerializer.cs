using System.Collections.Generic;
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
            var doc = new QmlProcessedDocument(txt);

            var elements = new List<QmlElement>();

            while (!doc.FinishedReading)
            {
                bool parsed = false;
                foreach (var item in DeserializeElements)
                {
                    if (!item.ShouldParse(doc)) continue;
                    var el = item.Parse(doc);
                    if (el != null)
                        elements.Add(el);

                    parsed = true;
                    break;
                }

                if (!parsed)
                    doc.GetLine();
            }

            return new QmlDocument(elements);
        }
    }
}