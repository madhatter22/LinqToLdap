using System.DirectoryServices.Protocols;
using System.Security.Principal;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class SecurityIdentifierPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        public SecurityIdentifierPropertyMapping(PropertyMappingArguments<T> arguments) : base(arguments)
        {
        }

        public override string FormatValueToFilter(object value)
        {
            if (value != null)
            {
                var identifier = value as SecurityIdentifier;
                var binary = new byte[identifier.BinaryLength];

                identifier.GetBinaryForm(binary, 0);

                return binary.ToStringOctet();
            }

            return null;
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
            if (value == null) return value;

            var identifier = value as SecurityIdentifier;
            var binary = new byte[identifier.BinaryLength];

            identifier.GetBinaryForm(binary, 0);

            return binary;
        }

        public override object FormatValueFromDirectory(DirectoryAttribute value, string dn)
        {
            if (value != null)
            {
                var bytes = value.GetValues(typeof(byte[]))[0] as byte[];
                if (bytes == null) ThrowMappingException(dn);

                return new SecurityIdentifier(bytes, 0);
            }

            AssertNullable(dn);

            return null;
        }
    }
}