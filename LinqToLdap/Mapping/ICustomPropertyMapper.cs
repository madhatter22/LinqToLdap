using System;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.Mapping
{
    /// <summary>
    /// Interface for custom mapping properties
    /// </summary>
    public interface ICustomPropertyMapper<T, TProperty>
    {
        /// <summary>
        /// Specify an attribute name for a mapped property.
        /// </summary>
        /// <param name="attributeName">Attribute name in the directory</param>
        /// <returns></returns>
        ICustomPropertyMapper<T, TProperty> Named(string attributeName);

        /// <summary>
        /// Specify the function for converting the property value to a valid LDAP filter value.
        /// </summary>
        /// <param name="converter">The function</param>
        /// <returns></returns>
        ICustomPropertyMapper<T, TProperty> ConvertToFilterUsing(Func<TProperty, string> converter);

        /// <summary>
        /// Specify the function for converting the <see cref="DirectoryAttribute"/> to a valid value for the mapped property on a mapped object.
        /// </summary>
        /// <param name="converter">THe function</param>
        /// <returns></returns>
        ICustomPropertyMapper<T, TProperty> ConvertFromDirectoryUsing(Func<DirectoryAttribute, TProperty> converter);

        /// <summary>
        /// Specify the function for converting a property value to a valid directory value.
        /// </summary>
        /// <param name="converter">The function</param>
        /// <returns></returns>
        ICustomPropertyMapper<T, TProperty> ConvertToDirectoryUsing(Func<TProperty, byte[]> converter);

        /// <summary>
        /// Specify the function for converting a property value to a valid directory value.
        /// </summary>
        /// <param name="converter">The function</param>
        /// <returns></returns>
        ICustomPropertyMapper<T, TProperty> ConvertToDirectoryUsing(Func<TProperty, byte[][]> converter);

        /// <summary>
        /// Specify the function for converting a property value to a valid directory value.
        /// </summary>
        /// <param name="converter">The function</param>
        /// <returns></returns>
        ICustomPropertyMapper<T, TProperty> ConvertToDirectoryUsing(Func<TProperty, string> converter);

        /// <summary>
        /// Specify the function for converting a property value to a valid directory value.
        /// </summary>
        /// <param name="converter">The function</param>
        /// <returns></returns>
        ICustomPropertyMapper<T, TProperty> ConvertToDirectoryUsing(Func<TProperty, string[]> converter);

        /// <summary>
        /// The equal comparison to use when comparing the original value to the current value.  For use with change tracking updatable objects.  Will default to standard equals comparison if null.
        /// </summary>
        /// <param name="comparer">The function</param>
        /// <returns></returns>
        ICustomPropertyMapper<T, TProperty> CompareChangesUsing(Func<TProperty, TProperty, bool> comparer);

        /// <summary>
        /// The read only configuration .
        /// </summary>
        /// <returns></returns>
        ICustomPropertyMapper<T, TProperty> ReadOnly(ReadOnly readOnly = Mapping.ReadOnly.Always);
    }
}