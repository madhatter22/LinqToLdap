using LinqToLdap.Collections;
using LinqToLdap.Exceptions;
using LinqToLdap.Mapping;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;

namespace LinqToLdap
{
    ///<summary>
    /// Interface for performing LINQ queries against a directory
    ///</summary>
    public interface IDirectoryContext : IDisposable
    {
        /// <summary>
        /// Retrieves the mapped class from the directory using the distinguished name.  <see cref="SearchScope.Base"/> is used.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name to look for.</param>
        /// <typeparam name="T">The type of mapped object</typeparam>
        /// <returns></returns>
        T GetByDN<T>(string distinguishedName) where T : class;

        /// <summary>
        /// Retrieves the attributes from the directory using the distinguished name.  <see cref="SearchScope.Base"/> is used.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name to look for.</param>
        /// <param name="attributes">The attributes to load.</param>
        /// <returns></returns>
        IDirectoryAttributes GetByDN(string distinguishedName, params string[] attributes);

        /// <summary>
        /// Creates a query against the directory for a directory entries.
        /// </summary>
        /// <param name="namingContext">The place in the directory from which you want to start your search.</param>
        /// <param name="scope">Determines the depth at which the search is performed</param>
        /// <param name="objectClass">The object class in the directory for the type.</param>
        /// <param name="objectClasses">The object classes in the directory for the type.</param>
        /// <param name="objectCategory">The object category in the directory for the type</param>
        /// <returns></returns>
        IQueryable<IDirectoryAttributes> Query(string namingContext, SearchScope scope = SearchScope.Subtree,
                                  string objectClass = null,
                                  IEnumerable<string> objectClasses = null,
                                  string objectCategory = null);

        /// <summary>
        /// Creates a query against the directory.
        /// </summary>
        /// <typeparam name="T">Directory type</typeparam>
        /// <param name="scope">Determines the depth at which the search is performed</param>
        /// <param name="namingContext">Optional naming context to override the mapped naming context.</param>
        /// <returns></returns>
        IQueryable<T> Query<T>(SearchScope scope = SearchScope.Subtree, string namingContext = null) where T : class;

        /// <summary>
        /// Creates a query against the directory.
        /// </summary>
        /// <typeparam name="T">Directory type</typeparam>
        /// <param name="example">An anonymous object that can be used for auto mapping.</param>
        /// <param name="namingContext">The place in the directory from which you want to start your search.</param>
        /// <param name="objectClass">The object class in the directory for the type.</param>
        /// <param name="objectClasses">The object classes in the directory for the type.</param>
        /// <param name="objectCategory">The object category in the directory for the type</param>
        /// <returns></returns>
        IQueryable<T> Query<T>(T example, string namingContext, string objectClass = null, IEnumerable<string> objectClasses = null, string objectCategory = null) where T : class;

        /// <summary>
        /// Creates a query against the directory.
        /// </summary>
        /// <typeparam name="T">Directory type</typeparam>
        /// <param name="scope">Determines the depth at which the search is performed</param>
        /// <param name="example">An anonymous object that can be used for auto mapping.</param>
        /// <param name="namingContext">The place in the directory from which you want to start your search.</param>
        /// <param name="objectClass">The object class in the directory for the type.</param>
        /// <param name="objectClasses">The object classes in the directory for the type.</param>
        /// <param name="objectCategory">The object category in the directory for the type</param>
        /// <returns></returns>
        IQueryable<T> Query<T>(T example, SearchScope scope, string namingContext, string objectClass = null, IEnumerable<string> objectClasses = null,
                               string objectCategory = null) where T : class;

        /// <summary>
        /// List server information from RootDSE.
        /// </summary>
        /// <returns></returns>
        IDirectoryAttributes ListServerAttributes(params string[] attributes);

        /// <summary>
        /// Adds the entry to the directory and returns the newly saved entry from the directory. If the <paramref name="distinguishedName"/> is
        /// null then a mapped distinguished name property is used.
        /// </summary>
        /// <typeparam name="T">The type of entry.</typeparam>
        /// <param name="entry">The object to save.</param>
        /// <param name="distinguishedName">The distinguished name for the entry.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if entry is null</exception>
        /// <exception cref="ArgumentException">Thrown if distinguished name is null and there is no mapped distinguished name property.</exception>
        /// <exception cref="MappingException">
        /// Thrown if <paramref name="distinguishedName"/> is null and Distinguished Name is not mapped.
        /// Thrown if object class or object category have not been mapped.
        /// Thrown if <typeparamref name="T"/> has not been mapped.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the add was not successful.</exception>
        /// <exception cref="LdapException">Thrown if the add was not successful.</exception>
        T AddAndGet<T>(T entry, string distinguishedName = null, DirectoryControl[] controls = null) where T : class;

