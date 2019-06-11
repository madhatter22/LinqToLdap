using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace LinqToLdap.Collections
{
    /// <summary>
    /// Allows you to get or set values for an entry.
    /// </summary>
    public sealed class DirectoryAttributes : IDirectoryAttributes
    {
        private static readonly Dictionary<string, string> ByteProperties =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    {"Ibm-entryuuid", "Ibm-entryuuid"},
                    {"Nsuniqueid", "Nsuniqueid"}
                };

        private readonly string _distinguishedName;
        private readonly SearchResultEntry _entry;
        private List<DirectoryAttributeModification> _changedAttributes;
        private ReadOnlyCollection<string> _attributeNames;

        /// <summary>
        /// Instantiates this class with a reference to <paramref name="entry"/>.
        /// </summary>
        /// <param name="entry">The <see cref="SearchResultEntry"/> to wrap.</param>
        internal DirectoryAttributes(SearchResultEntry entry)
        {
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));
        }

        /// <summary>
        /// Instantiates this class without a reference to a <see cref="SearchResultEntry"/>.
        /// It can be used to set attributes for the directory.
        /// </summary>
        /// <param name="distinguishedName">
        /// The distinguished name for the entry.
        /// </param>
        /// <param name="attributes">
        /// The attributes for the entry that will be treated as changed attributes.  Attributes named distinguishedname and entrydn are ignored.
        /// </param>
        public DirectoryAttributes(string distinguishedName = null, IEnumerable<KeyValuePair<string, object>> attributes = null)
        {
            _distinguishedName = distinguishedName;

            if (attributes == null) return;

            foreach (var keyValuePair in attributes)
            {
                var name = keyValuePair.Key;
                if (name.Equals("distinguishedname", StringComparison.OrdinalIgnoreCase) || name.Equals("entrydn", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                AddModification(keyValuePair.Value.ToDirectoryModification(name, DirectoryAttributeOperation.Replace));
            }
        }

        /// <summary>
        /// All of the attributes present for the entry.
        /// </summary>
        public ReadOnlyCollection<string> AttributeNames => _attributeNames ??
                                                            (_entry != null
                                                                // ReSharper disable AssignNullToNotNullAttribute
                                                                ? _attributeNames = new ReadOnlyCollection<string>(_entry.Attributes.AttributeNames.Cast<string>().ToList())
                                                                // ReSharper restore AssignNullToNotNullAttribute
                                                                : new ReadOnlyCollection<string>(new List<string>()));

        /// <summary>
        /// Add a <see cref="DirectoryAttributeModification"/> for the entry.
        /// </summary>
        /// <param name="modification">The <see cref="DirectoryAttributeModification"/> for the entry.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="modification"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the modification is for "distinguishedname" or "entrydn".
        /// Also thrown <paramref name="modification"/> does not have a valid name.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown if a modification with the same name and operation has already been added.</exception>
        public IDirectoryAttributes AddModification(DirectoryAttributeModification modification)
        {
            if (modification == null) throw new ArgumentNullException(nameof(modification));
            string name = modification.Name;
            if (name.IsNullOrEmpty()) throw new ArgumentException("The modification must have a name.");

            if (name.Equals("distinguishedname", StringComparison.OrdinalIgnoreCase) || name.Equals("entrydn", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Cannot change the distinguished name. Please use MoveEntry or RenameEntry.");

            if (_changedAttributes == null)
            {
                _changedAttributes = new List<DirectoryAttributeModification>();
            }

            var changed = _changedAttributes
                .FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && c.Operation == modification.Operation);

            if (changed == null)
            {
                _changedAttributes.Add(modification);
            }
            else
            {
                throw new InvalidOperationException(
                    $"A modification for {modification.Name} with operation {modification.Operation} has already been added.");
            }

            return this;
        }

        /// <summary>
        /// Gets the Distinguished Name
        /// </summary>
        public string DistinguishedName => _entry != null
            ? _entry.DistinguishedName
            : (_distinguishedName.IsNullOrEmpty() ? string.Empty : _distinguishedName);

        /// <summary>
        /// The <see cref="SearchResultEntry"/> returned by the directory.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if an attempt is made to access the Entry when this instance was not create from a <see cref="SearchResultEntry"/>.</exception>
        public SearchResultEntry Entry
        {
            get
            {
                if (_entry == null) throw new InvalidOperationException("Entry is not available for this instance.");
                return _entry;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the attributes.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            if (_entry == null) yield break;

            foreach (string attributeName in AttributeNames)
            {
                yield return new KeyValuePair<string, object>(attributeName, GetValue(attributeName));
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the attributes.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the <see cref="DirectoryAttribute"/> if available.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public DirectoryAttribute Get(string attribute)
        {
            if (attribute.IsNullOrEmpty()) throw new ArgumentNullException(nameof(attribute));
            return _entry?.Attributes[attribute];
        }

        /// <summary>
        /// Gets the data for the attribute, if available, without regard for performance.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public object GetValue(string attribute)
        {
            var directoryAttribute = Get(attribute);

            if (directoryAttribute != null)
            {
                if (directoryAttribute.Count > 1)
                {
                    var type = directoryAttribute[0].GetType();

                    if (type == typeof(string))
                    {
                        return GetStrings(attribute).ToList();
                    }
                    if (type == typeof(byte[]))
                    {
                        return GetByteArrays(attribute).ToList();
                    }
                }

                if (attribute.EndsWith("guid", StringComparison.OrdinalIgnoreCase) || ByteProperties.ContainsKey(attribute))
                {
                    var bytes = GetBytes(attribute);

                    if (bytes != null && bytes.Length == 16)
                    {
                        return new Guid(bytes);
                    }

                    return bytes;
                }
                if ("objectsid".Equals(attribute, StringComparison.OrdinalIgnoreCase))
                {
                    return GetSecurityIdentifier(attribute);
                }
                if ("photo".Equals(attribute, StringComparison.OrdinalIgnoreCase))
                {
                    return GetBytes(attribute);
                }

                return directoryAttribute.Count == 0 ? null : directoryAttribute[0];
            }

            return null;
        }

        /// <summary>
        /// Gets the data for the attribute, if available, without regard for performance.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public object this[string attribute] => GetValue(attribute);

        /// <summary>
        /// Gets the byte array value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public byte[] GetBytes(string attribute)
        {
            byte[] value = null;
            var type = typeof(byte[]);

            var directoryAttribute = Get(attribute);

            if (directoryAttribute != null)
            {
                object rawValue = null;
                try
                {
                    var valueArray = directoryAttribute.GetValues(type);
                    rawValue = valueArray.Length == 0
                                   ? null
                                   : valueArray[0];
                    value = rawValue == null
                        ? default
                        : (byte[])rawValue;
                }
                catch (Exception ex)
                {
                    ThrowFormatException(ex, attribute, rawValue, type);
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the <see cref="X509Certificate2"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public X509Certificate2 GetX509Certificate2(string attribute)
        {
            X509Certificate2 value = null;
            var type = typeof(byte[]);

            var directoryAttribute = Get(attribute);

            if (directoryAttribute != null)
            {
                object rawValue = null;
                try
                {
                    var valueArray = directoryAttribute.GetValues(type);
                    rawValue = valueArray.Length == 0
                                   ? null
                                   : valueArray[0];
                    value = rawValue == null
                        ? default
                        : new X509Certificate2((byte[])rawValue);
                }
                catch (Exception ex)
                {
                    ThrowFormatException(ex, attribute, rawValue, type);
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the <see cref="X509Certificate"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public X509Certificate GetX509Certificate(string attribute)
        {
            return GetX509Certificate2(attribute);
        }

        /// <summary>
        /// Gets the <see cref="String"/> array value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public IEnumerable<string> GetStrings(string attribute)
        {
            var type = typeof(string);

            var directoryAttribute = Get(attribute);

            if (directoryAttribute != null)
            {
                var valueArray = directoryAttribute.GetValues(type);
                object[] rawValue = valueArray.Length == 0
                                        ? null
                                        : valueArray;
                if (rawValue != null)
                {
                    foreach (string o in rawValue)
                    {
                        yield return o;
                    }
                }
            }

            yield break;
        }

        /// <summary>
        /// Gets the string value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public string GetString(string attribute)
        {
            string value = null;
            var type = typeof(string);

            var directoryAttribute = Get(attribute);

            if (directoryAttribute != null)
            {
                object rawValue = null;
                try
                {
                    var valueArray = directoryAttribute.GetValues(type);
                    rawValue = valueArray.Length == 0
                                   ? null
                                   : valueArray[0];
                    value = rawValue == null
                        ? default
                        : (string)rawValue;
                }
                catch (Exception ex)
                {
                    ThrowFormatException(ex, attribute, rawValue, type);
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the <see cref="Int32"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public int? GetInt(string attribute)
        {
            return GetValueType<int>(attribute);
        }

        /// <summary>
        /// Gets the <see cref="Int64"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public long? GetLong(string attribute)
        {
            return GetValueType<long>(attribute);
        }

        /// <summary>
        /// Gets the <see cref="Double"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public double? GetDouble(string attribute)
        {
            return GetValueType<double>(attribute);
        }

        /// <summary>
        /// Gets the <see cref="Decimal"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public decimal? GetDecimal(string attribute)
        {
            return GetValueType<decimal>(attribute);
        }

        /// <summary>
        /// Gets the <see cref="Int16"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public short? GetShort(string attribute)
        {
            return GetValueType<short>(attribute);
        }

        /// <summary>
        /// Gets the <see cref="Byte"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        /// <exception cref="LinqToLdap.Exceptions.MappingException">
        /// Thrown if the value cannot be
        /// </exception>
        public byte? GetByte(string attribute)
        {
            return GetValueType<byte>(attribute);
        }

        /// <summary>
        /// Gets the <see cref="float"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public float? GetFloat(string attribute)
        {
            return GetValueType<float>(attribute);
        }

        /// <summary>
        /// Gets the <see cref="Boolean"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public bool? GetBoolean(string attribute)
        {
            return GetValueType<bool>(attribute);
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <param name="format">The format of the DateTime.  Use null for file time</param>
        /// <returns></returns>
        public DateTime? GetDateTime(string attribute, string format)
        {
            DateTime? value = null;
            var type = typeof(DateTime);

            var directoryAttribute = Get(attribute);

            if (directoryAttribute != null)
            {
                object rawValue = null;
                try
                {
                    var valueArray = directoryAttribute.GetValues(typeof(string));
                    rawValue = valueArray.Length == 0
                                   ? null
                                   : valueArray[0];
                    value = rawValue == null || Equals("9223372036854775807", rawValue)
                        ? default(DateTime?)
                        : (format == null ? DateTime.FromFileTime(long.Parse(rawValue.ToString())) : rawValue.FormatLdapDateTime(format));
                }
                catch (Exception ex)
                {
                    ThrowFormatException(ex, attribute, rawValue, type);
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> value for the specified attribute if available.
        /// Uses the default format yyyyMMddHHmmss.0Z.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public DateTime? GetDateTime(string attribute)
        {
            return GetDateTime(attribute, ExtensionMethods.LdapFormat);
        }

        /// <summary>
        /// Gets the list of <see cref="byte"/> array values for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public IEnumerable<byte[]> GetByteArrays(string attribute)
        {
            var type = typeof(byte[]);

            var directoryAttribute = Get(attribute);

            if (directoryAttribute != null)
            {
                foreach (byte[] value in directoryAttribute.GetValues(type))
                {
                    yield return value;
                }
            }

            yield break;
        }

        /// <summary>
        /// Gets the list of <see cref="X509Certificate2"/> values for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public IEnumerable<X509Certificate2> GetX509Certificate2s(string attribute)
        {
            X509Certificate2[] value = null;
            var type = typeof(byte[]);

            var directoryAttribute = Get(attribute);

            if (directoryAttribute != null)
            {
                object rawValue = null;
                try
                {
                    var valueArray = directoryAttribute.GetValues(type);
                    rawValue = valueArray;
                    value = new X509Certificate2[valueArray.Length];
                    for (int i = 0; i < valueArray.Length; i++)
                    {
                        value[i] = new X509Certificate2((byte[])valueArray[i]);
                    }
                }
                catch (Exception ex)
                {
#if NET35
                    ThrowFormatException(ex, attribute, value == null ? "" : string.Join(",", new[] { rawValue.ToString() }), type);
#else
                    ThrowFormatException(ex, attribute, value == null ? "" : string.Join(",", rawValue), type);
#endif
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the list of <see cref="X509Certificate"/> values for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public IEnumerable<X509Certificate> GetX509Certificates(string attribute)
        {
#if NET35
            var certs = GetX509Certificate2s(attribute);
            return certs != null ? certs.Cast<X509Certificate>() : null;
#else
            return GetX509Certificate2s(attribute);
#endif
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public Guid? GetGuid(string attribute)
        {
            byte[] value = null;
            try
            {
                value = GetBytes(attribute);
                if (value != null)
                {
                    return new Guid(value);
                }
            }
            catch (Exception ex)
            {
#if NET35
                ThrowFormatException(ex, attribute, value == null ? "" : string.Join(",", value.Select(b => b.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray()), typeof(Guid));
#else
                ThrowFormatException(ex, attribute, value == null ? "" : string.Join(",", value), typeof(Guid));
#endif
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="SecurityIdentifier"/> value for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public SecurityIdentifier GetSecurityIdentifier(string attribute)
        {
            byte[] value = null;
            try
            {
                value = GetBytes(attribute);
                if (value != null)
                {
                    return new SecurityIdentifier(value, 0);
                }
            }
            catch (Exception ex)
            {
#if NET35
                ThrowFormatException(ex, attribute, value == null ? "" : string.Join(",", value.Select(b => b.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray()), typeof(SecurityIdentifier));
#else
                ThrowFormatException(ex, attribute, value == null ? "" : string.Join(",", value), typeof(SecurityIdentifier));
#endif
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="SecurityIdentifier"/> values for the specified attribute if available.
        /// </summary>
        /// <param name="attribute">The name of the attribute</param>
        /// <returns></returns>
        public IEnumerable<SecurityIdentifier> GetSecurityIdentifiers(string attribute)
        {
            IEnumerable<byte[]> value = null;
            try
            {
                value = GetByteArrays(attribute);

                return value
                    .Select(x => new SecurityIdentifier(x, 0))
                    .ToArray();
            }
            catch (Exception ex)
            {
#if NET35
                ThrowFormatException(ex, attribute, value == null ? "" : string.Join(",", value.Select(b => b.ToString()).ToArray()), typeof(IEnumerable<SecurityIdentifier>));
#else
                ThrowFormatException(ex, attribute, value == null ? "" : string.Join(",", value), typeof(IEnumerable<SecurityIdentifier>));
#endif
            }

            return null;
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, byte[] value)
        {
            return AddModification(value.ToDirectoryModification(attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, X509Certificate value)
        {
            return AddModification(value.ToDirectoryModification(attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, IEnumerable<X509Certificate> value)
        {
            return AddModification(value.ToDirectoryModification(attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, IEnumerable<X509Certificate2> value)
        {
            return AddModification(value.ToDirectoryModification(attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, IEnumerable<string> value)
        {
            return AddModification(value.ToDirectoryModification(attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, string value)
        {
            return AddModification(value.ToDirectoryModification(attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, byte value)
        {
            return AddModification(value.ToDirectoryModification(attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, short value)
        {
            return AddModification(value.ToDirectoryModification(attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, int value)
        {
            return AddModification(value.ToDirectoryModification(attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, long value)
        {
            return AddModification(value.ToDirectoryModification(attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, float value)
        {
            return AddModification(value.ToDirectoryModification(attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, double value)
        {
            return AddModification(value.ToDirectoryModification(attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, decimal value)
        {
            return AddModification(value.ToDirectoryModification(attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        /// <param name="format">The format of the <paramref name="value"/>.</param>
        public IDirectoryAttributes Set(string attribute, DateTime value, string format = ExtensionMethods.LdapFormat)
        {
            var modification = new DirectoryAttributeModification { Name = attribute, Operation = DirectoryAttributeOperation.Replace };

            modification.Add(format == null ? value.ToFileTime().ToString() : value.FormatLdapDateTime(format));
            return AddModification(modification);
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, bool value)
        {
            var modification = new DirectoryAttributeModification { Name = attribute, Operation = DirectoryAttributeOperation.Replace };

            modification.Add(value ? "TRUE" : "FALSE");
            return AddModification(modification);
        }

        /// <summary>
        /// Sets the value of the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <param name="value">The value.</param>
        public IDirectoryAttributes Set(string attribute, IEnumerable<byte[]> value)
        {
            return AddModification(value.ToDirectoryModification(attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Sets the value of the attribute to "not set".
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        public IDirectoryAttributes SetNull(string attribute)
        {
            return AddModification(ExtensionMethods.ToDirectoryModification(null, attribute, DirectoryAttributeOperation.Replace));
        }

        /// <summary>
        /// Returns any added, removed, or updated attributes and the value.
        /// </summary>
        ///<returns></returns>
        public IEnumerable<DirectoryAttributeModification> GetChangedAttributes()
        {
            return _changedAttributes ?? new List<DirectoryAttributeModification>();
        }

        private T? GetValueType<T>(string attribute) where T : struct
        {
            T? value = null;
            var type = typeof(T);

            var directoryAttribute = Get(attribute);

            if (directoryAttribute != null)
            {
                object rawValue = null;
                try
                {
                    var valueArray = directoryAttribute.GetValues(typeof(string));
                    rawValue = valueArray.Length == 0
                                   ? null
                                   : valueArray[0];
                    value = rawValue == null
                        ? default(T?)
                        : (T)Convert.ChangeType(rawValue, type);
                }
                catch (Exception ex)
                {
                    ThrowFormatException(ex, attribute, rawValue, type);
                }
            }

            return value;
        }

        private void ThrowFormatException(Exception ex, string attribute, object value, Type conversionType)
        {
            var message =
                $"Value '{value}' for attribute '{attribute}' caused {ex.GetType().Name} when trying to convert to '{conversionType.Name}' for {DistinguishedName}";

            ThrowFormatException(message, ex);
        }

        private static void ThrowFormatException(string message, Exception innerException)
        {
            throw new FormatException(message, innerException);
        }
    }
}