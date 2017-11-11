using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Principal;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class SecurityIdentifierArrayPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        public SecurityIdentifierArrayPropertyMapping(PropertyMappingArguments<T> arguments)
            : base(arguments)
        {
        }

        public override string FormatValueToFilter(object value)
        {
            if (!(value is SecurityIdentifier))
                throw new NotSupportedException("SecurityIdentifier[] cannot be used in filters.");

            var identifier = value as SecurityIdentifier;
            var binary = new byte[identifier.BinaryLength];

            identifier.GetBinaryForm(binary, 0);
            
            return binary.ToStringOctet();
        }

        public override DirectoryAttributeModification GetDirectoryAttributeModification(object instance)
        {
            var modification = new DirectoryAttributeModification
            {
                Name = AttributeName,
                Operation = DirectoryAttributeOperation.Replace
            };
            var value = (IEnumerable<SecurityIdentifier>)GetValueForDirectory(instance);

            if (value != null)
            {
                foreach (var c in value)
                {
                    var binary = new byte[c.BinaryLength];

                    c.GetBinaryForm(binary, 0);
                    modification.Add(binary);
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
                SecurityIdentifier[] identifiers = Array.ConvertAll(value.GetValues(typeof(byte[])), obj => new SecurityIdentifier((byte[])obj, 0));

                return identifiers;
            }

            AssertNullable(dn);

            return null;
        }

        public override bool IsEqual(object instance, object value, out DirectoryAttributeModification modification)
        {
            var propertyValue = GetValue(instance) as SecurityIdentifier[];
            var identifiers = value as SecurityIdentifier[];
            if ((propertyValue == null || propertyValue.Length == 0) && (identifiers == null || identifiers.Length == 0))
            {
                modification = null;
                return true;
            }

            if (propertyValue == null || identifiers == null || identifiers.Length != propertyValue.Length)
            {
                modification = GetDirectoryAttributeModification(instance);
                return false;
            }

            if (!identifiers.SequenceEqual(propertyValue))
            {
                modification = GetDirectoryAttributeModification(instance);
                return false;
            }

            modification = null;
            return true;
        }
    }
}
