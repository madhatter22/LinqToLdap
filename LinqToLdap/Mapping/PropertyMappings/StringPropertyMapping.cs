using System.DirectoryServices.Protocols;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class StringPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        public StringPropertyMapping(PropertyMappingArguments<T> arguments)
            : base(arguments)
        {
        }

        public override string FormatValueToFilter(object value)
        {
            return value.ToString().CleanFilterValue();
        }

        public override DirectoryAttributeModification GetDirectoryAttributeModification(object instance)
        {
            var modification = new DirectoryAttributeModification
            {
                Name = AttributeName,
                Operation = DirectoryAttributeOperation.Replace
            };
            var value = (string)GetValueForDirectory(instance);

            if (value != null)
            {
                modification.Add(value);
            }

            return modification;
        }

        public override object GetValueForDirectory(object instance)
        {
            return GetValue(instance);
        }

        public override object FormatValueFromDirectory(DirectoryAttribute value, string dn)
        {
            return value != null ? value.Count == 0 ? string.Empty : value[0] : null;
        }
    }
}