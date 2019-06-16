using LinqToLdap.Logging;
using System.DirectoryServices.Protocols;

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

#if !NET35 && !NET40

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
        System.Threading.Tasks.Task<object> ExecuteAsync(LdapConnection connection, SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null);

#endif
    }
}