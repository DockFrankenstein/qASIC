﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using qASIC.QML;

namespace qASIC.Options
{
    public class OptionsSerializer
    {
        public OptionsSerializer() : this($"{System.IO.Path.GetDirectoryName(Environment.ProcessPath)}/settings.txt") { }

        public OptionsSerializer(string path)
        {
            Path = path;

            OnSave = list =>
            {
                var serializer = new QmlSerializer();
                var doc = new QmlDocument();

                foreach (var item in list)
                    doc.AddEntry(item.Key, item.Value?.ToString());

                return serializer.Serialize(doc);
            };

            OnLoad = txt =>
            {
                var serializer = new QmlSerializer();
                var doc = serializer.Deserialize(txt);

                var dict = new Dictionary<string, object>();

                foreach (var item in doc.Where(x => x is QmlEntry).Select(x => x as QmlEntry).GroupBy(x => x.Path))
                    dict.Add(item.Key, item.First());

                return dict;
            };
        }

        public string Path { get; set; }

        public event Func<Dictionary<string, object>, string> OnSave;
        public event Func<string, Dictionary<string, object>> OnLoad;

        public void Save(OptionsList list)
        {
            if (string.IsNullOrWhiteSpace(Path))
                return;

            var itemsList = list
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.First().Value?.Value);

            var txt = OnSave(itemsList);
            var directory = System.IO.Path.GetDirectoryName(Path);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var writer = new StreamWriter(Path))
                writer.Write(txt);
        }

        public void Load(OptionsList list)
        {
            if (string.IsNullOrWhiteSpace(Path)) 
                return;

            using (var reader = new StreamReader(Path))
            {
                var txt = reader.ReadToEnd();
                var loadedItemList = OnLoad(txt);

                var loadedList = new OptionsList();

                foreach (var item in loadedItemList)
                    loadedList.Set(item.Key, item.Value, true);

                list.MergeList(loadedList);
            }
        }
    }
}