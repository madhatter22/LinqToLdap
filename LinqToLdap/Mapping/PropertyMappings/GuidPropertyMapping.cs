using System;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class GuidPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        public GuidPropertyMapping(PropertyMappingArguments<T> arguments) : base(arguments)
        {
        }

        public override string FormatValueToFilter(object value)
        {
            return value != null
                       ? ((Guid)value).ToStringOctet()
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

            return value == null ? null : ((Guid)value).ToByteArray();
        }

        public override object FormatValueFromDirectory(DirectoryAttribute value, string dn)
        {
            if (value != null)
            {
                var bytes = value.GetValues(typeof(byte[]))[0] as byte[];
                if (bytes == null) ThrowMappingException(dn);

                try
                {
                    return new Guid(bytes);
                }
                catch (Exception ex)
                {
                    ThrowMappingException(value, dn, ex);
                }
            }

            AssertNullable(dn);

            return null;
        }
    }
}