/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System.DirectoryServices.Protocols;

namespace LinqToLdap.Transformers
{
    /// <summary>
    /// Interface for transforming query results
    /// </summary>
    public interface IResultTransformer
    {
        /// <summary>
        /// Transform the <paramref name="entry"/>
        /// </summary>
        /// <param name="entry">The entry to transform</param>
        /// <returns></returns>
        object Transform(SearchResultEntry entry);

        /// <summary>
        /// Get the default value of the transform.
        /// </summary>
        /// <returns></returns>
        object Default();
    }
}
