#if (!NET35 && !NET40)

using LinqToLdap.Collections;
using LinqToLdap.EventListeners;
using LinqToLdap.Helpers;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using LinqToLdap.Transformers;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Threading.Tasks;

namespace LinqToLdap.Async
{
    /// <summary>
    /// Async extension methods for <see cref="LdapConnection"/>.
    /// </summary>
    public static class LdapConnectionAsyncExtensions
    {
        /// <summary>
        /// Adds the entry to the directory.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="entry">The entry to add.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <param name="listeners">The event listeners to be notified.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connection"/> or <paramref name="entry"/>> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the add was not successful.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        public static async Task AddAsync(this LdapConnection connection, IDirectoryAttributes entry, ILinqToLdapLogger log = null,
            DirectoryControl[] controls = null, IEnumerable<IAddEventListener> listeners = null, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            string distinguishedName = null;
            try
            {
                if (connection == null) throw new ArgumentNullException("connection");
                if (entry == null) throw new ArgumentNullException("entry");
                distinguishedName = entry.DistinguishedName;

                if (distinguishedName.IsNullOrEmpty()) throw new ArgumentException("entry.DistinguishedName is invalid.");

                var request = new AddRequest(distinguishedName, entry.GetChangedAttributes().Where(da => da.Count > 0).ToArray());
                if (controls != null)
                {
                    request.Controls.AddRange(controls);
                }

                if (listeners != null)
                {
                    var args = new ListenerPreArgs<object, AddRequest>(entry, request, connection);
                    foreach (var eventListener in listeners.OfType<IPreAddEventListener>())
                    {
                        eventListener.Notify(args);
                    }
                }

                if (log != null && log.TraceEnabled) log.Trace(request.ToLogString());

                AddResponse response = null;
#if NET45
                await Task.Factory.FromAsync(
                    (callback, state) =>
                    {
                        return connection.BeginSendRequest(request, resultProcessing, callback, state);
                    },
                    (asyncresult) =>
                    {
                        response = (AddResponse)connection.EndSendRequest(asyncresult);
                        response.AssertSuccess();
                    },
                    null
                );
#else
                response = await Task.Run(() => connection.SendRequest(request) as AddResponse);
                response.AssertSuccess();
#endif

                if (listeners != null)
                {
                    var args = new ListenerPostArgs<object, AddRequest, AddResponse>(entry, request, response, connection);
                    foreach (var eventListener in listeners.OfType<IPostAddEventListener>())
                    {
                        eventListener.Notify(args);
                    }
                }
            }
            catch (Exception ex)
            {
                if (log != null) log.Error(ex, string.Format("An error occurred while trying to add '{0}'.", distinguishedName));
                throw;
            }
        }

        /// <summary>
        /// Adds the entry to the directory and returns the newly saved entry from the directory.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="entry">The entry to add.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <param name="listeners">The event listeners to be notified.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connection"/> or <paramref name="entry"/>> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the add was not successful.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        public static async Task<IDirectoryAttributes> AddAndGetAsync(this LdapConnection connection, IDirectoryAttributes entry,
            ILinqToLdapLogger log = null, DirectoryControl[] controls = null, IEnumerable<IAddEventListener> listeners = null,
            PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            await AddAsync(connection, entry, log, controls, listeners, resultProcessing);

