/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using System.Collections.Generic;
using LinqToLdap.Transformers;

namespace LinqToLdap.QueryCommands.Options
{
    internal class ListAttributesQueryCommandOptions : QueryCommandOptions
    {
        public ListAttributesQueryCommandOptions(Dictionary<string, string> attributes) 
            : base(attributes ?? new Dictionary<string, string>())
        {
        }

        public override Type GetEnumeratorReturnType()
        {
            return typeof(KeyValuePair<string, IEnumerable<KeyValuePair<string, object>>>);
        }

        public override IResultTransformer GetTransformer()
        {
            return new RawEntryResultTransformer();
        }
    }
}
