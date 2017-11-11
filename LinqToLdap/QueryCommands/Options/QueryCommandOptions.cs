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
using System.DirectoryServices.Protocols;
using LinqToLdap.Collections;
using LinqToLdap.Transformers;

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

        public object GetEnumerator(SearchResultEntryCollection results)
        {
            return Activator.CreateInstance(typeof(SearchResponseEnumerable<>).MakeGenericType(GetEnumeratorReturnType()),
                                            new object[] { results, GetTransformer() });
        }

        public object GetEnumerator()
        {
            return Activator.CreateInstance(typeof(SearchResponseEnumerable<>).MakeGenericType(GetEnumeratorReturnType()),
                                            new object[] { null, GetTransformer() });
        }

        public object GetEnumerator(List<SearchResultEntry> results)
        {
            return Activator.CreateInstance(typeof(SearchResponseEnumerable<>).MakeGenericType(GetEnumeratorReturnType()),
                                            new object[] { GetTransformer(), results });
        }

        public abstract Type GetEnumeratorReturnType();
        public abstract IResultTransformer GetTransformer();
    }
}
