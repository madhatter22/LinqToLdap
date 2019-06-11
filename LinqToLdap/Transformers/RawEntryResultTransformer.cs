using LinqToLdap.Collections;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.Transformers
{
    internal class RawEntryResultTransformer : IResultTransformer
    {
        public object Transform(SearchResultEntry entry)
        {
            return new KeyValuePair<string, IEnumerable<KeyValuePair<string, object>>>(
                entry.DistinguishedName,
                new DirectoryAttributes(entry));
        }

        public object Default()
        {
            return default(KeyValuePair<string, IEnumerable<KeyValuePair<string, object>>>);
        }
    }
}