using System;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class EnumPropertyMapping<T> : PropertyMappingGeneric<T> where T : class
    {
        private readonly bool _isStoredAsInt;

        public EnumPropertyMapping(PropertyMappingArguments<T> arguments, bool isStoredAsInt) : base(arguments)
        {
            _isStoredAsInt = isStoredAsInt;
        }

        public override object FormatValueFromDirectory(DirectoryAttribute value, string dn)
        {
            string str;
            if (value != null && value.Count > 0 && (str = value[0] as string) != null && !str.IsNullOrEmpty())
            {
                try
                {
                    var en = Enum.Parse(UnderlyingType, str);

                    return en;
                }
                catch (Exception ex)
                {
                    ThrowMappingException(str, dn, ex);
                }
            }

            AssertNullable(dn);

            return null;
        }

        public override string FormatValueToFilter(object value)
        {
            if (_isStoredAsInt)
            {
                return ((int)value).ToString();
            }
            if (value is int)
            {
                return Enum.GetName(UnderlyingType, value);
            }
            return value.ToString();
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
            var value = GetValue(instance);

            if (value == null) return value;

            return _isStoredAsInt
                ? ((int)value).ToString()
                : value.ToString();
        }
    }
}