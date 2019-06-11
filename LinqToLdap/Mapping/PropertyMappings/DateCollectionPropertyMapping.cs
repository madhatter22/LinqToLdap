using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.Protocols;
using System.Linq;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class DateCollectionPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        private readonly bool _isFileTimeFormat;
        private readonly string _dateFormat;
        private readonly bool _isNullable;

        public DateCollectionPropertyMapping(PropertyMappingArguments<T> arguments, string dateFormat)
            : base(arguments)
        {
            _isFileTimeFormat = dateFormat == null;

            _dateFormat = dateFormat;

            var elementType = PropertyType.GetGenericArguments()[0];
            _isNullable = elementType == typeof(DateTime?);
        }

        public override object FormatValueFromDirectory(DirectoryAttribute value, string dn)
        {
            if (value != null)
            {
                try
                {
                    var strings = Array.ConvertAll(value.GetValues(typeof(string)), obj => obj.ToString());

                    if (_isNullable)
                    {
                        var dates = new List<DateTime?>(strings.Length);
                        dates.AddRange(strings.Select(str => new DateTime?(_isFileTimeFormat
                                                    ? DateTime.FromFileTime(long.Parse(str))
                                                    : str.FormatLdapDateTime(_dateFormat))));

                        return new Collection<DateTime?>(dates);
                    }
                    else
                    {
                        var dates = new List<DateTime>(strings.Length);
                        dates.AddRange(strings.Select(str => _isFileTimeFormat
                                                    ? DateTime.FromFileTime(long.Parse(str))
                                                    : str.FormatLdapDateTime(_dateFormat)));

                        return new Collection<DateTime>(dates);
                    }
                }
                catch (Exception ex)
                {
                    ThrowMappingException(value, dn, ex);
                }
            }

            if (DirectoryValueMappings != null && DirectoryValueMappings.ContainsKey(string.Empty))
            {
                return DirectoryValueMappings[string.Empty];
            }

            AssertNullable(dn);

            return null;
        }

        public override string FormatValueToFilter(object value)
        {
            if (value.GetType() != typeof(DateTime))
            {
                if (value.GetType() != typeof(DateTime?))
                {
                    throw new NotSupportedException("Arrays cannot be used in filters.");
                }
            }

            var date = (DateTime)value;

            return _isFileTimeFormat
                ? date.ToFileTime().ToString()
                : date.FormatLdapDateTime(_dateFormat);
        }

        public override DirectoryAttributeModification GetDirectoryAttributeModification(object instance)
        {
            var modification = new DirectoryAttributeModification
            {
                Name = AttributeName,
                Operation = DirectoryAttributeOperation.Replace
            };
            var value = (string[])GetValueForDirectory(instance);

            if (value != null)
            {
                foreach (var s in value)
                {
                    modification.Add(s);
                }
            }

            return modification;
        }

        public override object GetValueForDirectory(object instance)
        {
            if (_isNullable)
            {
                var value = (ICollection<DateTime?>)GetValue(instance);
                if (value == null) return null;
                return _isFileTimeFormat
                    ? value.Select(d => d.HasValue ? d.Value.ToFileTime().ToString() : null)
                        .Where(d => d != null)
                        .ToArray()
                    : value.Select(d => d.HasValue ? d.Value.FormatLdapDateTime(_dateFormat) : null)
                        .Where(d => d != null)
                        .ToArray();
            }
            else
            {
                var value = (ICollection<DateTime>)GetValue(instance);
                if (value == null) return null;
                return _isFileTimeFormat
                    ? value.Select(d => d.ToFileTime().ToString()).ToArray()
                    : value.Select(d => d.FormatLdapDateTime(_dateFormat)).ToArray();
            }
        }

        public override bool IsEqual(object instance, object value, out DirectoryAttributeModification modification)
        {
            if (_isNullable)
            {
                var propertyValue = GetValue(instance) as ICollection<DateTime?>;
                var dates = value as ICollection<DateTime?>;
                if ((propertyValue == null || propertyValue.Count == 0) && (dates == null || dates.Count == 0))
                {
                    modification = null;
                    return true;
                }

                if (propertyValue == null || dates == null || dates.Count != propertyValue.Count)
                {
                    modification = GetDirectoryAttributeModification(instance);
                    return false;
                }

                if (!dates.SequenceEqual(propertyValue))
                {
                    modification = GetDirectoryAttributeModification(instance);
                    return false;
                }

                modification = null;
                return true;
            }
            else
            {
                var propertyValue = GetValue(instance) as ICollection<DateTime>;
                var dates = value as ICollection<DateTime>;
                if ((propertyValue == null || propertyValue.Count == 0) && (dates == null || dates.Count == 0))
                {
                    modification = null;
                    return true;
                }

                if (propertyValue == null || dates == null || dates.Count != propertyValue.Count)
                {
                    modification = GetDirectoryAttributeModification(instance);
                    return false;
                }

                if (!dates.SequenceEqual(propertyValue))
                {
                    modification = GetDirectoryAttributeModification(instance);
                    return false;
                }

                modification = null;
                return true;
            }
        }
    }
}