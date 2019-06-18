using LinqToLdap.Collections;
using LinqToLdap.Helpers;
using LinqToLdap.Transformers;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.QueryCommands.Options
{
    internal abstract class QueryCommandOptions : IQueryCommandOptions
    {
        protected QueryCommandOptions(SelectProjection selectProjection)
        {
            SelectProjection = selectProjection;
            AttributesToLoad = selectProjection.SelectedProperties;
        }

        protected QueryCommandOptions(IDictionary<string, string> queriedAttributes)
        {
            AttributesToLoad = queriedAttributes;
        }

        public IDictionary<string, string> AttributesToLoad { get; private set; }

        public string Filter { get; set; }

        public bool IsLongCount { get; set; }

        public bool WithoutPaging { get; set; }

        public IPagingOptions PagingOptions { get; set; }

        protected SelectProjection SelectProjection { get; private set; }

        public ISortingOptions SortingOptions { get; set; }

        public IEnumerable<DirectoryControl> Controls { get; set; }

        public int? PageSize { get; set; }

        public int? TakeSize { get; set; }

        public int? SkipSize { get; set; }

        public bool YieldNoResults { get; set; }

        public PartialResultProcessing AsyncProcessing { get; set; }

        public object GetEnumerator(SearchResultEntryCollection results)
        {
            return ObjectActivator.CreateGenericInstance(typeof(SearchResponseEnumerable<>), GetEnumeratorReturnType(),
                                            new object[] { results, GetTransformer() }, "1");
        }

        public object GetEnumerator()
        {
            return ObjectActivator.CreateGenericInstance(typeof(SearchResponseEnumerable<>), GetEnumeratorReturnType(),
                                            new object[] { null, GetTransformer() }, "1");
        }

        public object GetEnumerator(List<SearchResultEntry> results)
        {
            return ObjectActivator.CreateGenericInstance(typeof(SearchResponseEnumerable<>), GetEnumeratorReturnType(),
                                            new object[] { GetTransformer(), results }, "2");
        }

        public abstract Type GetEnumeratorReturnType();

        public abstract IResultTransformer GetTransformer();
    }
}