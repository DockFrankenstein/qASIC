using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

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
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        public string PathPrefix { get; private set; }
        List<QmlElement> Elements { get; set; } = new List<QmlElement>();
        Dictionary<string, List<QmlEntry>> EntryMap { get; set; } = new Dictionary<string, List<QmlEntry>>(); 

        #region Adding
        public QmlDocument AddElement(QmlElement element)
        {
            Elements.Add(element);
            if (element is QmlEntry entry)
            {
                if (!EntryMap.ContainsKey(entry.Path))
                    EntryMap.Add(entry.Path, new List<QmlEntry>());

                EntryMap[entry.Path].Add(entry);
            }

            return this;
        }

        public QmlDocument AddEntry(string path, string value) =>
            AddElement(new QmlEntry($"{PathPrefix}{path}", path, value));

        public QmlDocument StartArrayEntry(string path) =>
            AddElement(new QmlEntry($"{PathPrefix}{path}", path, string.Empty)
            {
                IsArrayStart = true
            });

        public QmlDocument AddArrayItem(string value)
        {
            var prevEntry = GetLastElementOfType<QmlEntry>();
            return AddElement(new QmlEntry(prevEntry?.Path ?? string.Empty, prevEntry?.RelativePath ?? string.Empty, value)
            {
                IsArrayItem = true,
            });
        }

        public QmlDocument StartGroup(string groupPath)
        {
            PathPrefix = string.IsNullOrWhiteSpace(groupPath) ?
                string.Empty :
                $"{groupPath}.";

            return AddElement(new QmlGroupBorder(groupPath));
        }

        public QmlDocument FinishGroup() =>
            StartGroup(string.Empty);

        public QmlDocument AddComment(string comment) =>
            AddElement(new QmlComment(comment));

        public QmlDocument AddSpace() =>
            AddElement(new QmlSpace());
        #endregion

        #region Getting
        public QmlEntry GetEntry(string path) =>
            EntryMap.TryGetValue(path, out var val) ?
            val.Where(x => !x.IsArrayStart).FirstOrDefault() :
            null;

        public QmlEntry[] GetEntries(string path) =>
            EntryMap.TryGetValue(path, out var val) ?
            val.Where(x => !x.IsArrayStart).ToArray() :
            new QmlEntry[0];
        
        public T GetLastElementOfType<T>() where T : QmlElement
        {
            for (int i = Elements.Count - 1; i >= 0; i--)
            {
                if (Elements[i] is T el)
                    return el;
            }

            return null;
        }

        public string GetValue(string path, string defaultValue = null) =>
            GetEntry(path)?.Value ?? defaultValue;

        public string[] GetValues(string path) =>
            GetEntries(path).Select(x => x.Value).ToArray();
        #endregion

        public IEnumerator<QmlElement> GetEnumerator() =>
            Elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}