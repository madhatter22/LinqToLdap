using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LinqToLdap.Collections
{
    /// <summary>
    /// Represents a collection
    /// </summary>
    [Serializable]
    public class OriginalValuesCollection : Collection<SerializableKeyValuePair<string, object>>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public OriginalValuesCollection() : base(new List<SerializableKeyValuePair<string, object>>()) { }

        /// <summary>
        /// Constructor that takes list of values.
        /// </summary>
        /// <param name="values"></param>
        public OriginalValuesCollection(IList<SerializableKeyValuePair<string, object>> values) : base(values) { }

        /// <summary>
        /// Case insensitive key accessor for the value.
        /// </summary>
        /// <param name="key">The property name</param>
        /// <returns></returns>
        public object this[String key]
        {
            get
            {
                return this.Where(kvp => kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                    .Select(kvp => kvp.Value)
                    .FirstOrDefault();
            }
        }
    }
}