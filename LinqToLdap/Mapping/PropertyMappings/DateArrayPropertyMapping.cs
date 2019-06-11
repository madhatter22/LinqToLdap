using System;
using System.DirectoryServices.Protocols;
using System.Linq;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class DateArrayPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        private readonly bool _isFileTimeFormat;
        private readonly string _dateFormat;
        private readonly bool _isNullable;

        public DateArrayPropertyMapping(PropertyMappingArguments<T> arguments, string dateFormat)
            : base(arguments)
        {
            _isFileTimeFormat = dateFormat == null;

            _dateFormat = dateFormat;

            var elementType = PropertyType.GetElementType();
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
                        var dates = new DateTime?[strings.Length];

                        for (int i = 0; i < strings.Length; i++)
                        {
                            var str = strings[i];
                            var dateTime = _isFileTimeFormat
                                                    ? DateTime.FromFileTime(long.Parse(str))
                                                    : str.FormatLdapDateTime(_dateFormat);

                            dates[i] = dateTime;
                        }

                        return dates;
                    }
                    else
                    {
                        var dates = new DateTime[strings.Length];

                        for (int i = 0; i < strings.Length; i++)
                        {
                            var str = strings[i];
                            var dateTime = _isFileTimeFormat
                                                    ? DateTime.FromFileTime(long.Parse(str))
                                                    : str.FormatLdapDateTime(_dateFormat);

                            dates[i] = dateTime;
                        }

                        return dates;
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
                var value = (DateTime?[])GetValue(instance);
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
                var value = (DateTime[])GetValue(instance);
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
                var propertyValue = GetValue(instance) as DateTime?[];
                var dates = value as DateTime?[];
                if ((propertyValue == null || propertyValue.Length == 0) && (dates == null || dates.Length == 0))
                {
                    modification = null;
                    return true;
                }

                if (propertyValue == null || dates == null || dates.Length != propertyValue.Length)
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
                var propertyValue = GetValue(instance) as DateTime[];
                var dates = value as DateTime[];
                if ((propertyValue == null || propertyValue.Length == 0) && (dates == null || dates.Length == 0))
                {
                    modification = null;
                    return true;
                }

                if (propertyValue == null || dates == null || dates.Length != propertyValue.Length)
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