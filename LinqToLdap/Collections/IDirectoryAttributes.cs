using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.Protocols;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace LinqToLdap.Collections
{
    /// <summary>
    /// Allows you to get or set values for an entry.
    /// </summary>
    public interface IDirectoryAttributes : IEnumerable<KeyValuePair<string, object>>
    {
        /// <summary>
        /// Gets the Distinguished Name
        /// </summary>
        string DistinguishedName { get; }

        /// <summary>
        /// The <see cref="SearchResultEntry"/> returned by the directory.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if an attempt is made to access the Entry when this instance was not create from a <see cref="SearchResultEntry"/>.</exception>
        SearchResultEntry Entry { get; }

        /// <summary>
        /// All of the attributes pressent for the entry.
        /// </summary>
        ReadOnlyCollection<string> AttributeNames { get; }

        /// <summary>
        /// Gets the data for the attribute, if available, without regard for performance.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        object GetValue(string attribute);

        /// <summary>
        /// Gets the data for the attribute, if available, without regard for performance.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        object this[string attribute] { get; }

        /// <summary>
        /// Gets the <see cref="DirectoryAttribute"/> if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        DirectoryAttribute Get(string attribute);

        /// <summary>
        /// Gets the byte array value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        byte[] GetBytes(string attribute);

        /// <summary>
        /// Gets the <see cref="String"/> array value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        IEnumerable<string> GetStrings(string attribute);

        /// <summary>
        /// Gets the string value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        string GetString(string attribute);

        /// <summary>
        /// Gets the <see cref="Byte"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        /// <exception cref="LinqToLdap.Exceptions.MappingException">
        /// Thrown if the value cannot be
        /// </exception>
        byte? GetByte(string attribute);

        /// <summary>
        /// Gets the <see cref="Int32"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        int? GetInt(string attribute);

        /// <summary>
        /// Gets the <see cref="Int64"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        long? GetLong(string attribute);

        /// <summary>
        /// Gets the <see cref="Double"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        double? GetDouble(string attribute);

        /// <summary>
        /// Gets the <see cref="Decimal"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        decimal? GetDecimal(string attribute);

        /// <summary>
        /// Gets the <see cref="Int16"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        short? GetShort(string attribute);

        /// <summary>
        /// Gets the <see cref="float"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        float? GetFloat(string attribute);

        /// <summary>
        /// Gets the <see cref="Boolean"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        bool? GetBoolean(string attribute);

        /// <summary>
        /// Gets the <see cref="DateTime"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <param name="format">The format of the DateTime.  Use null for file time</param>
        /// <returns></returns>
        DateTime? GetDateTime(string attribute, string format);

        /// <summary>
        /// Gets the <see cref="DateTime"/> value for the specified attribute if available.
        /// Uses the default format yyyyMMddHHmmss.0Z.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        DateTime? GetDateTime(string attribute);

        /// <summary>
        /// Gets the list of <see cref="byte"/> array values for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        IEnumerable<byte[]> GetByteArrays(string attribute);

        /// <summary>
        /// Gets the <see cref="Guid"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        Guid? GetGuid(string attribute);

        /// <summary>
        /// Gets the <see cref="X509Certificate"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        X509Certificate GetX509Certificate(string attribute);

        /// <summary>
        /// Gets the <see cref="X509Certificate2"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        X509Certificate2 GetX509Certificate2(string attribute);

        /// <summary>
        /// Gets the list of <see cref="X509Certificate"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        IEnumerable<X509Certificate> GetX509Certificates(string attribute);

        /// <summary>
        /// Gets the list of <see cref="X509Certificate2"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
// ReSharper disable InconsistentNaming
        IEnumerable<X509Certificate2> GetX509Certificate2s(string attribute);

        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Gets the <see cref="SecurityIdentifier"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        SecurityIdentifier GetSecurityIdentifier(string attribute);

        /// <summary>
        /// Gets the <see cref="SecurityIdentifier"/> values for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        IEnumerable<SecurityIdentifier> GetSecurityIdentifiers(string attribute);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, byte[] value);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, IEnumerable<string> value);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, string value);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, byte value);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, short value);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, int value);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, long value);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, float value);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, double value);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, decimal value);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        /// <param name="format">The format of the <paramref name="value"/>.</param>
        IDirectoryAttributes Set(string attribute, DateTime value, string format = ExtensionMethods.LdapFormat);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, bool value);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, IEnumerable<X509Certificate> value);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, IEnumerable<X509Certificate2> value);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, X509Certificate value);

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        IDirectoryAttributes Set(string attribute, IEnumerable<byte[]> value);

        /// <summary>
        /// Sets the value of the attribute to "not set".
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        IDirectoryAttributes SetNull(string attribute);

        /// <summary>
        /// Returns any added, removed, or updated attributes and the value.
        /// </summary>
        ///<returns></returns>
        IEnumerable<DirectoryAttributeModification> GetChangedAttributes();

        /// <summary>
        /// Add a <see cref="DirectoryAttributeModification"/> for the entry.
        /// </summary>
        /// <param name="modification">The <see cref="DirectoryAttributeModification"/> for the entry.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="modification"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the modification is for "distinguishedname", "cn", "ou", or "entrydn".
        /// Also thrown <paramref name="modification"/> does not have a valid name.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown if a modification with the same name and operation has already been added.</exception>
        IDirectoryAttributes AddModification(DirectoryAttributeModification modification);
    }
}