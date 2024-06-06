using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace qASIC.Options
{
    [Serializable]
    public class OptionsList : IEnumerable<KeyValuePair<string, OptionsList.ListItem>>
    {
        /// <summary>Default binding flags used for finding option attributes.</summary>
        public const BindingFlags DEFAULT_OPTION_BINDING_FLAGS = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private Dictionary<string, ListItem> Values = new Dictionary<string, ListItem>();

        public event Action<ListItem[]> OnValueSet;

        #region Dictionary
        public ListItem this[string key]
        {
            get => Values[OptionsManager.FormatKeyString(key)];
        }

        public int Count => Values.Count;

        void Set(string key, ListItem value, bool silent = false)
        {
            key = OptionsManager.FormatKeyString(key);

            value.OnValueChanged += _ => OnValueSet?.Invoke(new ListItem[] { value });

            if (Values.ContainsKey(key))
            {
                Values[key] = value;

                if (!silent)
                    OnValueSet?.Invoke(new ListItem[] { value });
                return;
            }

            Values.Add(key, value);

            if (!silent)
                OnValueSet?.Invoke(new ListItem[] { value });
        }

        /// <summary>Sets the value of a specified item.</summary>
        /// <param name="key">Name of the item.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="silent">When true, this method won't invoke any events.</param>
        public void Set(string key, object value, bool silent = false)
        {
            key = OptionsManager.FormatKeyString(key);

            if (Values.ContainsKey(key))
            {
                Values[key].ChangeValue(value, silent);
                return;
            }

            var item = new ListItem(key, value);

            Values.Add(key, item);
            item.OnValueChanged += _ => OnValueSet?.Invoke(new ListItem[] { item });
            item.ChangeValue(value, silent);
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

            value.OnValueChanged -= _ => OnValueSet?.Invoke(new ListItem[] { value });

            return Values.Remove(key);
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <returns><c>true</c> if the <see cref="OptionsList"/> contains an element with the specified key; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string key, out ListItem value) =>
            Values.TryGetValue(OptionsManager.FormatKeyString(key), out value);

        IEnumerator IEnumerable.GetEnumerator() =>
            Values.GetEnumerator();
        #endregion

        /// <summary>Merge items of a different list into this one.</summary>
        /// <param name="list">List to merge with this one.</param>
        /// <param name="silent">When true, this method won't invoke any events.</param>
        public void MergeList(OptionsList list, bool silent = false)
        {
            var items = list
                .Except(this)
                .Select(x => x.Value)
                .ToArray();

            foreach (var item in items)
            {
                //Set only value if item already exists
                if (ContainsKey(item.Name))
                {
                    Set(item.Name, item.Value, true);
                    continue;
                }

                //Set the entire item (including default) if doesn't exists
                Set(item.Name, item, true);
            }

            if (!silent && items.Length > 0)
                OnValueSet?.Invoke(items);
        }

        /// <summary>Ensure this list contains every item that's specified in a target list.</summary>
        /// <param name="list">The target list to pull items from.</param>
        public void EnsureTargets(OptionTargetList list, qRegisteredObjects registeredObjects)
        {
            foreach (var item in list)
            {
                if (ContainsKey(item.Key)) break;

                var key = OptionsManager.FormatKeyString(item.Key);

                object value = null;
                object defaultValue = null;

                switch (list.TryGetValue(registeredObjects, key, out object val), 
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

        [Serializable]
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

            /// <summary>Name of the item.</summary>
            public string Name { get; private set; }

            object _value;

            public object Value 
            { 
                get => _value; 
                set
                {
                    ChangeValue(value);
                }
            }

            /// <summary>Changes the value of the item.</summary>
            /// <param name="value">The new item of the item.</param>
            /// <param name="silent">When true, this method won't invoke any events.</param>
            public void ChangeValue(object value, bool silent = false)
            {
                _value = value;
                if (!silent)
                    OnValueChanged?.Invoke(value);
            }

            public object DefaultValue { get; set; }

            /// <summary>Reverts the item's value to default.</summary>
            /// <param name="silent">When true, this method won't invoke any events.</param>
            public void ResetToDefault(bool silent = false) =>
                ChangeValue(DefaultValue, silent);

            public override string ToString() =>
                $"{Name}: {Value} (default: {DefaultValue})";

            public static bool operator !=(ListItem a, ListItem b) =>
                !(a == b);

            public static bool operator ==(ListItem a, ListItem b) =>
                (ReferenceEquals(a, null) && ReferenceEquals(b, null)) ||
                (a.Name == b.Name && a.Value == b.Value && a.DefaultValue == b.DefaultValue);

            public override bool Equals(object obj)
            {
                if (!(obj is ListItem))
                    return false;

                return this == (ListItem)obj;
            }

            public override int GetHashCode() =>
                ToString().GetHashCode();
        }
    }
}