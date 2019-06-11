using System.DirectoryServices.Protocols;
using System.Linq;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class ByteArrayPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        public ByteArrayPropertyMapping(PropertyMappingArguments<T> arguments) : base(arguments)
        {
        }

        public override string FormatValueToFilter(object value)
        {
            return value != null
                       ? (value as byte[]).ToStringOctet()
                       : null;
        }

        public override DirectoryAttributeModification GetDirectoryAttributeModification(object instance)
        {
            var modification = new DirectoryAttributeModification
            {
                Name = AttributeName,
                Operation = DirectoryAttributeOperation.Replace
            };
            var value = (byte[])GetValueForDirectory(instance);

            if (value != null)
            {
                modification.Add(value);
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
                var bytes = value.GetValues(typeof(byte[]))[0] as byte[];
                if (bytes == null) ThrowMappingException(dn);

                return bytes;
            }

            AssertNullable(dn);

            return null;
        }

        public override bool IsEqual(object instance, object value, out DirectoryAttributeModification modification)
        {
            var propertyValue = GetValue(instance) as byte[];
            var bytes = value as byte[];
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

            if (!bytes.SequenceEqual(propertyValue))
            {
                modification = GetDirectoryAttributeModification(instance);
                return false;
            }

            modification = null;
            return true;
        }
    }
}