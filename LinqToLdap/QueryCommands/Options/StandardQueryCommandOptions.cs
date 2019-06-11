using LinqToLdap.Mapping;
using LinqToLdap.Transformers;
using System;
using System.Collections.Generic;

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