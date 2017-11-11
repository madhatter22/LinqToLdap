/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using System.DirectoryServices.Protocols;
using LinqToLdap.Collections;

namespace LinqToLdap.Transformers
{
    internal class DynamicResultTransformer : IResultTransformer
    {
        private readonly SelectProjection _projection;
        public DynamicResultTransformer(SelectProjection projection = null)
        {
            _projection = projection;
        }

        public object Transform(SearchResultEntry entry)
        {
            var attributes = new DirectoryAttributes(entry);

            return _projection == null ? attributes : _projection.Projection.DynamicInvoke(attributes);
        }

        public object Default()
        {
            if (_projection != null)
            {
                var type = _projection.ReturnType;
                return type.IsValueType ? Activator.CreateInstance(type) : null;
            }

            return null;
        }
    }
}
