/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using System.DirectoryServices.Protocols;
using System.Linq;
using LinqToLdap.Collections;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class StringArrayPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        public StringArrayPropertyMapping(PropertyMappingArguments<T> arguments)
            : base(arguments)
        {
        }

        public override string FormatValueToFilter(object value)
        {
            if (!(value is string))
                throw new NotSupportedException("String arrays cannot be used in filters.");

            return value.ToString().CleanFilterValue();
        }

        public override DirectoryAttributeModification GetDirectoryAttributeModification(object instance)
        {
            var modification = new DirectoryAttributeModification
            {
                Name = AttributeName,
                Operation = DirectoryAttributeOperation.Replace
            };
            var value = (string[])GetValueForDirectory(instance);

            if (value != null)
            {
                foreach (var s in value)
                {
                    modification.Add(s);
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
                string[] strings = Array.ConvertAll(value.GetValues(typeof(string)), obj => (string) obj);

                return strings;
            }

            AssertNullable(dn);

            return null;
        }

        public override bool IsEqual(object instance, object value, out DirectoryAttributeModification modification)
        {
            var propertyValue = GetValue(instance) as string[];
            var strings = value as string[];
            if ((propertyValue == null || propertyValue.Length == 0) && (strings == null || strings.Length == 0))
            {
                modification = null;
                return true;
            }

            if (propertyValue == null || strings == null || strings.Length != propertyValue.Length)
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
