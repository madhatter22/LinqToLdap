using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Principal;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class SecurityIdentifierCollectionPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        public SecurityIdentifierCollectionPropertyMapping(PropertyMappingArguments<T> arguments)
            : base(arguments)
        {
        }

        public override string FormatValueToFilter(object value)
        {
            if (!(value is SecurityIdentifier))
                throw new NotSupportedException("SecurityIdentifier collections cannot be used in filters.");

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
                var list = value.GetValues(typeof(byte[]))
                    .Select(obj => new SecurityIdentifier((byte[])obj, 0));

                return
                    new System.Collections.ObjectModel.Collection<SecurityIdentifier>(list.ToList());
            }

            AssertNullable(dn);

            return null;
        }

        public override bool IsEqual(object instance, object value, out DirectoryAttributeModification modification)
        {
            var propertyValue = GetValue(instance) as ICollection<SecurityIdentifier>;
            var identifiers = value as ICollection<SecurityIdentifier>;
            if ((propertyValue == null || propertyValue.Count == 0) && (identifiers == null || identifiers.Count == 0))
            {
                modification = null;
                return true;
            }

            if (propertyValue == null || identifiers == null || identifiers.Count != propertyValue.Count)
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
