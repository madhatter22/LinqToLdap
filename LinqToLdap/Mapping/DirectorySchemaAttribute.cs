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

namespace LinqToLdap.Mapping
{
    /// <summary>
    /// Maps an object to a naming context in the directory
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DirectorySchemaAttribute : Attribute
    {
        private bool _includeObjectCategory = true;
        private bool _includeObjectClasses = true;

        /// <summary>
        /// Maps the object to the naming context
        /// </summary>
        /// <param name="namingContext">The naming context</param>
        public DirectorySchemaAttribute(string namingContext)
        {
            NamingContext = namingContext;
        }

        /// <summary>
        /// Mapped object classes
        /// </summary>
        public string[] ObjectClasses { get; set; }

        /// <summary>
        /// Mapped object class
        /// </summary>
        public string ObjectClass
        {
            get { return ObjectClasses != null ? ObjectClasses.FirstOrDefault() : null; }
            set
            {
                if (!value.IsNullOrEmpty())
                {
                    ObjectClasses = new[] { value };
                }
            }
        }

        /// <summary>
        /// Mapped naming context
        /// </summary>
        public string NamingContext { get; private set; }

        /// <summary>
        /// Mapped object category
        /// </summary>
        public string ObjectCategory { get; set; }

        /// <summary>
        /// Indicates if the object category should be included in filters.
        /// </summary>
        public bool IncludeObjectCategory
        {
            get { return _includeObjectCategory; }
            set { _includeObjectCategory = value; }
        }

        /// <summary>
        /// Indicates if the object classes should be included in filters.
        /// </summary>
        public bool IncludeObjectClasses
        {
            get { return _includeObjectClasses; }
            set { _includeObjectClasses = value; }
        }
    }
}
