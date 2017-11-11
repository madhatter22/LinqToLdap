/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System.DirectoryServices.Protocols;
using LinqToLdap.Logging;

namespace LinqToLdap.QueryCommands
{
    /// <summary>
    /// Interface for a command that executes a query against the directory.
    /// </summary>
    public interface IQueryCommand
    {
        /// <summary>
        /// Executes a query against the <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Connection to use.</param>
        /// <param name="scope">Search depth.</param>
        /// <param name="pagingEnabled">Indicates if paging is enabled.  Defaults to true</param>
        /// <param name="log">Log for writing <see cref="SearchRequest"/> information.</param>
        /// <param name="maxPageSize">The max server page size.  Defaults to 500</param>
        /// <param name="namingContext">DistinguishedName for the <see cref="SearchRequest"/></param>
        /// <returns></returns>
        object Execute(DirectoryConnection connection, SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null);
    }
}
