namespace qASIC.QML
{
    public abstract class QmlElement
    {
        public abstract string CreateContent();
        public abstract bool ShouldParse(QmlProcessedDocument doc);
        public abstract QmlElement Parse(QmlProcessedDocument doc);
    }
}