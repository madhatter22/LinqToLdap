using System.Collections.Generic;

namespace LinqToLdap.Collections
{
    /// <summary>
    /// Represents a page of items
    /// </summary>
    /// <typeparam name="T">The type of items</typeparam>
    public interface ILdapPage<T> : IEnumerable<T>
    {
        /// <summary>
        /// The actual number of items in the page.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// The size of the page.
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Indicates if there is another page.
        /// </summary>
        bool HasNextPage { get; }

        /// <summary>
        /// The cookie for the next page.
        /// </summary>
        byte[] NextPage { get; }

        /// <summary>
        /// The LDAP Filter used to produce this page.
        /// </summary>
        string Filter { get; }
    }
}