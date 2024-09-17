namespace qASIC.QML
{
    public abstract class QmlElement
    {
        public abstract string CreateContent();
        public abstract bool ShouldParse(QmlProcessedDocument processed, QmlDocument doc);
        public abstract void Parse(QmlProcessedDocument processed, QmlDocument doc);
    }
}