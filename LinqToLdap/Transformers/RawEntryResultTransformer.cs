/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using LinqToLdap.Collections;
using System.Linq;

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
