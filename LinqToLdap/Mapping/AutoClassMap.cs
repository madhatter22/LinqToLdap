/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using System.Linq;
using LinqToLdap.Collections;

namespace LinqToLdap.Mapping
{
    using System.Collections.Generic;

    /// <summary>
    /// Generates a class map via convention.  Property names will map directly to their attribute names.
    /// </summary>
    /// <typeparam name="T">Class to map</typeparam>
    public class AutoClassMap<T> : ClassMap<T> where T : class 
    {
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
        public override IClassMap PerformMapping(string namingContext = null, string objectCategory = null, bool includeObjectCategory = true, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true)
        {
            NamingContext(namingContext);
            ObjectCategory(objectCategory, includeObjectCategory);
            ObjectClasses(objectClasses, includeObjectClasses);

            var type = typeof(T);

            var properties = (IsForAnonymousType
                ? type.GetProperties(Flags)
                : type.GetProperties(Flags)
                    .Where(p => p.GetGetMethod() != null && p.GetSetMethod() != null)).ToList();

            properties
                .ForEach(
                    p =>
                        MapPropertyInfo(p,
                            p.Name.Equals("DistinguishedName", StringComparison.OrdinalIgnoreCase) ||
                            p.Name.Equals("entrydn", StringComparison.OrdinalIgnoreCase),
                            p.Name.Equals("cn", StringComparison.OrdinalIgnoreCase) ||
                            p.Name.Equals("ou", StringComparison.OrdinalIgnoreCase)));

            var catchAll = properties
                .FirstOrDefault(p => typeof(IDirectoryAttributes).IsAssignableFrom(p.PropertyType));

            if (catchAll != null)
            {
                CatchAll(catchAll);
            }

            return this;
        }

        /// <summary>
        /// Converts the mapping to functional object mapping.
        /// </summary>
        /// <returns></returns>
        public override IObjectMapping ToObjectMapping()
        {
            return IsForAnonymousType
                       ? new AnonymousObjectMapping<T>(GetNamingContext(),
                                                       PropertyMappings.Select(pm => pm.ToPropertyMapping()),
                                                       GetObjectCategory(),
                                                       IncludeObjectCategory,
                                                       GetObjectClass(),
                                                       IncludeObjectClasses)
                       : base.ToObjectMapping();
        }
    }
}