            return await GetByDNAsync(connection, entry.DistinguishedName, log, null, resultProcessing);
        }

        /// <summary>
        /// Deletes an entry from the directory.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="distinguishedName">The distinguished name of the entry
        /// </param><param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="listeners">The event listeners to be notified.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="distinguishedName"/> is null, empty or white space.</exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        public static async Task DeleteAsync(this LdapConnection connection, string distinguishedName, ILinqToLdapLogger log = null,
            DirectoryControl[] controls = null, IEnumerable<IDeleteEventListener> listeners = null, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            try
            {
                if (connection == null) throw new ArgumentNullException("connection");
                if (distinguishedName.IsNullOrEmpty())
                    throw new ArgumentNullException("distinguishedName");

                var request = new DeleteRequest(distinguishedName);
                if (controls != null)
                {
                    request.Controls.AddRange(controls);
                }

                if (listeners != null)
                {
                    var args = new ListenerPreArgs<string, DeleteRequest>(distinguishedName, request, connection);
                    foreach (var eventListener in listeners.OfType<IPreDeleteEventListener>())
                    {
                        eventListener.Notify(args);
                    }
                }

                if (log != null && log.TraceEnabled) log.Trace(request.ToLogString());

                DeleteResponse response = null;
#if NET45
                await Task.Factory.FromAsync(
                    (callback, state) =>
                    {
                        return connection.BeginSendRequest(request, resultProcessing, callback, state);
                    },
                    (asyncresult) =>
                    {
                        response = (DeleteResponse)connection.EndSendRequest(asyncresult);
                        response.AssertSuccess();
                    },
                    null
                );
#else
                response = await Task.Run(() => connection.SendRequest(request) as DeleteResponse);
                response.AssertSuccess();
#endif

                if (listeners != null)
                {
                    var args = new ListenerPostArgs<string, DeleteRequest, DeleteResponse>(distinguishedName, request, response, connection);
                    foreach (var eventListener in listeners.OfType<IPostDeleteEventListener>())
                    {
                        eventListener.Notify(args);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("An error occurred while trying to delete '{0}'.", distinguishedName);
                if (log != null) log.Error(ex, message);
                throw;
            }
        }

        /// <summary>
        /// Updates the entry in the directory.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="entry">The entry to update.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <param name="listeners">The event listeners to be notified.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        public static async Task UpdateAsync(this LdapConnection connection, IDirectoryAttributes entry,
            ILinqToLdapLogger log = null, DirectoryControl[] controls = null, IEnumerable<IUpdateEventListener> listeners = null,
            PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            string distinguishedName = null;
            try
            {
                if (connection == null) throw new ArgumentNullException("connection");
                if (entry == null) throw new ArgumentNullException("entry");

                distinguishedName = entry.DistinguishedName;
                if (distinguishedName.IsNullOrEmpty()) throw new ArgumentException("entry.DistinguishedName is invalid.");

                var changes = entry.GetChangedAttributes();

                if (changes.Any())
                {
                    var request = new ModifyRequest(distinguishedName, changes.ToArray());
                    if (controls != null)
                    {
                        request.Controls.AddRange(controls);
                    }

                    if (listeners != null)
                    {
                        var args = new ListenerPreArgs<object, ModifyRequest>(entry, request, connection);
                        foreach (var eventListener in listeners.OfType<IPreUpdateEventListener>())
                        {
                            eventListener.Notify(args);
                        }
                    }

                    if (log != null && log.TraceEnabled) log.Trace(request.ToLogString());

                    ModifyResponse response = null;
#if NET45
                    await System.Threading.Tasks.Task.Factory.FromAsync(
                        (callback, state) =>
                        {
                            return connection.BeginSendRequest(request, resultProcessing, callback, state);
                        },
                        (asyncresult) =>
                        {
                            response = (ModifyResponse)connection.EndSendRequest(asyncresult);
                            response.AssertSuccess();
                        },
                        null
                    );
#else
                    response = await Task.Run(() => connection.SendRequest(request) as ModifyResponse);
                    response.AssertSuccess();
#endif

                    if (listeners != null)
                    {
                        var args = new ListenerPostArgs<object, ModifyRequest, ModifyResponse>(entry, request, response, connection);
                        foreach (var eventListener in listeners.OfType<IPostUpdateEventListener>())
                        {
                            eventListener.Notify(args);
                        }
                    }
                }
                else
                {
                    if (log != null && log.TraceEnabled) log.Trace(string.Format("No changes found for {0}.", distinguishedName));
                }
            }
            catch (Exception ex)
            {
                if (log != null) log.Error(ex, string.Format("An error occurred while trying to update '{0}'.", distinguishedName));

                throw;
            }
        }

        /// <summary>
        /// Updates the entry in the directory and returns the updated version from the directory.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="entry">The entry to update.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <param name="listeners">The event listeners to be notified.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        public static async Task<IDirectoryAttributes> UpdateAndGetAsync(this LdapConnection connection, IDirectoryAttributes entry,
            ILinqToLdapLogger log = null, DirectoryControl[] controls = null, IEnumerable<IUpdateEventListener> listeners = null,
            PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            await UpdateAsync(connection, entry, log, controls, listeners, resultProcessing);

            return await GetByDNAsync(connection, entry.DistinguishedName, log, null, resultProcessing);
        }

        /// <summary>
        /// List server information from RootDSE.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="attributes">
        /// Specify specific attributes to load.  Some LDAP servers require an explicit request for certain attributes.
        /// </param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        public static async Task<IDirectoryAttributes> ListServerAttributesAsync(this LdapConnection connection, string[] attributes = null, ILinqToLdapLogger log = null,
            PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            try
            {
                if (connection == null) throw new ArgumentNullException("connection");
                using (var provider = new DirectoryQueryProvider(
                    connection, SearchScope.Base, new ServerObjectMapping(), false)
                { Log = log, IsDynamic = true })
                {
                    var directoryQuery = new DirectoryQuery<IDirectoryAttributes>(provider);

                    var query = directoryQuery
                        .FilterWith("(objectClass=*)");

                    if (attributes?.Length > 0)
                    {
                        query = query.Select(attributes);
                    }

                    var results = await QueryableAsyncExtensions.FirstOrDefaultAsync(query, resultProcessing);

                    return results ?? new DirectoryAttributes();
                }
            }
            catch (Exception ex)
            {
                if (log != null) log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the attributes from the directory using the distinguished name.  <see cref="SearchScope.Base"/> is used.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="distinguishedName">The distinguished name to look for.</param>
        /// <param name="attributes">The attributes to load.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        public static async Task<IDirectoryAttributes> GetByDNAsync(this LdapConnection connection, string distinguishedName, ILinqToLdapLogger log = null, string[] attributes = null,
            PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            try
            {
                if (connection == null) throw new ArgumentNullException("connection");

                var request = new SearchRequest { DistinguishedName = distinguishedName, Scope = SearchScope.Base };

                if (attributes != null)
                    request.Attributes.AddRange(attributes);

                var transformer = new DynamicResultTransformer();

                if (log != null && log.TraceEnabled) log.Trace(request.ToLogString());

#if NET45
                return await System.Threading.Tasks.Task.Factory.FromAsync(
                    (callback, state) =>
                    {
                        return connection.BeginSendRequest(request, resultProcessing, callback, state);
                    },
                    (asyncresult) =>
                    {
                        var response = connection.EndSendRequest(asyncresult) as SearchResponse;
                        response.AssertSuccess();

                        return (response.Entries.Count == 0
                                ? transformer.Default()
                                : transformer.Transform(response.Entries[0])) as IDirectoryAttributes;
                    },
                    null
                );
#else
                var response = await Task.Run(() => connection.SendRequest(request) as SearchResponse);
                response.AssertSuccess();

                return (response.Entries.Count == 0
                        ? transformer.Default()
                        : transformer.Transform(response.Entries[0])) as IDirectoryAttributes;
#endif
            }
            catch (Exception ex)
            {
                if (log != null) log.Error(ex, string.Format("An error occurred while trying to retrieve '{0}'.", distinguishedName));
                throw;
            }
        }

        /// <summary>
        /// Moves the entry from one container to another without modifying the entry's name.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="currentDistinguishedName">The entry's current distinguished name</param>
        /// <param name="newNamingContext">The new container for the entry</param>
        /// <param name="deleteOldRDN">Maps to <see cref="P:System.DirectoryServices.Protocols.ModifyDNRequest.DeleteOldRdn"/>. Defaults to null to use default behavior</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="currentDistinguishedName"/> has an invalid format.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="currentDistinguishedName"/>
        /// or <paramref name="newNamingContext"/> are null, empty or white space.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapConnection">Thrown if the operation fails.</exception>
        public static async System.Threading.Tasks.Task<string> MoveEntryAsync(this LdapConnection connection, string currentDistinguishedName, string newNamingContext, ILinqToLdapLogger log = null,
            bool? deleteOldRDN = null, DirectoryControl[] controls = null, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            try
            {
                if (connection == null) throw new ArgumentNullException("connection");

                if (currentDistinguishedName.IsNullOrEmpty())
                    throw new ArgumentNullException("currentDistinguishedName");

                if (newNamingContext.IsNullOrEmpty())
                    throw new ArgumentNullException("newNamingContext");

                var name = DnParser.GetEntryName(currentDistinguishedName);

                var response = await SendModifyDnRequestAsync(connection, currentDistinguishedName, newNamingContext, name, deleteOldRDN, controls, log, resultProcessing);
                response.AssertSuccess();

                return string.Format("{0},{1}", name, newNamingContext);
            }
            catch (Exception ex)
            {
                if (log != null) log.Error(ex, string.Format("An error occurred while trying to move entry '{0}' to '{1}'.", currentDistinguishedName, newNamingContext));

                throw;
            }
        }

        /// <summary>
        /// Renames the entry within the same container. The <paramref name="newName"/> can be in the format
        /// XX=New Name or just New Name.
        /// </summary>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="currentDistinguishedName">The entry's current distinguished name</param>
        /// <param name="newName">The new name of the entry</param>
        /// <param name="deleteOldRDN">Maps to <see cref="P:System.DirectoryServices.Protocols.ModifyDNRequest.DeleteOldRdn"/>. Defaults to null to use default behavior</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="currentDistinguishedName"/> has an invalid format.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="currentDistinguishedName"/>
        /// or <paramref name="newName"/> are null, empty or white space.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapConnection">Thrown if the operation fails.</exception>
        public static async Task<string> RenameEntryAsync(this LdapConnection connection, string currentDistinguishedName, string newName, ILinqToLdapLogger log = null,
            bool? deleteOldRDN = null, DirectoryControl[] controls = null, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            try
            {
                if (connection == null) throw new ArgumentNullException("connection");

                if (currentDistinguishedName.IsNullOrEmpty())
                    throw new ArgumentNullException("currentDistinguishedName");

                if (newName.IsNullOrEmpty())
                    throw new ArgumentNullException("newName");

                newName = DnParser.FormatName(newName, currentDistinguishedName);
                var container = DnParser.GetEntryContainer(currentDistinguishedName);

                var response = await SendModifyDnRequestAsync(connection, currentDistinguishedName, container, newName, deleteOldRDN, controls, log, resultProcessing);
                response.AssertSuccess();

                return string.Format("{0},{1}", newName, container);
            }
            catch (Exception ex)
            {
                if (log != null) log.Error(ex, string.Format("An error occurred while trying to rename entry '{0}' to '{1}'.", currentDistinguishedName, newName));

                throw;
            }
        }

        /// <summary>
        /// Uses range retrieval to get all values for <paramref name="attributeName"/> on <paramref name="distinguishedName"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the attribute.  Must be <see cref="string"/> or <see cref="Array"/> of <see cref="byte"/>.</typeparam>
        /// <param name="connection">The connection to the directory.</param>
        /// <param name="log">The log for query information. Defaults to null.</param>
        /// <param name="distinguishedName">The distinguished name of the entry.</param>
        /// <param name="attributeName">The attribute to load.</param>
        /// <param name="start">The starting point for the range. Defaults to 0.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="distinguishedName"/> or <paramref name="attributeName"/> is null, empty or white space.
        /// </exception>
        /// <returns></returns>
        public static async Task<IList<TValue>> RetrieveRangesAsync<TValue>(this LdapConnection connection, string distinguishedName, string attributeName,
            int start = 0, ILinqToLdapLogger log = null, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            int idx = start;
            int step = 0;
            string currentRange = null;
            try
            {
                if (connection == null) throw new ArgumentNullException("connection");

                if (distinguishedName.IsNullOrEmpty()) throw new ArgumentNullException("distinguishedName");
                if (attributeName.IsNullOrEmpty()) throw new ArgumentNullException("attributeName");
                //Code pulled from http://dunnry.com/blog/2007/08/10/RangeRetrievalUsingSystemDirectoryServicesProtocols.aspx

                var list = new List<TValue>();
                var range = string.Format("{0};range={{0}}-{{1}}", attributeName);

                currentRange = string.Format(range, idx, "*");

                var request = new SearchRequest
                {
                    DistinguishedName = distinguishedName,
                    Filter = string.Format("({0}=*)", attributeName),
                    Scope = SearchScope.Base
                };
                request.Attributes.Add(currentRange);

                bool lastSearch = false;

                while (true)
                {
                    SearchResponse response = null;
#if NET45
                    await System.Threading.Tasks.Task.Factory.FromAsync(
                        (callback, state) =>
                        {
                            return connection.BeginSendRequest(request, resultProcessing, callback, state);
                        },
                        (asyncresult) =>
                        {
                            response = connection.EndSendRequest(asyncresult) as SearchResponse;
                        },
                        null
                    );
#else
                    response = await Task.Run(() => connection.SendRequest(request) as SearchResponse);
#endif

                    response.AssertSuccess();

                    if (response.Entries.Count != 1) break;

                    SearchResultEntry entry = response.Entries[0];

                    foreach (string attrib in entry.Attributes.AttributeNames)
                    {
                        currentRange = attrib;
                        lastSearch = currentRange.IndexOf("*", 0, StringComparison.Ordinal) > 0;
                        step = entry.Attributes[currentRange].Count;
                    }

                    foreach (TValue member in entry.Attributes[currentRange].GetValues(typeof(TValue)))
                    {
                        list.Add(member);
                        idx++;
                    }

                    if (lastSearch)
                        break;

                    currentRange = string.Format(range, idx, (idx + step));

                    request.Attributes.Clear();
                    request.Attributes.Add(currentRange);
                }

                return list;
            }
            catch (Exception ex)
            {
                if (log != null) log.Error(ex, string.Format("An error occurred while trying to retrieve ranges of type '{0}' for range '{1}'.", typeof(TValue).FullName, currentRange));

                throw;
            }
        }

        private static async Task<ModifyDNResponse> SendModifyDnRequestAsync(LdapConnection connection, string dn, string parentDn, string newName, bool? deleteOldRDN, DirectoryControl[] controls,
            ILinqToLdapLogger log = null, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            var request = new ModifyDNRequest
            {
                DistinguishedName = dn,
                NewParentDistinguishedName = parentDn,
                NewName = newName,
            };

            if (deleteOldRDN.HasValue)
            {
                request.DeleteOldRdn = deleteOldRDN.Value;
            }
            if (controls != null)
            {
                request.Controls.AddRange(controls);
            }
            if (log != null && log.TraceEnabled) log.Trace(request.ToLogString());

#if NET45
            return await Task.Factory.FromAsync(
                    (callback, state) =>
                    {
                        return connection.BeginSendRequest(request, resultProcessing, callback, state);
                    },
                    (asyncresult) =>
                    {
                        return connection.EndSendRequest(asyncresult) as ModifyDNResponse;
                    },
                    null
                );
#else
            return await Task.Run(() => connection.SendRequest(request) as ModifyDNResponse);
#endif
        }
    }
}

#endif