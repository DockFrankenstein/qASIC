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

        public override bool ShouldParse(QmlProcessedDocument doc) =>
            string.IsNullOrWhiteSpace(doc.PeekLine());

        public override QmlElement Parse(QmlProcessedDocument doc)
        {
            int i = 0;
            while (!doc.FinishedReading && string.IsNullOrWhiteSpace(doc.PeekLine()))
            {
                i++;
                doc.GetLine();
            }

            return new QmlSpace(i);
        }
    }
}