using System;
using System.DirectoryServices.Protocols;
using System.Linq.Expressions;
using System.Reflection;
using LinqToLdap.Helpers;
using LinqToLdap.Mapping.PropertyMappings;

namespace LinqToLdap.Mapping
{
    internal class CustomPropertyMappingBuilder<T, TProperty> : IPropertyMappingBuilder, ICustomPropertyMapper<T, TProperty> where T : class 
    {
        private Func<DirectoryAttribute, TProperty> _convertFrom;
        private Func<TProperty, object> _convertTo;
        private Func<TProperty, string> _convertToFilter;
        private Func<TProperty, TProperty, bool> _isEqual;
        
        public CustomPropertyMappingBuilder(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            PropertyInfo = propertyInfo;
        }

        public bool IsDistinguishedName { get { return false; } }
        public bool IsReadOnly { get; private set; }

        public bool IsStoreGenerated { get; private set; }
        public string AttributeName { get; private set; }
        public PropertyInfo PropertyInfo { get; set; }

        public string PropertyName
        {
            get { return PropertyInfo.Name; }
        }

        public IPropertyMapping ToPropertyMapping()
        {
            var arguments = new PropertyMappingArguments<T>
            {
                PropertyName = PropertyInfo.Name,
                PropertyType = PropertyInfo.PropertyType,
                AttributeName = AttributeName ?? PropertyInfo.Name.Replace('_', '-'),
                Getter = DelegateBuilder.BuildGetter<T>(PropertyInfo),
                Setter = DelegateBuilder.BuildSetter<T>(PropertyInfo),
                IsStoreGenerated = IsStoreGenerated,
                IsDistinguishedName = IsDistinguishedName,
                IsReadOnly = IsReadOnly,
            };
            return new CustomPropertyMapping<T, TProperty>(arguments, _convertFrom, _convertTo, _convertToFilter, _isEqual);
        }
        
        public ICustomPropertyMapper<T, TProperty> Named(string attributeName)
        {
            AttributeName = attributeName.IsNullOrEmpty() ? null : attributeName;
            return this;
        }

        public ICustomPropertyMapper<T, TProperty> StoreGenerated()
        {
            IsStoreGenerated = true;
            return this;
        }

        public ICustomPropertyMapper<T, TProperty> ConvertFromDirectoryUsing(Func<DirectoryAttribute, TProperty> converter)
        {
            if (converter == null) throw new ArgumentNullException("converter");
            _convertFrom = converter;
            return this;
        }

        public ICustomPropertyMapper<T, TProperty> ConvertToFilterUsing(Func<TProperty, string> converter)
        {
            if (converter == null) throw new ArgumentNullException("converter");
            _convertToFilter = converter;
            return this;
        }

        public ICustomPropertyMapper<T, TProperty> ConvertToDirectoryUsing(Func<TProperty, byte[]> converter)
        {
            if (converter == null) throw new ArgumentNullException("converter");
            Expression<Func<TProperty, object>> e = v => converter(v);
            _convertTo = e.Compile();
            return this;
        }

        public ICustomPropertyMapper<T, TProperty> ConvertToDirectoryUsing(Func<TProperty, byte[][]> converter)
        {
            if (converter == null) throw new ArgumentNullException("converter");
            Expression<Func<TProperty, object>> e = v => converter(v);
            _convertTo = e.Compile();
            return this;
        }

        public ICustomPropertyMapper<T, TProperty> ConvertToDirectoryUsing(Func<TProperty, string> converter)
        {
            if (converter == null) throw new ArgumentNullException("converter");
            Expression<Func<TProperty, object>> e = v => converter(v);
            _convertTo = e.Compile();
            return this;
        }

        public ICustomPropertyMapper<T, TProperty> ConvertToDirectoryUsing(Func<TProperty, string[]> converter)
        {
            if (converter == null) throw new ArgumentNullException("converter");
            Expression<Func<TProperty, object>> e = v => converter(v);
            _convertTo = e.Compile();

            return this;
        }

        public ICustomPropertyMapper<T, TProperty> CompareChangesUsing(Func<TProperty, TProperty, bool> comparer)
        {
            if (comparer == null) throw new ArgumentNullException("comparer");
            _isEqual = comparer;

            return this;
        }

        public ICustomPropertyMapper<T, TProperty> ReadOnly()
        {
            IsReadOnly = true;

            return this;
        }
    }
}
