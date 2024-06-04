using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace qASIC.Core.Serialization.Serializers
{
    public class ConfigValue : IEnumerable<KeyValuePair<string, ConfigValue>>
    {
        public ConfigValue() : this(string.Empty)
        {

        }

        public ConfigValue(string value)
        {
            Value = value;
        }

        public ConfigValue(string[] array)
        {
            ArrayValue = array;
        }

        Dictionary<string, ConfigValue> arrays = new Dictionary<string, ConfigValue>();

        public string Comment { get; set; }
        public string Value { get; set; }
        public string[] ArrayValue { get; set; } = new string[0];

        internal string GetFinalCommentString(string indent = "") =>
            string.IsNullOrEmpty(Comment) ?
            string.Empty :
            $"{indent}#{Comment.Replace("\n", $"\n{indent}#")}\n";

        public ConfigValue this[string names]
        {
            get => arrays[names];
        }

        public ConfigValue Add(string name, ConfigValue value)
        {
            if (arrays.ContainsKey(name))
            {
                arrays[name] = value;
                return this;
            }

            arrays.Add(name, value);
            return this;
        }

        public ConfigValue Add(string name, string value) =>
            Add(name, new ConfigValue(value));

        public string[] GetArrayValues() =>
            arrays.Select(x => x.Key).ToArray();

        public int Count =>
            arrays.Count();

        public bool Contains(string name) =>
            arrays.ContainsKey(name);

        public IEnumerator<KeyValuePair<string, ConfigValue>> GetEnumerator() =>
            arrays.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}