        /// <summary>
        /// Adds the entry to the directory and returns the newly saved entry from the directory.
        /// </summary>
        /// <param name="entry">The attributes for the entry</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the add was not successful.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        IDirectoryAttributes AddAndGet(IDirectoryAttributes entry, DirectoryControl[] controls);

        /// <summary>
        /// Adds the entry to the directory and returns the newly saved entry from the directory.
        /// </summary>
        /// <param name="entry">The attributes for the entry</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the add was not successful.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        IDirectoryAttributes AddAndGet(IDirectoryAttributes entry);

        /// <summary>
        /// Adds the entry to the directory and returns the newly saved entry from the directory.
        /// </summary>
        /// <param name="entry">The attributes for the entry</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the add was not successful.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        IDirectoryAttributes AddAndGet(DirectoryAttributes entry, DirectoryControl[] controls);

        /// <summary>
        /// Adds the entry to the directory and returns the newly saved entry from the directory.
        /// </summary>
        /// <param name="entry">The attributes for the entry</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the add was not successful.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        IDirectoryAttributes AddAndGet(DirectoryAttributes entry);

        /// <summary>
        /// Adds the entry to the directory. If the <paramref name="distinguishedName"/> is
        /// null then a mapped distinguished name property is used.
        /// </summary>
        /// <typeparam name="T">The type of entry.</typeparam>
        /// <param name="entry">The object to save.</param>
        /// <param name="distinguishedName">The distinguished name for the entry.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if entry is null</exception>
        /// <exception cref="ArgumentException">Thrown if distinguished name is null and there is no mapped distinguished name property.</exception>
        /// <exception cref="MappingException">
        /// Thrown if <paramref name="distinguishedName"/> is null and Distinguished Name is not mapped.
        /// Thrown if object class or object category have not been mapped.
        /// Thrown if <typeparamref name="T"/> has not been mapped.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the add was not successful.</exception>
        /// <exception cref="LdapException">Thrown if the add was not successful.</exception>
        void Add<T>(T entry, string distinguishedName = null, DirectoryControl[] controls = null) where T : class;

        /// <summary>
        /// Adds the entry to the directory.
        /// </summary>
        /// <param name="entry">The attributes for the entry</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the add was not successful.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        void Add(IDirectoryAttributes entry, DirectoryControl[] controls);

        /// <summary>
        /// Adds the entry to the directory.
        /// </summary>
        /// <param name="entry">The attributes for the entry</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the add was not successful.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        void Add(IDirectoryAttributes entry);

        /// <summary>
        /// Adds the entry to the directory.
        /// </summary>
        /// <param name="entry">The attributes for the entry</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the add was not successful.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        void Add(DirectoryAttributes entry, DirectoryControl[] controls);

        /// <summary>
        /// Adds the entry to the directory.
        /// </summary>
        /// <param name="entry">The attributes for the entry</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the add was not successful.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        void Add(DirectoryAttributes entry);

        /// <summary>
        /// Deletes an entry from the directory.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the entry</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="distinguishedName"/> is null, empty or white space.</exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapException">Thrown if the operation fails.</exception>
        void Delete(string distinguishedName, params DirectoryControl[] controls);

        /// <summary>
        /// Updates the entry in the directory and returns the updated version from the directory. If the <paramref name="distinguishedName"/> is
        /// null then a mapped distinguished name property is used.
        /// </summary>
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
        T UpdateAndGet<T>(T entry, string distinguishedName = null, DirectoryControl[] controls = null) where T : class;

        /// <summary>
        /// Updates the entry in the directory and returns the updated version from the directory.
        /// </summary>
        /// <param name="entry">The attributes for the entry.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        IDirectoryAttributes UpdateAndGet(IDirectoryAttributes entry);

        /// <summary>
        /// Updates the entry in the directory and returns the updated version from the directory.
        /// </summary>
        /// <param name="entry">The attributes for the entry.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        IDirectoryAttributes UpdateAndGet(DirectoryAttributes entry);

