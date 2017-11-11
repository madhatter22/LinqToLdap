/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */


using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using LinqToLdap.Collections;

namespace LinqToLdap.Mapping
{
    /// <summary>
    /// Interface for updatable directory objects.
    /// </summary>
    public interface IDirectoryObject
    {
        /// <summary>
        /// Gets the changes to send to the directory.
        /// </summary>
        /// <param name="mapping">The mapping for the object.</param>
        /// <returns></returns>
        IEnumerable<DirectoryAttributeModification> GetChanges(IObjectMapping mapping);

        /// <summary>
        /// The original property values loaded form the directory for this object.
        /// </summary>
        OriginalValuesCollection OriginalValues { get; set; }
    }
}
