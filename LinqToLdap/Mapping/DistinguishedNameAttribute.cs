/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;

namespace LinqToLdap.Mapping
{
    /// <summary>
    /// Indicates that the property will contain the distinguished name for a class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DistinguishedNameAttribute : Attribute
    {
        /// <summary>
        /// Maps a property to the specified <paramref name="attributeName"/>.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        public DistinguishedNameAttribute(string attributeName = "distinguishedname")
        {
            AttributeName = attributeName;
        }

        /// <summary>
        /// The mapped attribute name.
        /// </summary>
        public string AttributeName { get; private set; }
    }
}
