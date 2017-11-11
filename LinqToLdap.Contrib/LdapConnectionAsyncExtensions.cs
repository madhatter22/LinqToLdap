using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Threading.Tasks;
using LinqToLdap.Collections;
using LinqToLdap.EventListeners;
using LinqToLdap.Logging;

namespace LinqToLdap
{
    /// <summary>
    /// Async extension methods for <see cref="LdapConnection"/>.
    /// </summary>
    public static class LdapConnectionAsyncExtensions
    {
        /// <summary>
        /// Executes <see cref="LdapConnectionExtensions.Add(LdapConnection,IDirectoryAttributes,ILinqToLdapLogger,DirectoryControl[],IEnumerable{IAddEventListener})"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="entry">The entry to add.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <param name="listeners">The event listeners to be notified.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connection"/> or <paramref name="entry"/>> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the add was not successful.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        public static Task AddAsync(this LdapConnection connection, IDirectoryAttributes entry, ILinqToLdapLogger log = null,
                               DirectoryControl[] controls = null, IEnumerable<IPreAddEventListener> listeners = null)
        {
            return Task.Factory.StartNew(() => connection.Add(entry, log, controls, listeners));
        }

        /// <summary>
        /// Executes <see cref="LdapConnectionExtensions.AddAndGet"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="entry">The entry to add.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <param name="listeners">The event listeners to be notified.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connection"/> or <paramref name="entry"/>> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the add was not successful.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        public static Task<IDirectoryAttributes> AddAndGetAsync(this LdapConnection connection, IDirectoryAttributes entry, ILinqToLdapLogger log = null, DirectoryControl[] controls = null, IEnumerable<IPreAddEventListener> listeners = null)
        {
            return Task.Factory.StartNew(() => connection.AddAndGet(entry, log, controls, listeners));
        }

        /// <summary>
        /// Executes <see cref="LdapConnectionExtensions.Delete"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="distinguishedName">The distinguished name of the entry
        /// </param><param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="listeners">The event listeners to be notified.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="distinguishedName"/> is null, empty or white space.</exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        public static Task DeleteAsync(this LdapConnection connection, string distinguishedName, ILinqToLdapLogger log = null, DirectoryControl[] controls = null, IEnumerable<IPreDeleteEventListener> listeners = null)
        {
            return Task.Factory.StartNew(() => connection.Delete(distinguishedName, log, controls, listeners));
        }

        /// <summary>
        /// Executes <see cref="LdapConnectionExtensions.Update"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="entry">The entry to update.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <param name="listeners">The event listeners to be notified.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        public static Task UpdateAsync(this LdapConnection connection, IDirectoryAttributes entry, ILinqToLdapLogger log = null, DirectoryControl[] controls = null, IEnumerable<IPreUpdateEventListener> listeners = null)
        {
            return Task.Factory.StartNew(() => connection.Update(entry, log, controls, listeners));
        }

        /// <summary>
        /// Executes <see cref="LdapConnectionExtensions.UpdateAndGet"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="entry">The entry to update.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <param name="listeners">The event listeners to be notified.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        public static Task<IDirectoryAttributes> UpdateAndGetAsync(this LdapConnection connection, IDirectoryAttributes entry, ILinqToLdapLogger log = null, DirectoryControl[] controls = null, IEnumerable<IPreUpdateEventListener> listeners = null)
        {
            return Task.Factory.StartNew(() => connection.UpdateAndGet(entry, log, controls, listeners));
        }

        /// <summary>
        /// Executes <see cref="LdapConnectionExtensions.ListServerAttributes"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="attributes">
        /// Specify specific attributes to load.  Some LDAP servers require an explicit request for certain attributes.
        /// </param>
        /// <returns></returns>
        public static Task<IDirectoryAttributes> ListServerAttributesAsync(this LdapConnection connection, string[] attributes = null, ILinqToLdapLogger log = null)
        {
            return Task.Factory.StartNew(() => connection.ListServerAttributes(attributes, log));
        }

        /// <summary>
        /// Executes <see cref="LdapConnectionExtensions.GetByDN"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="distinguishedName">The distinguished name to look for.</param>
        /// <param name="attributes">The attributes to load.</param>
        /// <returns></returns>
        public static Task<IDirectoryAttributes> GetByDNAsync(this LdapConnection connection, string distinguishedName, ILinqToLdapLogger log = null, params string[] attributes)
        {
            return Task.Factory.StartNew(() => connection.GetByDN(distinguishedName, log, attributes));
        }

        /// <summary>
        /// Executes <see cref="LdapConnectionExtensions.MoveEntry"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="currentDistinguishedName">The entry's current distinguished name</param>
        /// <param name="newNamingContext">The new container for the entry</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="currentDistinguishedName"/> has an invalid format.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="currentDistinguishedName"/>
        /// or <paramref name="newNamingContext"/> are null, empty or white space.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapConnection">Thrown if the operation fails.</exception>
        public static Task<string> MoveEntryAsync(this LdapConnection connection, string currentDistinguishedName, string newNamingContext, ILinqToLdapLogger log = null, params DirectoryControl[] controls)
        {
            return Task.Factory.StartNew(() => connection.MoveEntry(currentDistinguishedName, newNamingContext, log));
        }

        /// <summary>
        /// Executes <see cref="LdapConnectionExtensions.RenameEntry"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="currentDistinguishedName">The entry's current distinguished name</param>
        /// <param name="newName">The new name of the entry</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="currentDistinguishedName"/> has an invalid format.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="currentDistinguishedName"/>
        /// or <paramref name="newName"/> are null, empty or white space.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapConnection">Thrown if the operation fails.</exception>
        public static Task<string> RenameEntryAsync(this LdapConnection connection, string currentDistinguishedName, string newName, ILinqToLdapLogger log = null, params DirectoryControl[] controls)
        {
            return Task.Factory.StartNew(() => connection.RenameEntry(currentDistinguishedName, newName, log));
        }

        /// <summary>
        /// Executes <see cref="LdapConnectionExtensions.RetrieveRanges{T}"/> within a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the attribute.  Must be <see cref="string"/> or <see cref="Array"/> of <see cref="byte"/>.</typeparam>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="distinguishedName">The distinguished name of the entry.</param>
        /// <param name="attributeName">The attribute to load.</param>
        /// <param name="start">The starting point for the range. Defaults to 0.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="distinguishedName"/> or <paramref name="attributeName"/> is null, empty or white space.
        /// </exception>
        /// <returns></returns>
        public static Task<IList<TValue>> RetrieveRangesAsync<TValue>(this LdapConnection connection, string distinguishedName, string attributeName, int start = 0, ILinqToLdapLogger log = null)
        {
            return
                Task.Factory.StartNew(
                    () => connection.RetrieveRanges<TValue>(distinguishedName, attributeName, start, log));
        }
    }
}
