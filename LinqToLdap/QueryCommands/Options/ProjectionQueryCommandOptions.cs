/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using LinqToLdap.Mapping;
using LinqToLdap.Transformers;

namespace LinqToLdap.QueryCommands.Options
{
    internal class ProjectionQueryCommandOptions : QueryCommandOptions
    {
        private readonly IObjectMapping _mapping;

        public ProjectionQueryCommandOptions(IObjectMapping mapping, SelectProjection selectProjection) : base(selectProjection)
        {
            _mapping = mapping;
        }

        public override Type GetEnumeratorReturnType()
        {
            return SelectProjection.ReturnType;
        }

        public override IResultTransformer GetTransformer()
        {
            return new ProjectionResultTransformer(SelectProjection, AttributesToLoad, _mapping);
        }
    }
}
