using System.Collections.Generic;

namespace qASIC
{
    public static class IDictionaryExtensions
    {
        public static void SetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            switch (dictionary.ContainsKey(key))
            {
                case true:
                    dictionary[key] = value;
                    break;
                case false:
                    dictionary.Add(key, value);
                    break;
            }
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary.ContainsKey(key))
                return dictionary[key];

            return defaultValue;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) =>
            GetOrDefault(dictionary, key, default);
    }
}