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
using LinqToLdap.Transformers;

namespace LinqToLdap.QueryCommands.Options
{
    /// <summary>
    /// Options created to aid in query executions
    /// </summary>
    public interface IQueryCommandOptions
    {
        /// <summary>
        /// Attributes to load for the query
        /// </summary>
        IDictionary<string, string> AttributesToLoad { get; }

        /// <summary>
        /// Query filter
        /// </summary>
        string Filter { get; }

        /// <summary>
        /// Indicates the page size to use if no paging options have been specified.
        /// </summary>
        int? PageSize { get; }

        /// <summary>
        /// Indicates the max number of records to retrieve
        /// </summary>
        int? TakeSize { get; }

        /// <summary>
        /// Indicates the number of records to skip.
        /// </summary>
        int? SkipSize { get; }

        /// <summary>
        /// Indicates if the count operation is a long count.
        /// </summary>
        bool IsLongCount { get; }

        /// <summary>
        /// Indicates if the current query should execute without paging.
        /// </summary>
        bool WithoutPaging { get; }

        /// <summary>
        /// Gets the paging information for a query.  Null if there is no paging information.
        /// </summary>
        IPagingOptions PagingOptions { get; }

        /// <summary>
        /// Gets the sorting information for a query.  Null if there is no sorting information.
        /// </summary>
        ISortingOptions SortingOptions { get; }

        ///<summary>
        /// Gets controls to be used with the query.
        ///</summary>
        IEnumerable<DirectoryControl> Controls { get; }

        /// <summary>
        /// Gets an enumerator that can handle the <paramref name="results"/>
        /// </summary>
        /// <param name="results">Results to iterate.</param>
        /// <returns></returns>
        object GetEnumerator(SearchResultEntryCollection results);

        /// <summary>
        /// Gets an enumerator that can handle the <paramref name="results"/>
        /// </summary>
        /// <param name="results">Results to iterate.</param>
        /// <returns></returns>
        object GetEnumerator(List<SearchResultEntry> results);

        /// <summary>
        /// Gets a default enumerator with no results to enumerate.
        /// </summary>
        /// <returns></returns>
        object GetEnumerator();

        /// <summary>
        /// Gets a <see cref="IResultTransformer"/> that can transform a <see cref="SearchResultEntry"/>
        /// </summary>
        /// <returns></returns>
        IResultTransformer GetTransformer();

        /// <summary>
        /// Gets the return type from GetEnumerator.
        /// </summary>
        Type GetEnumeratorReturnType();

        /// <summary>
        /// Indicates if a locally evaluated condition results in no filter being created.
        /// </summary>
        bool YieldNoResults { get; }
    }
}
