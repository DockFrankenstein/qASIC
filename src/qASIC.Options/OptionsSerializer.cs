using System;
using qASIC.Serialization.Serializers;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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
                var serializer = new ConfigSerializer();
                return serializer.Serialize(list);
            };

            OnLoad = txt =>
            {
                var serializer = new ConfigSerializer();
                return serializer.Deserialize<Dictionary<string, object>>(txt);
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

            //using (var writer = new StreamWriter(Path))
            //    writer.Write(txt);
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