using System;
using System.DirectoryServices.Protocols;
using System.Linq;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class ByteArrayArrayPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        public ByteArrayArrayPropertyMapping(PropertyMappingArguments<T> arguments)
            : base(arguments)
        {
        }

        public override string FormatValueToFilter(object value)
        {
            if (!(value is byte[]))
                throw new NotSupportedException("Byte[][] cannot be used in filters.");

            return (value as byte[]).ToStringOctet();
        }

        public override DirectoryAttributeModification GetDirectoryAttributeModification(object instance)
        {
            var modification = new DirectoryAttributeModification
            {
                Name = AttributeName,
                Operation = DirectoryAttributeOperation.Replace
            };
            var value = (byte[][])GetValueForDirectory(instance);

            if (value != null)
            {
                foreach (var b in value)
                {
                    modification.Add(b);
                }
            }

            return modification;
        }

        public override object GetValueForDirectory(object instance)
        {
            var value = GetValue(instance);
            return value;
        }

        public override object FormatValueFromDirectory(DirectoryAttribute value, string dn)
        {
            if (value != null)
            {
                byte[][] bytes = Array.ConvertAll(value.GetValues(typeof(byte[])), obj => (byte[])obj);

                return bytes;
            }

            AssertNullable(dn);

            return null;
        }

        public override bool IsEqual(object instance, object value, out DirectoryAttributeModification modification)
        {
            var propertyValue = GetValue(instance) as byte[][];
            var bytes = value as byte[][];
            if ((propertyValue == null || propertyValue.Length == 0) && (bytes == null || bytes.Length == 0))
            {
                modification = null;
                return true;
            }

            if (propertyValue == null || bytes == null || bytes.Length != propertyValue.Length)
            {
                modification = GetDirectoryAttributeModification(instance);
                return false;
            }

            if (bytes.Where((t, i) => !propertyValue[i].SequenceEqual(t)).Any())
            {
                modification = GetDirectoryAttributeModification(instance);
                return false;
            }

            modification = null;
            return true;
        }
    }
}