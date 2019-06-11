using LinqToLdap.EventListeners;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace LinqToLdap
{
    /// <summary>
    /// Interface for accessing configuration information for connecting, querying, and updating a LDAP server.
    /// </summary>
    public interface ILdapConfiguration
    {
        /// <summary>
        /// The configured connection factory to be used for all
        /// <see cref="DirectoryContext"/>s that don't explicitly get an <see cref="LdapConnection"/>.
        /// </summary>
        ILdapConnectionFactory ConnectionFactory { get; }

        /// <summary>
        /// Used for writing <see cref="DirectoryRequest"/> and <see cref="DirectoryResponse"/> information to a log.
        /// </summary>
        ILinqToLdapLogger Log { get; }

        /// <summary>
        /// Get the server max page size.
        /// </summary>
        int ServerMaxPageSize { get; }

        /// <summary>
        /// Indicates if paging is enabled.
        /// </summary>
        bool PagingEnabled { get; }

        /// <summary>
        /// Creates a <see cref="DirectoryContext"/> from the configuration.
        /// </summary>
        ///<returns></returns>
        IDirectoryContext CreateContext();

        /// <summary>
        /// Class responsible for mapping objects to directory entries.
        /// </summary>
        IDirectoryMapper Mapper { get; }

        /// <summary>
        /// Get all event listeners of type <typeparamref name="TListener"/> registered with this configuration.
        /// </summary>
        /// <typeparam name="TListener">The type of listeners to retrieve.</typeparam>
        /// <returns></returns>
        IEnumerable<TListener> GetListeners<TListener>() where TListener : IEventListener;

        /// <summary>
        /// Get all event listeners registered with this configuration.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IEventListener> GetListeners();
    }
}