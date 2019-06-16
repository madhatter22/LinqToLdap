#if !NET35

using LinqToLdap.Collections;
using LinqToLdap.Exceptions;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Threading.Tasks;

namespace LinqToLdap.Async
{
    /// <summary>
    /// Async extension methods for <see cref="IDirectoryContext"/>.
    /// </summary>
    public static class DirectoryContextAsyncExtensions
    {
        /// <summary>
        /// Executes <see cref="DirectoryContext.GetByDN{T}"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The <see cref="DirectoryContext"/>.</param>
        /// <param name="distinguishedName">The distinguished name to look for.</param>
        /// <typeparam name="T">The type of mapped object</typeparam>
        /// <returns></returns>
        public static Task<T> GetByDNAsync<T>(this IDirectoryContext context, string distinguishedName) where T : class
        {
            return Task.Factory.StartNew(() => context.GetByDN<T>(distinguishedName));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.GetByDN"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The <see cref="DirectoryContext"/>.</param>
        /// <param name="distinguishedName">The distinguished name to look for.</param>
        /// <param name="attributes">The attributes to load.</param>
        /// <returns></returns>
        public static Task<IDirectoryAttributes> GetByDNAsync(this IDirectoryContext context, string distinguishedName, params string[] attributes)
        {
            return Task.Factory.StartNew(() => context.GetByDN(distinguishedName, attributes));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.Add{T}(T,string,DirectoryControl[])"/>
        /// </summary>
        /// <typeparam name="T">The type of entry.</typeparam>
        /// <param name="context">The <see cref="DirectoryContext"/>.</param>
        /// <param name="entry">The object to save.</param>
        /// <param name="distinguishedName">The distinguished name for the entry.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        public static Task AddAsync<T>(this IDirectoryContext context, T entry, string distinguishedName = null,
                                DirectoryControl[] controls = null) where T : class
        {
            return Task.Factory.StartNew(() => context.Add(entry, distinguishedName, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.Add(IDirectoryAttributes,DirectoryControl[])"/>
        /// </summary>
        /// <param name="context">The <see cref="DirectoryContext"/>.</param>
        /// <param name="entry">The attributes for the entry</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        public static Task AddAsync(this IDirectoryContext context, IDirectoryAttributes entry,
                                    DirectoryControl[] controls)
        {
            return Task.Factory.StartNew(() => context.Add(entry, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.Add(IDirectoryAttributes)"/>
        /// </summary>
        /// <param name="context">The <see cref="DirectoryContext"/>.</param>
        /// <param name="entry">The attributes for the entry</param>
        public static Task AddAsync(this IDirectoryContext context, IDirectoryAttributes entry)
        {
            return Task.Factory.StartNew(() => context.Add(entry));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.Add(DirectoryAttributes,DirectoryControl[])"/>
        /// </summary>
        /// <param name="context">The <see cref="DirectoryContext"/>.</param>
        /// <param name="entry">The attributes for the entry</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        public static Task AddAsync(this IDirectoryContext context, DirectoryAttributes entry,
                                    DirectoryControl[] controls)
        {
            return Task.Factory.StartNew(() => context.Add(entry, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.Add(DirectoryAttributes)"/>
        /// </summary>
        /// <param name="context">The <see cref="DirectoryContext"/>.</param>
        /// <param name="entry">The attributes for the entry</param>
        public static Task AddAsync(this IDirectoryContext context, DirectoryAttributes entry)
        {
            return Task.Factory.StartNew(() => context.Add(entry));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.AddAndGet{T}(T,string,DirectoryControl[])"/>
        /// </summary>
        /// <typeparam name="T">The type of entry.</typeparam>
        /// <param name="context">The <see cref="DirectoryContext"/>.</param>
        /// <param name="entry">The object to save.</param>
        /// <param name="distinguishedName">The distinguished name for the entry.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        public static Task<T> AddAndGetAsync<T>(this IDirectoryContext context, T entry, string distinguishedName = null,
                                DirectoryControl[] controls = null) where T : class
        {
            return Task.Factory.StartNew(() => context.AddAndGet(entry, distinguishedName, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.AddAndGet(IDirectoryAttributes,DirectoryControl[])"/>
        /// </summary>
        /// <param name="context">The <see cref="DirectoryContext"/>.</param>
        /// <param name="entry">The attributes for the entry</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        public static Task<IDirectoryAttributes> AddAndGetAsync(this IDirectoryContext context, IDirectoryAttributes entry,
                                    DirectoryControl[] controls)
        {
            return Task.Factory.StartNew(() => context.AddAndGet(entry, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.AddAndGet(IDirectoryAttributes)"/>
        /// </summary>
        /// <param name="context">The <see cref="DirectoryContext"/>.</param>
        /// <param name="entry">The attributes for the entry</param>
        public static Task<IDirectoryAttributes> AddAndGetAsync(this IDirectoryContext context, IDirectoryAttributes entry)
        {
            return Task.Factory.StartNew(() => context.AddAndGet(entry));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.AddAndGet(DirectoryAttributes,DirectoryControl[])"/>
        /// </summary>
        /// <param name="context">The <see cref="DirectoryContext"/>.</param>
        /// <param name="entry">The attributes for the entry</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        public static Task<IDirectoryAttributes> AddAndGetAsync(this IDirectoryContext context, DirectoryAttributes entry,
                                    DirectoryControl[] controls)
        {
            return Task.Factory.StartNew(() => context.AddAndGet(entry, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.AddAndGet(DirectoryAttributes)"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The <see cref="DirectoryContext"/>.</param>
        /// <param name="entry">The attributes for the entry</param>
        public static Task<IDirectoryAttributes> AddAndGetAsync(this IDirectoryContext context, DirectoryAttributes entry)
        {
            return Task.Factory.StartNew(() => context.AddAndGet(entry));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.Delete"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="distinguishedName">The distinguished name of the entry</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="distinguishedName"/> is null, empty or white space.</exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        public static Task Delete(this IDirectoryContext context, string distinguishedName, params DirectoryControl[] controls)
        {
            return Task.Factory.StartNew(() => context.Delete(distinguishedName, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.UpdateAndGet{T}"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entry">The entry to update</param>
        /// <param name="distinguishedName">The distinguished name for the entry.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <typeparam name="T">The type of entry.</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if entry is null</exception>
        /// <exception cref="MappingException">
        /// Thrown if <paramref name="distinguishedName"/> is null and Distinguished Name is not mapped.
        /// Thrown if <typeparamref name="T"/> has not been mapped.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if distinguished name is null and there is no mapped distinguished name property.</exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation is not successful</exception>
        /// <exception cref="LdapException">Thrown if the operation is not successful</exception>
        public static Task<T> UpdateAndGetAsync<T>(this IDirectoryContext context, T entry,
                                                   string distinguishedName = null, DirectoryControl[] controls = null)
            where T : class
        {
            return Task.Factory.StartNew(() => context.UpdateAndGet(entry, distinguishedName, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.UpdateAndGet(IDirectoryAttributes)"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entry">The attributes for the entry.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        public static Task<IDirectoryAttributes> UpdateAndGetAsync(this IDirectoryContext context, IDirectoryAttributes entry)
        {
            return Task.Factory.StartNew(() => context.UpdateAndGet(entry));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.UpdateAndGet(DirectoryAttributes)"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entry">The attributes for the entry.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        public static Task<IDirectoryAttributes> UpdateAndGetAsync(this IDirectoryContext context, DirectoryAttributes entry)
        {
            return Task.Factory.StartNew(() => context.UpdateAndGet(entry));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.UpdateAndGet(IDirectoryAttributes,DirectoryControl[])"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entry">The attributes for the entry.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        public static Task<IDirectoryAttributes> UpdateAndGetAsync(this IDirectoryContext context, IDirectoryAttributes entry,
                                                              DirectoryControl[] controls)
        {
            return Task.Factory.StartNew(() => context.UpdateAndGet(entry, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.Update{T}"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entry">The entry to update</param>
        /// <param name="distinguishedName">The distinguished name for the entry.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <typeparam name="T">The type of entry.</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if entry is null</exception>
        /// <exception cref="MappingException">
        /// Thrown if <paramref name="distinguishedName"/> is null and Distinguished Name is not mapped.
        /// Thrown if <typeparamref name="T"/> has not been mapped.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if distinguished name is null and there is no mapped distinguished name property.</exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation is not successful</exception>
        /// <exception cref="LdapException">Thrown if the operation is not successful</exception>
        public static Task UpdateAsync<T>(this IDirectoryContext context, T entry,
                                                   string distinguishedName = null, DirectoryControl[] controls = null)
            where T : class
        {
            return Task.Factory.StartNew(() => context.Update(entry, distinguishedName, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.Update(IDirectoryAttributes)"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entry">The attributes for the entry.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        public static Task UpdateAsync(this IDirectoryContext context, IDirectoryAttributes entry)
        {
            return Task.Factory.StartNew(() => context.Update(entry));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.Update(DirectoryAttributes)"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entry">The attributes for the entry.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        public static Task UpdateAsync(this IDirectoryContext context, DirectoryAttributes entry)
        {
            return Task.Factory.StartNew(() => context.Update(entry));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.Update(IDirectoryAttributes,DirectoryControl[])"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entry">The attributes for the entry.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        public static Task UpdateAsync(this IDirectoryContext context, IDirectoryAttributes entry,
                                                              DirectoryControl[] controls)
        {
            return Task.Factory.StartNew(() => context.Update(entry, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.Delete"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="distinguishedName">The distinguished name of the entry</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="distinguishedName"/> is null, empty or white space.</exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        public static Task DeleteAsync(this IDirectoryContext context, string distinguishedName, params DirectoryControl[] controls)
        {
            return Task.Factory.StartNew(() => context.Delete(distinguishedName, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.ListServerAttributes"/> within a <see cref="Task"/>.
        /// </summary>
        /// <returns></returns>
        public static Task<IDirectoryAttributes> ListServerAttributesAsync(this IDirectoryContext context, params string[] attributes)
        {
            return Task.Factory.StartNew(() => context.ListServerAttributes(attributes));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.AddAttribute"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="distinguishedName">The entry</param>
        /// <param name="attributeName">The name of the attribute</param>
        /// <param name="value">The value for the entry.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapConnection">Thrown if the operation fails.</exception>
        public static Task AddAttributeAsync(this IDirectoryContext context, string distinguishedName, string attributeName, object value = null,
                                 DirectoryControl[] controls = null)
        {
            return Task.Factory.StartNew(() => context.AddAttribute(distinguishedName, attributeName, value, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.DeleteAttribute"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="distinguishedName">The entry</param>
        /// <param name="attributeName">The name of the attribute</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="distinguishedName"/> or <paramref name="attributeName"/> is null, empty or white space.</exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapConnection">Thrown if the operation fails.</exception>
        public static Task DeleteAttributeAsync(this IDirectoryContext context, string distinguishedName, string attributeName,
                                                params DirectoryControl[] controls)
        {
            return Task.Factory.StartNew(() => context.DeleteAttribute(distinguishedName, attributeName, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.MoveEntry"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="currentDistinguishedName">The entry's current distinguished name</param>
        /// <param name="newNamingContext">The new container for the entry</param>
        /// <param name="deleteOldRDN">Maps to <see cref="P:System.DirectoryServices.Protocols.ModifyDNRequest.DeleteOldRdn"/>. Defaults to null to use default behavior from <see cref="P:System.DirectoryServices.Protocols.ModifyDNRequest.DeleteOldRdn"/>.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="currentDistinguishedName"/> has an invalid format.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="currentDistinguishedName"/>
        /// or <paramref name="newNamingContext"/> are null, empty or white space.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapConnection">Thrown if the operation fails.</exception>
        public static Task<string> MoveEntryAsync(this IDirectoryContext context, string currentDistinguishedName, string newNamingContext,
                                             bool? deleteOldRDN = null, params DirectoryControl[] controls)
        {
            return Task.Factory.StartNew(() => context.MoveEntry(currentDistinguishedName, newNamingContext, deleteOldRDN, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.RenameEntry"/> within a <see cref="Task"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="currentDistinguishedName">The entry's current distinguished name</param>
        /// <param name="newName">The new name of the entry</param>
        /// <param name="deleteOldRDN">Maps to <see cref="P:System.DirectoryServices.Protocols.ModifyDNRequest.DeleteOldRdn"/>. Defaults to null to use default behavior from <see cref="P:System.DirectoryServices.Protocols.ModifyDNRequest.DeleteOldRdn"/>.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="currentDistinguishedName"/> has an invalid format.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="currentDistinguishedName"/>
        /// or <paramref name="newName"/> are null, empty or white space.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapConnection">Thrown if the operation fails.</exception>
        public static Task<string> RenameEntryAsync(this IDirectoryContext context, string currentDistinguishedName, string newName,
                                               bool? deleteOldRDN = null, params DirectoryControl[] controls)
        {
            return Task.Factory.StartNew(() => context.RenameEntry(currentDistinguishedName, newName, deleteOldRDN, controls));
        }

        /// <summary>
        /// Executes <see cref="DirectoryContext.RetrieveRanges{TValue}"/> within a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the attribute.  Must be <see cref="string"/> or <see cref="Array"/> of <see cref="byte"/>.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="distinguishedName">The distinguished name of the entry.</param>
        /// <param name="attributeName">The attribute to load.</param>
        /// <param name="start">The starting point for the range. Defaults to 0.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="distinguishedName"/> or <paramref name="attributeName"/> is null, empty or white space.
        /// </exception>
        /// <returns></returns>
        public static Task<IList<TValue>> RetrieveRangesAsync<TValue>(this IDirectoryContext context, string distinguishedName,
                                                           string attributeName, int start = 0)
        {
            return Task.Factory.StartNew(() => context.RetrieveRanges<TValue>(distinguishedName, attributeName, start));
        }
    }
}

#endif