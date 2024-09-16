using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace qASIC.QML
{
    public class QmlDocument : IEnumerable<QmlElement>
    {
        public QmlDocument() { }
        public QmlDocument(IEnumerable<QmlElement> elements)
        {
            Elements = elements.ToList();
            EntryMap = elements
                .Where(x => x is QmlEntry)
                .Select(x => x as QmlEntry)
                .GroupBy(x => x.Path)
                .ToDictionary(x => x.Key, x => x.First());
        }

        public string PathPrefix { get; private set; }
        List<QmlElement> Elements { get; set; } = new List<QmlElement>();
        Dictionary<string, QmlEntry> EntryMap { get; set; } = new Dictionary<string, QmlEntry>(); 

        #region Adding
        public QmlDocument AddElement(QmlElement element)
        {
            Elements.Add(element);
            if (element is QmlEntry entry && !EntryMap.ContainsKey(entry.Path))
                EntryMap.Add(entry.Path, entry);

            return this;
        }

        public QmlDocument AddEntry(string path, string value) =>
            AddElement(new QmlEntry($"{PathPrefix}{path}", path, value));

        public QmlDocument AddGroup(string groupPath)
        {
            PathPrefix = string.IsNullOrWhiteSpace(groupPath) ?
                string.Empty :
                $"{groupPath}.";

            return AddElement(new QmlGroupBorder(groupPath));
        }

        public QmlDocument FinishGroup() =>
            AddGroup(string.Empty);

        public QmlDocument AddComment(string comment) =>
            AddElement(new QmlComment(comment));

        public QmlDocument AddSpace() =>
            AddElement(new QmlSpace());
        #endregion

        #region Getting
        public QmlEntry GetEntry(string path) =>
            EntryMap.TryGetValue(path, out var val) ?
            val :
            null;
        #endregion

        public IEnumerator<QmlElement> GetEnumerator() =>
            Elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}