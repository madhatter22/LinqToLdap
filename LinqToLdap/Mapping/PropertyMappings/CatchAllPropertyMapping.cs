using System;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class CatchAllPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        public CatchAllPropertyMapping(PropertyMappingArguments<T> arguments)
            : base(arguments)
        {
        }

        public override string FormatValueToFilter(object value)
        {
            throw new NotSupportedException("Catch All property does not support this operation.");
        }

        public override DirectoryAttributeModification GetDirectoryAttributeModification(object instance)
        {
            throw new NotSupportedException("Catch All property does not support this operation.");
        }

        public override object GetValueForDirectory(object instance)
        {
            throw new NotSupportedException("Catch All property does not support this operation.");
        }

        public override object FormatValueFromDirectory(DirectoryAttribute value, string dn)
        {
            throw new NotSupportedException("Catch All property does not support this operation.");
        }

        public override bool IsEqual(object instance, object value, out DirectoryAttributeModification modification)
        {
            throw new NotSupportedException("Catch All property does not support this operation.");
        }
    }
}