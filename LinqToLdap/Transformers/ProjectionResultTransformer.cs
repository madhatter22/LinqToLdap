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

namespace LinqToLdap.Transformers
{
    internal class ProjectionResultTransformer : ResultTransformer
    {
        private readonly SelectProjection _selectProjection;

        public ProjectionResultTransformer(SelectProjection selectProjection, IDictionary<string, string> queriedProperties, IObjectMapping mapping) 
            : base(queriedProperties, mapping, queriedProperties.Count == mapping.Properties.Count && selectProjection.ReturnType == mapping.Type)
        {
            _selectProjection = selectProjection;
        }

        public override object Transform(System.DirectoryServices.Protocols.SearchResultEntry entry)
        {
            var transformed = _selectProjection.Projection.DynamicInvoke(base.Transform(entry));

            return transformed;
        }

        public override object Default()
        {
            var type = _selectProjection.ReturnType;
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
