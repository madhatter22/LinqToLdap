using LinqToLdap.Exceptions;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class CustomPropertyMapping<T, TProperty> : PropertyMappingGeneric<T> where T : class
    {
        private readonly Func<DirectoryAttribute, TProperty> _convertFrom;
        private readonly Func<TProperty, object> _convertTo;
        private readonly Func<TProperty, string> _convertToFilter;
        private readonly Func<TProperty, TProperty, bool> _isEqual;

        public CustomPropertyMapping(PropertyMappingArguments<T> arguments, Func<DirectoryAttribute, TProperty> fromConverter, Func<TProperty, object> toConverter, Func<TProperty, string> toFilterConverter, Func<TProperty, TProperty, bool> isEqual)
            : base(arguments)
        {
            _convertFrom = fromConverter;
            _convertTo = toConverter;
            _convertToFilter = toFilterConverter;
            _isEqual = isEqual;
        }

        public override object FormatValueFromDirectory(DirectoryAttribute value, string dn)
        {
            if (_convertFrom == null)
                throw new MappingException(
                    string.Format("No from directory converter defined for mapped property '{0}' on '{1}'", PropertyName,
                                  typeof(T).FullName));

            try
            {
                return _convertFrom(value);
            }
            catch (Exception ex)
            {
                ThrowMappingException(value, dn, ex);
            }
            return null;
        }

        public override string FormatValueToFilter(object value)
        {
            if (_convertFrom == null)
                throw new MappingException(
                    string.Format("No to-filter converter defined for mapped property '{0}' on '{1}'", PropertyName,
                                  typeof(T).FullName));

            return _convertToFilter((TProperty)value);
        }

        public override DirectoryAttributeModification GetDirectoryAttributeModification(object instance)
        {
            var modification = new DirectoryAttributeModification
            {
                Name = AttributeName,
                Operation = DirectoryAttributeOperation.Replace
            };
            var value = GetValueForDirectory(instance);

            if (value != null)
            {
                if (value is byte[])
                {
                    modification.Add(value as byte[]);
                }
                else if (value is string)
                {
                    modification.Add(value as string);
                }
                else if (value is IEnumerable<byte[]>)
                {
                    foreach (var o in value as IEnumerable<byte[]>)
                    {
                        modification.Add(o);
                    }
                }
                else if (value is IEnumerable<string>)
                {
                    foreach (var o in value as IEnumerable<string>)
                    {
                        modification.Add(o);
                    }
                }
                else
                {
                    throw new MappingException(string.Format("Unsupported value '{0}' for property '{1}' on '{2}'", value, PropertyName, typeof(T).FullName));
                }
            }

            return modification;
        }

        public override object GetValueForDirectory(object instance)
        {
            var value = GetValue(instance);
            return _convertTo((TProperty)value);
        }

        public override bool IsEqual(object instance, object value, out DirectoryAttributeModification modification)
        {
            var propertyValue = GetValue(instance);

            if (value == null && propertyValue == null)
            {
                modification = null;
                return true;
            }

            if (_isEqual == null) return base.IsEqual(instance, value, out modification);

            var isEqual = _isEqual((TProperty)propertyValue, (TProperty)value);
            modification = !isEqual ? GetDirectoryAttributeModification(instance) : null;
            return isEqual;
        }
    }
}