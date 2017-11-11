/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System.DirectoryServices.Protocols;

namespace LinqToLdap.QueryCommands.Options
{
    /// <summary>
    /// Interface the contains <see cref="SortKey"/>s for a query.
    /// </summary>
    public interface ISortingOptions
    {
        /// <summary>
        /// The keys containing sort information.
        /// </summary>
        SortKey[] Keys { get; }
    }
}