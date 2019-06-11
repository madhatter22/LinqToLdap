using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class StringCollectionPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        public StringCollectionPropertyMapping(PropertyMappingArguments<T> arguments)
            : base(arguments)
        {
        }

        public override string FormatValueToFilter(object value)
        {
            if (!(value is string))
                throw new NotSupportedException("String collections cannot be used in filters.");

            return value.ToString().CleanFilterValue();
        }

        public override DirectoryAttributeModification GetDirectoryAttributeModification(object instance)
        {
            var modification = new DirectoryAttributeModification
            {
                Name = AttributeName,
                Operation = DirectoryAttributeOperation.Replace
            };
            var value = (IEnumerable<string>)GetValueForDirectory(instance);

            if (value != null)
            {
                foreach (var str in value)
                {
                    modification.Add(str);
                }
            }

            return modification;
        }

        public override object GetValueForDirectory(object instance)
        {
            return GetValue(instance);
        }

        public override object FormatValueFromDirectory(DirectoryAttribute value, string dn)
        {
            if (value != null)
            {
                var list = value.GetValues(typeof(string))
                    .Select(o => (string)o);
                return

                    new System.Collections.ObjectModel.Collection<string>(list.ToList());
            }

            AssertNullable(dn);

            return null;
        }

        public override bool IsEqual(object instance, object value, out DirectoryAttributeModification modification)
        {
            var propertyValue = GetValue(instance) as ICollection<string>;
            var strings = value as ICollection<string>;
            if ((propertyValue == null || propertyValue.Count == 0) && (strings == null || strings.Count == 0))
            {
                modification = null;
                return true;
            }

            if (propertyValue == null || strings == null || strings.Count != propertyValue.Count)
            {
                modification = GetDirectoryAttributeModification(instance);
                return false;
            }

            if (!strings.SequenceEqual(propertyValue))
            {
                modification = GetDirectoryAttributeModification(instance);
                return false;
            }

            modification = null;
            return true;
        }
    }
}