using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LinqToLdap.Collections
{
    /// <summary>
    /// A read only generic dictionary.  Taken from http://www.blackwasp.co.uk/ReadOnlyDictionary.aspx
    /// </summary>
    public sealed class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _dictionary;
        private ICollection<TKey> _keys;
        private ICollection<TValue> _values;

        ///<summary>
        /// Creates an empty read only dictionary
        ///</summary>
        public ReadOnlyDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Creates a read only dictionary from an existing enumerable <see cref="KeyValuePair{TKey,TValue}"/>.
        /// </summary>
        /// <param name="source"></param>
        public ReadOnlyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> source) : this(source, null)
        {
        }

        /// <summary>
        /// Creates a read only dictionary from an existing enumerable <see cref="KeyValuePair{TKey,TValue}"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="comparer"></param>
        public ReadOnlyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            _dictionary = comparer == null
                ? source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : source.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, comparer);
        }

        /// <summary>
        /// Creates a read only dictionary from an existing enumerable <see cref="KeyValuePair{TKey,TValue}"/>.
        /// </summary>
        /// <param name="source"></param>
        public ReadOnlyDictionary(Dictionary<TKey, TValue> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            _dictionary = new Dictionary<TKey, TValue>(source, source.Comparer);
        }

        #region IDictionary<TKey,TValue> Members

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Determines whether the <see cref="ReadOnlyDictionary{TKey,TValue}"/> contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="ReadOnlyDictionary{TKey,TValue}"/>.</param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Gets a collection containing the keys in the <see cref="ReadOnlyDictionary{TKey,TValue}"/>.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get { return _keys ?? (_keys = new ReadOnlyCollection<TKey>(new List<TKey>(_dictionary.Keys))); }
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified key, 
        /// if the key is found; otherwise, the default value for the type of the value parameter. 
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets a collection containing the values in the <see cref="ReadOnlyDictionary{TKey,TValue}"/>.
        /// </summary>
        public ICollection<TValue> Values
        {
            get { return _values ?? (_values = new ReadOnlyCollection<TValue>(new List<TValue>(_dictionary.Values))); }
        }

        ///<summary>
        /// Gets or sets the value associated with the specified key.
        ///</summary>
        ///<param name="key">The key of the value to get or set.</param>
        public TValue this[TKey key]
        {
            get
            {
                return _dictionary[key];
            }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return this[key];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Dictionary is read only
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Thrown because the dictionary is read only
        /// </exception>
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Determines whether the <see cref="IDictionary{TKey,TValue}"/> contains an element with the specified key.
        /// </summary>
        /// <param name="item">The key to locate in the <see cref="IDictionary{TKey,TValue}"/>.</param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="IDictionary{TKey,TValue}"/> to an array, 
        /// starting at the specified array index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional array that is the destination of the elements 
        /// copied from <see cref="IDictionary{TKey,TValue}"/>. The array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements in the dictionary.
        /// </summary>
        public int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }

        /// <summary>
        /// Return true
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection. 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
