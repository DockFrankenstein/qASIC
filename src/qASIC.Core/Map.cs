using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace qASIC
{
    /// <summary>Represents a collection of two lists that point their elements to each other (two-way <see cref="Dictionary{TKey, TValue}"/>).</summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    public class Map<T0, T1> : IEnumerable<KeyValuePair<T0, T1>>
    {
        public Map() : this(new Dictionary<T0, T1>())
        {

        }

        /// <summary>Initializes a new instance by copying elements from a dictionary. <b>Make sure the dictionary doesn't contain multiple elements with the same value!</b></summary>
        /// <param name="dictionary"></param>
        public Map(Dictionary<T0, T1> dictionary)
        {
            _forwardDictionary = dictionary;
            _backwardDictionary = dictionary.ToDictionary(x => x.Value, x => x.Key);
            Forward = new Indexer<T0, T1>(_forwardDictionary);
            Backward = new Indexer<T1, T0>(_backwardDictionary);
        }

        public Map(IEnumerable<KeyValuePair<T0, T1>> pairs) : this(pairs.ToDictionary(x => x.Key, x => x.Value)) { }

        private Dictionary<T0, T1> _forwardDictionary = new Dictionary<T0, T1>();
        private Dictionary<T1, T0> _backwardDictionary = new Dictionary<T1, T0>();

        public Indexer<T0, T1> Forward { get; private set; }
        public Indexer<T1, T0> Backward { get; private set; }

        /// <summary>Gets the number of elements in the map.</summary>
        public int Count =>
            _forwardDictionary.Count;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<T0, T1>> GetEnumerator()
        {
            return _forwardDictionary.GetEnumerator();
        }

        /// <summary>Sets a relation between two elements.</summary>
        /// <param name="forward">The first elements.</param>
        /// <param name="backward">The second element.</param>
        public void Set(T0 forward, T1 backward)
        {
            if (_forwardDictionary.TryGetValue(forward, out var forwardValue))
            {
                _backwardDictionary.Remove(forwardValue);
                _forwardDictionary.Remove(forward);
            }

            if (_backwardDictionary.TryGetValue(backward, out var backwardValue))
            {
                _forwardDictionary.Remove(backwardValue);
                _backwardDictionary.Remove(backward);
            }

            _forwardDictionary.Add(forward, backward);
            _backwardDictionary.Add(backward, forward);
        }

        /// <summary>Removes two elements related to each other using the one from the forward indexer.</summary>
        /// <param name="key">Elements to remove.</param>
        /// <returns>If the elements have been removed successfully.</returns>
        public bool RemoveForward(T0 key)
        {
            if (_forwardDictionary.ContainsKey(key))
                return false;

            T1 backwardKey = _forwardDictionary[key];
            return _forwardDictionary.Remove(key) &&
                _backwardDictionary.Remove(backwardKey);
        }

        /// <summary>Removes two elements related to each other using the one from the backward indexer.</summary>
        /// <param name="key">Element to remove.</param>
        /// <returns>If the elements have been removed successfully.</returns>
        public bool RemoveBackward(T1 key)
        {
            if (_backwardDictionary.ContainsKey(key))
                return false;

            T0 forwardKey = _backwardDictionary[key];
            return _forwardDictionary.Remove(forwardKey) &&
                _backwardDictionary.Remove(key);
        }

        /// <summary>Removes all elements from the map.</summary>
        public void Clear()
        {
            _forwardDictionary.Clear();
            _backwardDictionary.Clear();
        }

        /// <summary>Determines whether the map contains the specified pair of related elements.</summary>
        /// <param name="item">Pair of related elements.</param>
        /// <returns>If the map contains the pair.</returns>
        public bool Contains(KeyValuePair<T0, T1> item) =>
            _forwardDictionary.Contains(item);

        /// <summary>Contains elements mapped to one of the sides (works simular to a dictionary).</summary>
        /// <typeparam name="T">The key.</typeparam>
        /// <typeparam name="t">The value.</typeparam>
        public class Indexer<T, t> : IEnumerable<KeyValuePair<T, t>>
        {
            private readonly Dictionary<T, t> _dictionary;

            internal Indexer(Dictionary<T, t> dictionary)
            {
                _dictionary = dictionary;
            }

            public t this[T index]
            {
                get { return _dictionary[index]; }
            }

            /// <summary>Determines whenever the <see cref="Indexer{T, t}"/> contains an element with the specified key.</summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public bool ContainsKey(T key)
            {
                return _dictionary.ContainsKey(key);
            }

            /// <summary>Gets the value associated with the specified key.</summary>
            /// <param name="key">The key of the value to get.</param>
            /// <param name="value">The value.</param>
            /// <returns>If the of the specified key exists.</returns>
            public bool TryGetValue(T key, out t value) =>
                _dictionary.TryGetValue(key, out value);

            public IEnumerator<KeyValuePair<T, t>> GetEnumerator() =>
                _dictionary.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                GetEnumerator();
        }
    }
}
