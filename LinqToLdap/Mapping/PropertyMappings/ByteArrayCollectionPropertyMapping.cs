/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class ByteArrayCollectionPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        public ByteArrayCollectionPropertyMapping(PropertyMappingArguments<T> arguments)
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
            var value = (IEnumerable<byte[]>)GetValueForDirectory(instance);

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
                var bytes = value.GetValues(typeof(byte[]))
                    .Select(c => (byte[])c);

                return new System.Collections.ObjectModel.Collection<byte[]>(bytes.ToList());
            }

            AssertNullable(dn);

            return null;
        }

        public override bool IsEqual(object instance, object value, out DirectoryAttributeModification modification)
        {
            var propertyValue = GetValue(instance) as ICollection<byte[]>;
            var bytes = value as ICollection<byte[]>;
            if ((propertyValue == null || propertyValue.Count == 0) && (bytes == null || bytes.Count == 0))
            {
                modification = null;
                return true;
            }

            if (propertyValue == null || bytes == null || bytes.Count != propertyValue.Count)
            {
                modification = GetDirectoryAttributeModification(instance);
                return false;
            }

            var propertyEnum = propertyValue.GetEnumerator();
            var bytesEnum = bytes.GetEnumerator();
            while (propertyEnum.MoveNext() && bytesEnum.MoveNext())
            {
                if (propertyEnum.Current.SequenceEqual(bytesEnum.Current)) continue;

                modification = GetDirectoryAttributeModification(instance);
                return false;
            }

            modification = null;
            return true;
        }
    }
}
