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
using System.Reflection;
using LinqToLdap.Collections;
using LinqToLdap.Exceptions;

namespace LinqToLdap.Mapping
{
    /// <summary>
    /// Maps classes for use in querying and updating a LDAP server.
    /// </summary>
    public interface IDirectoryMapper
    {
        /// <summary>
        /// Creates or retrieves the <see cref="IObjectMapping"/> from the classMap.
        /// </summary>
        /// <param name="classMap">The mapping.</param>
        /// <param name="objectCategory">The object category for the object.</param>
        /// <param name="includeObjectCategory">
        /// Indicates if the object category should be included in all queries.
        /// </param>
        /// <param name="namingContext">The location of the objects in the directory.</param>
        /// <param name="objectClasses">The object classes for the object.</param>
        /// <param name="includeObjectClasses">Indicates if the object classes should be included in all queries.</param>
        /// <exception cref="MappingException">
        /// Thrown if the mapping is invalid.
        /// </exception>
        /// <returns></returns>
        IObjectMapping Map(IClassMap classMap, string namingContext = null, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true, string objectCategory = null, bool includeObjectCategory = true);

        /// <summary>
        /// Creates or retrieves the <see cref="IObjectMapping"/> from <typeparam name="T"/>.
        /// </summary>
        /// <param name="namingContext">The optional naming context.  Used for <see cref="AutoClassMap{T}"/></param>
        /// <param name="objectClasses">The optional object classes.  Used for <see cref="AutoClassMap{T}"/></param>
        /// <param name="objectClass">The optional object class.  Used for <see cref="AutoClassMap{T}"/></param>
        /// <param name="objectCategory">The optional object category.  Used for <see cref="AutoClassMap{T}"/></param>
        /// <exception cref="MappingException">
        /// Thrown if the mapping is invalid.
        /// </exception>
        /// <returns></returns>
        IObjectMapping Map<T>(string namingContext = null, string objectClass = null, IEnumerable<string> objectClasses = null, string objectCategory = null) where T : class;

        /// <summary>
        /// Gets the mapping for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type for the mapping.</typeparam>
        /// <exception cref="MappingException">
        /// Thrown if the mapping is not found.
        /// </exception>
        /// <returns></returns>
        IObjectMapping GetMapping<T>() where T : class;

        /// <summary>
        /// Gets the mapping for <param name="type"/>.
        /// </summary>
        /// <exception cref="MappingException">
        /// Thrown if the mapping is not found.
        /// </exception>
        /// <returns></returns>
        IObjectMapping GetMapping(Type type);

        /// <summary>
        /// Returns all mappings tracked by this object.
        /// </summary>
        /// <returns></returns>
        ReadOnlyDictionary<Type, IObjectMapping> GetMappings();

        /// <summary>
        /// Indicates if a custom AutoMapping delegate has been provided
        /// </summary>
        bool HasCustomAutoMapping { get; }

        /// <summary>
        /// Indicates if a custom AttributeMapping delegate has been provided
        /// </summary>
        bool HasCustomAttributeMapping { get; }

        /// <summary>
        /// Adds all mappings from <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly containing the mappings.</param>
        void AddMappingsFrom(Assembly assembly);

        /// <summary>
        /// Adds all mappings in the assembly.
        /// </summary>
        /// <param name="assemblyName">
        /// The name of the assembly containing the mappings.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="assemblyName"/> is null, empty or white space.
        /// </exception>
        void AddMappingsFrom(string assemblyName);

        /// <summary>
        /// Provide a delegate that takes an object type and returns the class map for it.
        /// </summary>
        /// <param name="autoClassMapBuilder">The delegate.</param>
        /// <returns></returns>
        IDirectoryMapper AutoMapWith(Func<Type, IClassMap> autoClassMapBuilder);

        /// <summary>
        /// Provide a delegate that takes an object type and returns the class map for it.
        /// </summary>
        /// <param name="attributeClassMapBuilder">The delegate.</param>
        /// <returns></returns>
        IDirectoryMapper AttributeMapWith(Func<Type, IClassMap> attributeClassMapBuilder);
    }
}