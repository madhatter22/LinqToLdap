/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */
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
using LinqToLdap.Mapping;
using LinqToLdap.Transformers;

namespace LinqToLdap.QueryCommands.Options
{
    
    internal class StandardQueryCommandOptions : QueryCommandOptions
    {
        private readonly IObjectMapping _mapping;

        public StandardQueryCommandOptions(IObjectMapping mapping, IDictionary<string, string> queriedAttributes) 
            : base(queriedAttributes)
        {
            _mapping = mapping;
        }

        public override Type GetEnumeratorReturnType()
        {
            return _mapping.Type;
        }

        public override IResultTransformer GetTransformer()
        {
            return new ResultTransformer(AttributesToLoad, _mapping);
        }
    }
}
