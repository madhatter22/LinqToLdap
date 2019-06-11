using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class X509Certificate2CollectionPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        private readonly bool _isX5092;

        public X509Certificate2CollectionPropertyMapping(PropertyMappingArguments<T> arguments)
            : base(arguments)
        {
            var genericArguments = PropertyType.GetGenericArguments();
            _isX5092 = genericArguments.Length == 1 && genericArguments[0] == typeof(X509Certificate2);
        }

        public override string FormatValueToFilter(object value)
        {
            if (!(value is X509Certificate))
                throw new NotSupportedException("Collection<X509Certificate2> cannot be used in filters.");

            return (value as X509Certificate).GetRawCertData().ToStringOctet();
        }

        public override DirectoryAttributeModification GetDirectoryAttributeModification(object instance)
        {
            var modification = new DirectoryAttributeModification
            {
                Name = AttributeName,
                Operation = DirectoryAttributeOperation.Replace
            };
#if NET35
            if (_isX5092)
            {
                var value = (IEnumerable<X509Certificate2>)GetValueForDirectory(instance);
                if (value != null)
                {
                    foreach (var b in value)
                    {
                        modification.Add(b.GetRawCertData());
                    }
                }
            }
            else
            {
                var value = (IEnumerable<X509Certificate>) GetValueForDirectory(instance);
                if (value != null)
                {
                    foreach (var b in value)
                    {
                        modification.Add(b.GetRawCertData());
                    }
                }
            }
#else
            var value = (IEnumerable<X509Certificate>)GetValueForDirectory(instance);
            if (value != null)
            {
                foreach (var b in value)
                {
                    modification.Add(b.GetRawCertData());
                }
            }
#endif

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
                if (_isX5092)
                {
                    var certs = value.GetValues(typeof(byte[]))
                        .Select(c => new X509Certificate2((byte[])c));
                    return new System.Collections.ObjectModel.Collection<X509Certificate2>(certs.ToList());
                }
                else
                {
                    var certs = value.GetValues(typeof(byte[]))
                        .Select(c => new X509Certificate((byte[])c));
                    return new System.Collections.ObjectModel.Collection<X509Certificate>(certs.ToList());
                }
            }

            AssertNullable(dn);

            return null;
        }

        public override bool IsEqual(object instance, object value, out DirectoryAttributeModification modification)
        {
            if (_isX5092)
            {
                var propertyValue = GetValue(instance) as ICollection<X509Certificate2>;
                var certificates = value as ICollection<X509Certificate2>;
                if ((propertyValue == null || propertyValue.Count == 0) && (certificates == null || certificates.Count == 0))
                {
                    modification = null;
                    return true;
                }

                if (propertyValue == null || certificates == null || certificates.Count != propertyValue.Count)
                {
                    modification = GetDirectoryAttributeModification(instance);
                    return false;
                }

                if (!certificates.SequenceEqual(propertyValue))
                {
                    modification = GetDirectoryAttributeModification(instance);
                    return false;
                }
            }
            else
            {
                var propertyValue = GetValue(instance) as ICollection<X509Certificate>;
                var certificates = value as ICollection<X509Certificate>;
                if ((propertyValue == null || propertyValue.Count == 0) && (certificates == null || certificates.Count == 0))
                {
                    modification = null;
                    return true;
                }

                if (propertyValue == null || certificates == null || certificates.Count != propertyValue.Count)
                {
                    modification = GetDirectoryAttributeModification(instance);
                    return false;
                }

                if (!certificates.SequenceEqual(propertyValue))
                {
                    modification = GetDirectoryAttributeModification(instance);
                    return false;
                }
            }

            modification = null;
            return true;
        }
    }
}