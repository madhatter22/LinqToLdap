/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using System.Collections.Generic;
using LinqToLdap.Exceptions;

namespace LinqToLdap.Mapping
{
    /// <summary>
    /// Interface for a class that maps an object to the directory
    /// </summary>
    public interface IClassMap
    {
        /// <summary>
        /// The <see cref="System.Type"/> of the class map.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Maps the schema and property information.
        /// </summary>
        /// <param name="objectCategory">The object category for the object.</param>
        /// <param name="includeObjectCategory">
        /// Indicates if the object category should be included in all queries.
        /// </param>
        /// <param name="namingContext">The location of the objects in the directory.</param>
        /// <param name="objectClasses">The object classes for the object.</param>
        /// <param name="includeObjectClasses">Indicates if the object classes should be included in all queries.</param>
        /// <returns></returns>
        IClassMap PerformMapping(string namingContext = null, string objectCategory = null, bool includeObjectCategory = true, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true);

        /// <summary>
        /// Validates the mapping.  Should throw a <see cref="MappingException"/> if the mapping is not valid.
        /// </summary>
        /// <exception cref="MappingException">Thrown if the mapping is invalid.</exception>
        void Validate();

        /// <summary>
        /// Produces a final mapping used for object contruction from the directory
        /// </summary>
        /// <returns></returns>
        IObjectMapping ToObjectMapping();
    }
}
