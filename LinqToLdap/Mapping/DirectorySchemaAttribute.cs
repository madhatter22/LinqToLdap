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
            get { return ObjectClasses?.FirstOrDefault(); }
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
        public bool IncludeObjectCategory { get; set; } = true;

        /// <summary>
        /// Indicates if the object classes should be included in filters.
        /// </summary>
        public bool IncludeObjectClasses { get; set; } = true;

        /// <summary>
        /// Indicates if this class should flatten its hierarchy when mapping. Flattened mappings will include inherited properties, but will not work with queries for subtypes or base types.
        /// </summary>
        public bool WithoutSubTypeMapping { get; set; }
    }
}