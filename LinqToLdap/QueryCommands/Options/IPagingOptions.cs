/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

namespace LinqToLdap.QueryCommands.Options
{
    /// <summary>
    /// Interface for paging options from a Query.
    /// </summary>
    public interface IPagingOptions
    {
        /// <summary>
        /// The next page.
        /// </summary>
        byte[] NextPage { get; }

        /// <summary>
        /// The size of the page.
        /// </summary>
        int PageSize { get; }
    }
}