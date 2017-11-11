/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

namespace LinqToLdap
{
    /// <summary>
    /// Enum for indicating Object Classes and or Object Category to be included / excluded from queries.
    /// </summary>
    public enum OC
    {
        /// <summary>
        /// Only Object Category
        /// </summary>
        ObjectCategory,
        /// <summary>
        /// Only Object Classes
        /// </summary>
        ObjectClasses,
        /// <summary>
        /// Object Category and Object Classes
        /// </summary>
        Both
    }
}