        /// <summary>
        /// Updates the entry in the directory and returns the updated version from the directory.
        /// </summary>
        /// <param name="entry">The attributes for the entry.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        IDirectoryAttributes UpdateAndGet(IDirectoryAttributes entry, DirectoryControl[] controls);

        /// <summary>
        /// Updates the entry in the directory. If the <paramref name="distinguishedName"/> is
        /// null then a mapped distinguished name property is used.
        /// </summary>
        /// <param name="entry">The entry to update</param>
        /// <param name="distinguishedName">The distinguished name for the entry.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <typeparam name="T">The type of entry.</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if entry is null</exception>
        /// <exception cref="MappingException">
        /// Thrown if <paramref name="distinguishedName"/> is null and Distinguished Name is not mapped.
        /// Thrown if object class or object category have not been mapped.
        /// Thrown if <typeparamref name="T"/> has not been mapped.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if distinguished name is null and there is no mapped distinguished name property.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="entry"/> is <see cref="DirectoryObjectBase"/> but the entry is not tracking changes.</exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation is not successful</exception>
        /// <exception cref="LdapException">Thrown if the operation is not successful</exception>
        void Update<T>(T entry, string distinguishedName = null, DirectoryControl[] controls = null) where T : class;

        /// <summary>
        /// Updates the entry in the directory.
        /// </summary>
        /// <param name="entry">The attributes for the entry.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        void Update(IDirectoryAttributes entry);

        /// <summary>
        /// Updates the entry in the directory.
        /// </summary>
        /// <param name="entry">The attributes for the entry.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        void Update(DirectoryAttributes entry);

        /// <summary>
        /// Updates the entry in the directory.
        /// </summary>
        /// <param name="entry">The attributes for the entry.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entry"/> is null.
        /// </exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails</exception>
        /// <exception cref="LdapException">Thrown if the operation fails</exception>
        void Update(IDirectoryAttributes entry, DirectoryControl[] controls);

        /// <summary>
        /// Adds the attribute to an entry.
        /// </summary>
        /// <param name="distinguishedName">The entry</param>
        /// <param name="attributeName">The name of the attribute</param>
        /// <param name="value">The value for the entry.</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapConnection">Thrown if the operation fails.</exception>
        void AddAttribute(string distinguishedName, string attributeName, object value = null, DirectoryControl[] controls = null);

        /// <summary>
        /// Removes the attribute from an entry.
        /// </summary>
        /// <param name="distinguishedName">The entry</param>
        /// <param name="attributeName">The name of the attribute</param>
        /// <param name="controls">Any <see cref="DirectoryControl"/>s to be sent with the request</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="distinguishedName"/> or <paramref name="attributeName"/> is null, empty or white space.</exception>
        /// <exception cref="DirectoryOperationException">Thrown if the operation fails.</exception>
        /// <exception cref="LdapConnection">Thrown if the operation fails.</exception>
        void DeleteAttribute(string distinguishedName, string attributeName, params DirectoryControl[] controls);

        /// <summary>
        /// Moves the entry from one container to another without modifying the entry's name and return's the new distinguished name.
        /// </summary>
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
        string MoveEntry(string currentDistinguishedName, string newNamingContext, bool? deleteOldRDN = null, params DirectoryControl[] controls);

        /// <summary>
        /// Renames the entry within the same container and return's the new distinguished name. The <paramref name="newName"/> should be in the format
        /// XX=New Name.
        /// </summary>
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
        string RenameEntry(string currentDistinguishedName, string newName, bool? deleteOldRDN = null, params DirectoryControl[] controls);

        /// <summary>
        /// Uses range retrieval to get all values for <paramref name="attributeName"/> on <paramref name="distinguishedName"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the attribute.  Must be <see cref="string"/> or <see cref="Array"/> of <see cref="byte"/>.</typeparam>
        /// <param name="distinguishedName">The distinguished name of the entry.</param>
        /// <param name="attributeName">The attribute to load.</param>
        /// <param name="start">The starting point for the range. Defaults to 0.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="distinguishedName"/> or <paramref name="attributeName"/> is null, empty or white space.
        /// </exception>
        /// <returns></returns>
        IList<TValue> RetrieveRanges<TValue>(string distinguishedName, string attributeName, int start = 0);

        /// <summary>
        /// Sends the request to the directory.
        /// </summary>
        /// <param name="request">The response from the directory</param>
        /// <returns></returns>
        DirectoryResponse SendRequest(DirectoryRequest request);
    }
}