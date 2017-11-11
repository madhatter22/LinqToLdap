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
using System.Linq;
using LinqToLdap.Collections;
using LinqToLdap.Transformers;

namespace LinqToLdap.QueryCommands.Options
{
    internal class DynamicQueryCommandOptions : QueryCommandOptions
    {
        private readonly SelectProjection _projection;
        public DynamicQueryCommandOptions(IEnumerable<string> attrubutes)
            : base(attrubutes == null ? new Dictionary<string, string>() : attrubutes.ToDictionary(a => a))
        {
        }

        public DynamicQueryCommandOptions(SelectProjection projection)
            : base(projection == null ? new Dictionary<string, string>() : projection.SelectedProperties)
        {
            _projection = projection;
        }

        public DynamicQueryCommandOptions()
            : base(new Dictionary<string, string>())
        {
        }

        public override Type GetEnumeratorReturnType()
        {
            return _projection == null ? typeof(IDirectoryAttributes) : _projection.ReturnType;
        }

        public override IResultTransformer GetTransformer()
        {
            return new DynamicResultTransformer(_projection);
        }
    }
}
