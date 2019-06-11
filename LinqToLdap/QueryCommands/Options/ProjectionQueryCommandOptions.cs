using LinqToLdap.Mapping;
using LinqToLdap.Transformers;
using System;

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