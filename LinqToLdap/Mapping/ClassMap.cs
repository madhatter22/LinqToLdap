using LinqToLdap.Collections;
using LinqToLdap.Exceptions;
using LinqToLdap.Mapping.PropertyMappingBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqToLdap.Mapping
{
    /// <summary>
    /// Defines a mapping for a directory entry. Derive from this class to create a mapping,
    /// and use the constructor to control how your object is queried.
    /// </summary>
    /// <example>
    /// public class UserMap : ClassMap&lt;User&gt;
    /// {
    ///   public UserMap()
    ///   {
    ///     Map(x => x.Name)
    ///       .Named("displayname");
    ///     Map(x => x.Age);
    ///   }
    /// }
    /// </example>
    /// <typeparam name="T">Type to map</typeparam>
    public abstract partial class ClassMap<T> : IClassMap where T : class
    {
        private string _namingContext;
        private string _objectCategory;
        private IEnumerable<string> _objectClass;

        /// <summary>
        /// Flags used for looking up properties
        /// </summary>
        protected const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// All mapped properties
        /// </summary>
        public readonly List<IPropertyMappingBuilder> PropertyMappings = new List<IPropertyMappingBuilder>();

        /// <summary>
        /// Constructs the class map
        /// </summary>
        protected ClassMap()
        {
            IsForAnonymousType = typeof(T).IsAnonymous();
        }

        /// <summary>
        /// The <see cref="System.Type"/> of the class map.
        /// </summary>
        public Type Type => typeof(T);

        /// <summary>
        /// Indicates if the mapping is for an anonymous type.
        /// </summary>
        protected bool IsForAnonymousType { get; }

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
        public abstract IClassMap PerformMapping(string namingContext = null, string objectCategory = null, bool includeObjectCategory = true, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true);

        /// <summary>
        /// Validates the mapping
        /// </summary>
        /// <exception cref="MappingException">
        /// Thrown if naming context is null or if no properties have been mapped
        /// </exception>
        public void Validate()
        {
            if (PropertyMappings.Count == 0)
                throw new MappingException("At least one property must be mapped.");
        }

        /// <summary>
        /// Gets the naming context
        /// </summary>
        /// <returns></returns>
        protected string GetNamingContext()
        {
            return _namingContext;
        }

        /// <summary>
        /// Indicates if object category will be included in all queries.
        /// </summary>
        protected bool IncludeObjectCategory { get; private set; }

        /// <summary>
        /// Indicates if object classes will be included in all queries.
        /// </summary>
        protected bool IncludeObjectClasses { get; private set; }

        /// <summary>
        /// Indicates if this class should flatten its hierarchy when mapping. Flattened mappings will include inherited properties, but will not work with queries for subtypes or base types.
        /// </summary>
        public bool WithoutSubTypeMapping { get; set; }

        /// <summary>
        /// Gets the object category
        /// </summary>
        /// <returns></returns>
        protected string GetObjectCategory()
        {
            return _objectCategory;
        }

        /// <summary>
        /// Gets the object class
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<string> GetObjectClass()
        {
            return _objectClass;
        }

        /// <summary>
        /// Produces a final mapping used for object construction from the directory
        /// </summary>
        /// <returns></returns>
        public virtual IObjectMapping ToObjectMapping()
        {
            return new StandardObjectMapping<T>(GetNamingContext(),
                                                PropertyMappings.Select(pmb => pmb.ToPropertyMapping()),
                                                GetObjectCategory(), IncludeObjectCategory, GetObjectClass(), IncludeObjectClasses)
            {
                WithoutSubTypeMapping = WithoutSubTypeMapping
            };
        }

        /// <summary>
        /// Set the naming context for the directory entry
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the naming context</param>
        protected void NamingContext(string distinguishedName)
        {
            _namingContext = distinguishedName;
        }

        /// <summary>
        /// Set the object category for the directory entry
        /// </summary>
        /// <param name="objectCategory">The object category</param>
        /// <param name="includeObjectCategory">Indicates if the object category should be included in filters.</param>
        protected void ObjectCategory(string objectCategory, bool includeObjectCategory = true)
        {
            _objectCategory = objectCategory;
            IncludeObjectCategory = !_objectCategory.IsNullOrEmpty() && includeObjectCategory;
        }

        /// <summary>
        /// Set the object classes for the directory entry
        /// </summary>
        /// <param name="objectClass">The object class</param>
        /// <param name="includeObjectClasses">Indicates if the object classes should be included in filters.</param>
        protected void ObjectClasses(IEnumerable<string> objectClass, bool includeObjectClasses = true)
        {
            _objectClass = objectClass != null && objectClass.Any()
                ? objectClass
                : null;
            IncludeObjectClasses = _objectClass != null && _objectClass.Any() && includeObjectClasses;
        }

        /// <summary>
        /// Set the object class for the directory entry
        /// </summary>
        /// <param name="objectClass">The object class</param>
        /// <param name="includeObjectClass">Indicates if the object class should be included in queries</param>
        protected void ObjectClass(string objectClass, bool includeObjectClass = true)
        {
            if (!objectClass.IsNullOrEmpty())
            {
                _objectClass = new List<string>(1) { objectClass };
            }
            IncludeObjectClasses = !objectClass.IsNullOrEmpty() && includeObjectClass;
        }

        /// <summary>
        /// Maps the <paramref name="property"/> as the distinguished name.
        /// </summary>
        /// <param name="property">The distinguished name property.</param>
        /// <param name="attributeName">The name of the distinguished name attribute. Defaults to "distinguishedname".</param>
        protected void DistinguishedName(Expression<Func<T, string>> property, string attributeName = "distinguishedname")
        {
            var propertyInfo = GetPropertyInfo(property.Body);
            DistinguishedName(propertyInfo, attributeName);
        }

        internal void DistinguishedName(PropertyInfo property, string attributeName = "distinguishedname")
        {
            if (attributeName.IsNullOrEmpty()) throw new MappingException("DistinguishedName must have an attribute name.");

            Map<string>(property, true).Named(attributeName);
        }

        internal void CatchAll(PropertyInfo property)
        {
            Map<IDirectoryAttributes>(property, isReadOnly: true);
        }

        private PropertyInfo GetPropertyInfo(Expression expression)
        {
            if (!(expression is MemberExpression))
                throw new ArgumentException("Expected MemberAccess expression but was " + expression.NodeType);

            var member = (expression as MemberExpression).Member;

            if (PropertyMappings.Any(p => p.PropertyName == member.Name))
            {
                throw new MappingException($"{member.Name} is already mapped for {typeof(T).FullName}");
            }

            var propertyInfo = typeof(T).GetProperty(member.Name, Flags);
            if (propertyInfo == null)
            {
                throw new MappingException($"Property named {member.Name} not found for type {typeof(T).FullName}");
            }
            if (propertyInfo.GetSetMethod(true) == null || propertyInfo.GetGetMethod(true) == null)
            {
                throw new MappingException("Cannot map a property without a getter and setter.");
            }

            return propertyInfo;
        }

        internal IPropertyMapperGeneric<TProperty> Map<TProperty>(PropertyInfo propertyInfo, bool isDistinguishedName = false, bool isReadOnly = false)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

            if (isDistinguishedName && PropertyMappings.Any(p => p.IsDistinguishedName))
                throw new MappingException("Cannot specify more than one DistinguishedName property.");

            if ((typeof(IDirectoryAttributes).IsAssignableFrom(propertyInfo.PropertyType))
                &&
                PropertyMappings.Any(
                    p =>
                        typeof(IDirectoryAttributes).IsAssignableFrom(p.PropertyInfo.PropertyType)))
            {
                throw new MappingException("Cannot specify more than one CatchAll property.");
            }

            var mapping = new PropertyMappingBuilder<T, TProperty>(propertyInfo, isDistinguishedName, isReadOnly);

            PropertyMappings.Add(mapping);

            return mapping;
        }

        internal TBuilder Map<TBuilder>(TBuilder builder) where TBuilder : IPropertyMappingBuilder
        {
            if (builder.PropertyInfo == null) throw new ArgumentNullException(nameof(builder));

            if (builder.IsDistinguishedName && PropertyMappings.Any(p => p.IsDistinguishedName))
                throw new MappingException("Cannot specify more than one DistinguishedName property.");

            PropertyMappings.Add(builder);

            return builder;
        }

        /// <summary>
        /// Allows for mapping a raw <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="propertyInfo">The property to map</param>
        /// <param name="isDistinguishedName">Indicates if the attribute is the distinguishedname.</param>
        /// <param name="isReadOnly">Indicates if the property should be treated as read only in the directory.</param>
        /// <returns></returns>
        protected IPropertyMapper MapPropertyInfo(PropertyInfo propertyInfo, bool isDistinguishedName = false, bool isReadOnly = false)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

            if (isDistinguishedName && PropertyMappings.Any(p => p.IsDistinguishedName))
                throw new MappingException("Cannot specify more than one DistinguishedName property.");

            if (isReadOnly && PropertyMappings.Any(p => p.IsReadOnly))
                throw new MappingException("Cannot specify more than one CommonName property.");

            if (PropertyMappings.Any(p => p.PropertyName == propertyInfo.Name))
            {
                throw new MappingException($"{propertyInfo.Name} is already mapped for {typeof(T).FullName}");
            }

            if (!IsForAnonymousType && (propertyInfo.GetSetMethod(true) == null || propertyInfo.GetGetMethod(true) == null))
            {
                throw new MappingException("Cannot map a property without a getter and setter.");
            }

            var type = typeof(PropertyMappingBuilder<,>).MakeGenericType(typeof(T), propertyInfo.PropertyType);

            var mapping = Activator.CreateInstance(type, new object[] { propertyInfo, isDistinguishedName, isReadOnly }) as IPropertyMappingBuilder;

            PropertyMappings.Add(mapping);

            return mapping as IPropertyMapper;
        }

        /// <summary>
        /// Create a custom property mapping.
        /// </summary>
        /// <param name="property">Property to map</param>
        /// <example>
        /// MapCustomProperty(x => x.Name);
        /// </example>
        protected ICustomPropertyMapper<T, TProperty> MapCustomProperty<TProperty>(Expression<Func<T, TProperty>> property)
        {
            var propertyInfo = GetPropertyInfo(property.Body);

            var mapping = new CustomPropertyMappingBuilder<T, TProperty>(propertyInfo);

            PropertyMappings.Add(mapping);

            return mapping;
        }
    }
}