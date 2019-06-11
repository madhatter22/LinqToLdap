using System.Collections.Generic;

namespace LinqToLdap.Collections
{
    /// <summary>
    /// Represents a page of items
    /// </summary>
    /// <typeparam name="T">The type of items</typeparam>
    public class LdapPage<T> : List<T>, ILdapPage<T>
    {
        /// <summary>
        /// Creates a a paged collection.
        /// </summary>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="nextPage">The cookie for the next page</param>
        /// <param name="enumerable">The items for the page</param>
        /// <param name="filter">Filter used to produce this page</param>
        public LdapPage(int pageSize, byte[] nextPage, IEnumerable<T> enumerable, string filter)
            : this(pageSize, nextPage, enumerable)
        {
            Filter = filter;
        }

        /// <summary>
        /// Creates a a paged collection.
        /// </summary>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="nextPage">The cookie for the next page</param>
        /// <param name="page">The items for the page</param>
        public LdapPage(int pageSize, byte[] nextPage, IEnumerable<T> page)
            : base(page)
        {
            PageSize = pageSize;
            NextPage = nextPage;

            HasNextPage = NextPage != null && NextPage.Length > 0;
        }

        /// <summary>
        /// The size of the page.
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Indicates if there is another page.
        /// </summary>
        public bool HasNextPage { get; private set; }

        /// <summary>
        /// The cookie for the next page.
        /// </summary>
        public byte[] NextPage { get; private set; }

        /// <summary>
        /// The LDAP Filter used to produce this page.
        /// </summary>
        public string Filter { get; private set; }
    }
}