using System;
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
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        public string PathPrefix { get; private set; }
        List<QmlElement> Elements { get; set; } = new List<QmlElement>();
        Dictionary<string, List<QmlEntry>> EntryMap { get; set; } = new Dictionary<string, List<QmlEntry>>();

        public void Clear() =>
            Elements.Clear();

        public int Count =>
            Elements.Count;

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

        public QmlDocument AddEntry(string path, object value) =>
            AddElement(new QmlEntry($"{PathPrefix}{path}", path, value));

        public QmlDocument StartArrayEntry(string path) =>
            AddElement(new QmlEntry($"{PathPrefix}{path}", path, string.Empty)
            {
                IsArrayStart = true
            });

        public QmlDocument AddArrayItem(object value)
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

        #region Getting elements
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
        #endregion

        #region Getting Single Value
        public T GetValue<T>(string path, T defaultValue = default) =>
            QmlUtility.ParseValue<T>(GetEntry(path)?.Value, defaultValue);

        public object GetValue(string path, Type type, object defaultValue = null) =>
            QmlUtility.ParseValue(type, GetEntry(path)?.Value, defaultValue);

        public bool TryGetValue<T>(string path, out T result) =>
            TryGetValue(path, default, out result);

        public bool TryGetValue<T>(string path, T defaultValue, out T result) =>
            QmlUtility.TryParseValue(GetEntry(path)?.Value, defaultValue, out result);

        public bool TryGetValue(string path, Type type, out object result) =>
            TryGetValue(path, type, default, out result);

        public bool TryGetValue(string path, Type type, object defaultValue, out object result) =>
            QmlUtility.TryParseValue(type, GetEntry(path)?.Value, defaultValue, out result);
        #endregion

        #region Getting Array Value
        public List<T> GetValues<T>(string path)
        {
            var list = new List<T>();
            foreach (var entry in GetEntries(path))
                if (entry.TryGetValue<T>(out T obj))
                    list.Add(obj);

            return list;
        }

        public List<object> GetValues(Type type, string path)
        {
            List<object> list = new List<object>();
            foreach (var entry in GetEntries(path))
                if (entry.TryGetValue(type, out object obj))
                    list.Add(obj);

            return list;
        }
        #endregion

        #region Setting Single Value
        public QmlDocument SetValue(string path, object value)
        {
            var entry = GetEntry(path);
            if (entry == null)
            {
                AddEntry(path, value);
                entry = GetLastElementOfType<QmlEntry>();
            }

            entry.Value = value?.ToString() ?? string.Empty;
            return this;
        }

        public QmlDocument SetValues(string path, object[] values)
        {
            var entries = GetEntries(path);
            var valueCount = values.Count();
            int min = Math.Min(valueCount, entries.Length);
            int max = Math.Max(valueCount, entries.Length);
            bool moreValues = valueCount > entries.Length;

            for (int i = 0; i < min; i++)
                entries[i].Value = values[i]?.ToString() ?? string.Empty;

            if (moreValues)
            {
                var insertAtIndex = entries.Length > 0 ?
                    Elements.IndexOf(entries[min - 1]) + 1 :
                    -1;

                var relativePath = entries.Length > 0 ?
                    entries[min - 1].RelativePath :
                    path;

                if (insertAtIndex == -1)
                {
                    //Finish if in group
                    if (GetLastElementOfType<QmlGroupBorder>()?.IsEnding == false)
                    {
                        FinishGroup();
                        AddSpace();
                    }

                    StartArrayEntry(path);
                    insertAtIndex = Elements.Count;
                }

                for (int i = min; i < max; i++)
                {
                    Elements.Insert(insertAtIndex, new QmlEntry(path, relativePath, values[i]));
                    insertAtIndex++;
                }

                return this;
            }

            for (int i = min; i < max; i++)
                Elements.Remove(entries[i]);

            return this;
        }
        #endregion

        public IEnumerator<QmlElement> GetEnumerator() =>
            Elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}