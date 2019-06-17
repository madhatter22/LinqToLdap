using LinqToLdap.Collections;
using LinqToLdap.Helpers;
using System;
using System.DirectoryServices.Protocols;

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