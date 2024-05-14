using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace qASIC.Options
{
    public class OptionsList : IEnumerable<KeyValuePair<string, OptionsList.ListItem>>
    {
        /// <summary>Default binding flags used for finding option attributes.</summary>
        public const BindingFlags DEFAULT_OPTION_BINDING_FLAGS = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private Dictionary<string, ListItem> Values = new Dictionary<string, ListItem>();

        public event Action<ListItem> OnChanged;

        #region Dictionary
        public ListItem this[string key]
        {
            get => Values[OptionsManager.FormatKeyString(key)];
            set => Values[OptionsManager.FormatKeyString(key)] = value; 
        }

        public int Count => Values.Count;

        public void Set(string key, ListItem value)
        {
            key = OptionsManager.FormatKeyString(key);

            if (Values.ContainsKey(key))
            {
                Values[key] = value;
                return;
            }

            Values.Add(key, value);
            value.OnValueChanged += _ => OnChanged?.Invoke(value);
        }

        public void Set(string key, object value)
        {
            key = OptionsManager.FormatKeyString(key);

            if (Values.ContainsKey(key))
            {
                Values[key].Value = value;
                return;
            }

            Values[key].Value = value;
        }

        public void Clear() =>
            Values.Clear();

        public bool ContainsKey(string key) =>
            Values.ContainsKey(OptionsManager.FormatKeyString(key));

        public IEnumerator<KeyValuePair<string, ListItem>> GetEnumerator() =>
            Values.GetEnumerator();

        public bool Remove(string key)
        {
            key = OptionsManager.FormatKeyString(key);
            if (!Values.ContainsKey(key)) 
                return false;

            var value = Values[key];
            value.OnValueChanged -= _ => OnChanged?.Invoke(value);

            return Values.Remove(key);
        }

        public bool TryGetValue(string key, out ListItem value) =>
            Values.TryGetValue(OptionsManager.FormatKeyString(key), out value);

        IEnumerator IEnumerable.GetEnumerator() =>
            Values.GetEnumerator();
        #endregion

        public void MergeList(OptionsList list)
        {
            foreach (var item in list)
            {
                if (Values.ContainsKey(item.Key))
                {
                    Values[item.Key] = item.Value;
                    continue;
                }
                
                Values.Add(item.Key, item.Value);
            }
        }

        public void EnsureTargets(OptionTargetList list)
        {
            foreach (var item in list)
            {
                if (ContainsKey(item.Key)) break;

                var key = OptionsManager.FormatKeyString(item.Key);

                object value = null;
                object defaultValue = null;

                switch (list.TryGetValue(key, out object val), 
                    list.TryGetDefalutValue(key, out object defaultVal))
                {
                    case (true, false):
                        value = val;
                        defaultValue = val;
                        break;
                    case (false, true):
                        value = defaultVal;
                        defaultValue = defaultVal;
                        break;
                    case (true, true):
                        value = val;
                        defaultValue = defaultVal;
                        break;
                }

                var listItem = new ListItem(key, value, defaultValue);

                Set(key, listItem);
            }
        }

        public class ListItem
        {
            public ListItem(string name) : this(name, default, default) { }
            public ListItem(string name, object value) : this(name, value, value) { }

            public ListItem(string name, object value, object defaultValue)
            {
                Name = name;
                _value = value;
                DefaultValue = defaultValue;
            }

            public event Action<object> OnValueChanged;

            public string Name { get; private set; }

            object _value;
            public object Value 
            { 
                get => _value; 
                set
                {
                    _value = value;
                    OnValueChanged(value);
                }
            }

            public object DefaultValue { get; set; }

            public void ResetToDefault() =>
                Value = DefaultValue;

            public override string ToString() =>
                $"{Name}: {Value} (default: {DefaultValue})";
        }
    }
}