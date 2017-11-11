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
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using LinqToLdap.Collections;
using LinqToLdap.Exceptions;

namespace LinqToLdap.Mapping
{
    /// <summary>
    /// Interface for an object mapped to a directory object
    /// </summary>
    public interface IObjectMapping
    {
        /// <summary>
        /// Mapped class type
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// 
        /// </summary>
        bool IsForAnonymousType { get; }

        /// <summary>
        /// Naming context for the mapped object
        /// </summary>
        string NamingContext { get; }

        /// <summary>
        /// Object category for the mapped object
        /// </summary>
        string ObjectCategory { get; }

        /// <summary>
        /// Indicates if the object category should always be included in queries.
        /// </summary>
        bool IncludeObjectCategory { get; }

        /// <summary>
        /// Object classes for the mapped object
        /// </summary>
        IEnumerable<string> ObjectClasses { get; }

        /// <summary>
        /// Indicates if this mapping has <see cref="SubTypeMappings"/>.
        /// </summary>
        bool HasSubTypeMappings { get; }

        /// <summary>
        /// Indicates if this mapping has a Catch All Property mapping.
        /// </summary>
        bool HasCatchAllMapping { get; }

        /// <summary>
        /// Indicates if the object classes should always be included in queries.
        /// </summary>
        bool IncludeObjectClasses { get; }

        /// <summary>
        /// Dictionary for properties mapped to attribute names
        /// </summary>
#if NET45
        System.Collections.ObjectModel.ReadOnlyDictionary<string, string> Properties { get; }
#else
        LinqToLdap.Collections.ReadOnlyDictionary<string, string> Properties { get; }
#endif
        
        /// <summary>
        /// All mapped properties
        /// </summary>
        /// <returns></returns>
        IEnumerable<IPropertyMapping> GetPropertyMappings();

        /// <summary>
        /// All mapped properties except those mapped as read-only, store-generated, or distinguished name.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IPropertyMapping> GetUpdateablePropertyMappings();

        /// <summary>
        /// Gets a property mapping by property name
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="owningType">The owning type of the property mapping</param>
        /// <returns></returns>
        /// <exception cref="MappingException">Thrown if the property mapping was not found.</exception>
        IPropertyMapping GetPropertyMapping(string name, Type owningType = null);

        /// <summary>
        /// Gets a property mapping by attribute name
        /// </summary>
        /// <param name="name">The name of the attribute</param>
        /// <param name="owningType">The owning type of the property mapping</param>
        /// <returns></returns>
        /// <exception cref="MappingException">Thrown if the property mapping was not found.</exception>
        IPropertyMapping GetPropertyMappingByAttribute(string name, Type owningType = null);

        /// <summary>
        /// Returns the distinguished name property mapping or null if there isn't one.
        /// </summary>
        ///<returns></returns>
        IPropertyMapping GetDistinguishedNameMapping();

        /// <summary>
        /// Returns the catch all property mapping or null if there isn't one.
        /// </summary>
        /// <returns></returns>
        IPropertyMapping GetCatchAllMapping();

        /// <summary>
        /// Creates an instance of the mapped class
        /// </summary>
        /// <param name="parameters">constructor parameters</param>
        /// <param name="objectClasses">The object classes used to determine if a subtype needs to be created.</param>
        /// <returns></returns>
        object Create(object[] parameters = null, object[] objectClasses = null);

        /// <summary>
        /// Mappings for objects that are subtypes of <see cref="Type"/>.
        /// </summary>
        ReadOnlyCollection<IObjectMapping> SubTypeMappings { get; }

        /// <summary>
        /// Adds a sub type mapping for this mapping.
        /// </summary>
        /// <param name="mapping"></param>
        void AddSubTypeMapping(IObjectMapping mapping);
    }
}
