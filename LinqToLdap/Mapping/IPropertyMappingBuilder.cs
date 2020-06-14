using System.Reflection;

namespace LinqToLdap.Mapping
{
    /// <summary>
    /// Interface for class that can build <see cref="IPropertyMapping"/>
    /// </summary>
    public interface IPropertyMappingBuilder
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Indicates if the builder is for a distinguished name property.
        /// </summary>
        bool IsDistinguishedName { get; }

        /// <summary>
        /// Indicates if the property is read only
        /// </summary>
        ReadOnly? ReadOnlyConfiguration { get; }

        /// <summary>
        /// The name of the attribute in the directory.
        /// </summary>
        string AttributeName { get; }

        /// <summary>
        /// The <see cref="PropertyInfo"/>.
        /// </summary>
        PropertyInfo PropertyInfo { get; }

        /// <summary>
        /// Create the property mapping
        /// </summary>
        /// <returns></returns>
        IPropertyMapping ToPropertyMapping();
    }
}