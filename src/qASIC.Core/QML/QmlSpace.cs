using System;

namespace qASIC.QML
{
    public class QmlSpace : QmlElement
    {
        public QmlSpace(int count = 1) : base() 
        {
            Count = count;
        }

        private int count;
        public int Count 
        {
            get => count;
            set => count = Math.Max(value, 1);
        }

        public override string CreateContent() =>
            new string('\n', Count);

        public override bool ShouldParse(QmlProcessedDocument processed, QmlDocument doc) =>
            string.IsNullOrWhiteSpace(processed.PeekLine());

        public override void Parse(QmlProcessedDocument processed, QmlDocument doc)
        {
            int i = 0;
            while (!processed.FinishedReading && string.IsNullOrWhiteSpace(processed.PeekLine()))
            {
                i++;
                processed.GetLine();
            }

            doc.AddElement(new QmlSpace(i));
        }
    }
}