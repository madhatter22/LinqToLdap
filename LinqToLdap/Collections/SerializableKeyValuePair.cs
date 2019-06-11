using System;
using System.Collections.Generic;

namespace LinqToLdap.Collections
{
    /// <summary>
    /// A serializable class implementation based on <see cref="KeyValuePair{TKey,TValue}"/>
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TValue">The type of the value</typeparam>
    [Serializable]
    public class SerializableKeyValuePair<TKey, TValue>
    {
        /// <summary>
        /// Default parameterless constructor.
        /// </summary>
        public SerializableKeyValuePair() { }

        /// <summary>
        /// Default constructor with initial values.
        /// </summary>
        public SerializableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Key property.
        /// </summary>
        public TKey Key { get; set; }

        /// <summary>
        /// Value property.
        /// </summary>
        public TValue Value { get; set; }
    }
}