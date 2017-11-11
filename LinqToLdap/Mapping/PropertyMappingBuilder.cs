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
using System.Collections.ObjectModel;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using LinqToLdap.Collections;
using LinqToLdap.Exceptions;
using LinqToLdap.Helpers;
using LinqToLdap.Mapping.PropertyMappings;

namespace LinqToLdap.Mapping
{
    internal class PropertyMappingBuilder<T, TProperty> : IPropertyMapperGeneric<TProperty>, IPropertyMappingBuilder, IPropertyMapper where T : class 
    {
        public PropertyMappingBuilder(PropertyInfo propertyInfo, bool isDistinguishedName, bool isReadyOnly)
        {
            IsDistinguishedName = isDistinguishedName;
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            PropertyInfo = propertyInfo;
            IsReadOnly = isReadyOnly;
        }

        public string DateTimeFormat { get; private set; } = "yyyyMMddHHmmss.0Z";

        public bool IsReadOnly { get; private set; }
        public bool IsDistinguishedName { get; }
        public bool IsEnumStoredAsInt { get; private set; }
        public bool IsStoreGenerated { get; private set; }
        public string AttributeName { get; private set; }
        public PropertyInfo PropertyInfo { get; }

        public string PropertyName => PropertyInfo.Name;

        public virtual IPropertyMapping ToPropertyMapping()
        {
            IPropertyMapping mapping;

            var type = typeof (T);
            var arguments = new PropertyMappingArguments<T>
                                {
                                    PropertyName = PropertyInfo.Name,
                                    PropertyType = PropertyInfo.PropertyType,
                                    AttributeName = AttributeName ?? PropertyInfo.Name.Replace('_', '-'),
                                    Getter = DelegateBuilder.BuildGetter<T>(PropertyInfo),
                                    Setter = !type.IsAnonymous()
                                                 ? DelegateBuilder.BuildSetter<T>(PropertyInfo)
                                                 : null,
                                    IsStoreGenerated = IsStoreGenerated,
                                    IsDistinguishedName = IsDistinguishedName,
                                    IsReadOnly = IsReadOnly,
                                    DirectoryMappings = null,
                                    InstanceMappings = null
                                };

            if (PropertyInfo.PropertyType == typeof(DateTime) || PropertyInfo.PropertyType == typeof(DateTime?))
            {
                mapping = new DatePropertyMapping<T>(arguments, DateTimeFormat);
            }
            else if (PropertyInfo.PropertyType.IsEnum || (Nullable.GetUnderlyingType(PropertyInfo.PropertyType) != null
                && Nullable.GetUnderlyingType(PropertyInfo.PropertyType).IsEnum))
            {
                mapping = new EnumPropertyMapping<T>(arguments, IsEnumStoredAsInt);
            }
            else if (PropertyInfo.PropertyType == typeof(byte[]))
            {
                mapping = new ByteArrayPropertyMapping<T>(arguments);
            }
            else if (PropertyInfo.PropertyType == typeof(Guid) || PropertyInfo.PropertyType == typeof(Guid?))
            {
                mapping = new GuidPropertyMapping<T>(arguments);
            }
            else if (PropertyInfo.PropertyType == typeof(string))
            {
                mapping = new StringPropertyMapping<T>(arguments);
            }
            else if (PropertyInfo.PropertyType == typeof(string[]))
            {
                mapping = new StringArrayPropertyMapping<T>(arguments);
            }
            else if (PropertyInfo.PropertyType == typeof(DateTime[]) || PropertyInfo.PropertyType == typeof(DateTime?[]))
            {
                mapping = new DateArrayPropertyMapping<T>(arguments, DateTimeFormat);
            }
            else if (PropertyInfo.PropertyType == typeof(ICollection<DateTime>) || PropertyInfo.PropertyType == typeof(Collection<DateTime>) ||
            PropertyInfo.PropertyType == typeof(ICollection<DateTime?>) || PropertyInfo.PropertyType == typeof(Collection<DateTime?>))
            {
                mapping = new DateCollectionPropertyMapping<T>(arguments, DateTimeFormat);
            }
            else if (PropertyInfo.PropertyType == typeof(Collection<string>) || PropertyInfo.PropertyType == typeof(ICollection<string>))
            {
                mapping = new StringCollectionPropertyMapping<T>(arguments);
            }
            else if (PropertyInfo.PropertyType == typeof(Collection<byte[]>) || PropertyInfo.PropertyType == typeof(ICollection<byte[]>))
            {
                mapping = new ByteArrayCollectionPropertyMapping<T>(arguments);
            }
            else if (PropertyInfo.PropertyType == typeof(bool) || PropertyInfo.PropertyType == typeof(bool?))
            {
                mapping = new BooleanPropertyMapping<T>(arguments);
            }
            else if (PropertyInfo.PropertyType == typeof(SecurityIdentifier))
            {
                mapping = new SecurityIdentifierPropertyMapping<T>(arguments);
            }
            else if (PropertyInfo.PropertyType == typeof(SecurityIdentifier[]))
            {
                mapping = new SecurityIdentifierArrayPropertyMapping<T>(arguments);
            }
            else if (PropertyInfo.PropertyType == typeof(ICollection<SecurityIdentifier>) || PropertyInfo.PropertyType == typeof(Collection<SecurityIdentifier>))
            {
                mapping = new SecurityIdentifierCollectionPropertyMapping<T>(arguments);
            }
            else if (PropertyInfo.PropertyType == typeof(X509Certificate2) || PropertyInfo.PropertyType == typeof(X509Certificate))
            {
                mapping = new X509Certificate2PropertyMapping<T>(arguments);
            }
            else if (PropertyInfo.PropertyType == typeof(byte[][]))
            {
                mapping = new ByteArrayArrayPropertyMapping<T>(arguments);
            }
            else if (PropertyInfo.PropertyType == typeof(ICollection<X509Certificate>) || PropertyInfo.PropertyType == typeof(Collection<X509Certificate>) ||
                PropertyInfo.PropertyType == typeof(ICollection<X509Certificate2>) || PropertyInfo.PropertyType == typeof(Collection<X509Certificate2>))
            {
                mapping = new X509Certificate2CollectionPropertyMapping<T>(arguments);
            }
            else if (PropertyInfo.PropertyType == typeof(X509Certificate[]) || PropertyInfo.PropertyType == typeof(X509Certificate2[]))
            {
                mapping = new X509Certificate2ArrayPropertyMapping<T>(arguments);
            }
            else if (PropertyInfo.PropertyType.IsValueType || (Nullable.GetUnderlyingType(PropertyInfo.PropertyType) != null))
            {
                mapping = new NumericPropertyMapping<T>(arguments);
            }
            else if (typeof (IDirectoryAttributes).IsAssignableFrom(PropertyInfo.PropertyType))
            {
                mapping = new CatchAllPropertyMapping<T>(arguments);
            }
            else
            {
                throw new MappingException(string.Format("Type '{0}' could not be mapped.", PropertyInfo.PropertyType.FullName));
            }
            
            return mapping;
        }

        IPropertyMapperGeneric<TProperty> IPropertyMapperGeneric<TProperty>.DateTimeFormat(string format)
        {
            DateTimeFormat = format;
            return this;
        }

        IPropertyMapperGeneric<TProperty> IPropertyMapperGeneric<TProperty>.EnumStoredAsInt()
        {
            IsEnumStoredAsInt = true;
            return this;
        }

        IPropertyMapperGeneric<TProperty> IPropertyMapperGeneric<TProperty>.StoreGenerated()
        {
            IsStoreGenerated = true;
            return this;
        }

        IPropertyMapper IPropertyMapper.ReadOnly()
        {
            IsReadOnly = true;
            return this;
        }

        IPropertyMapperGeneric<TProperty> IPropertyMapperGeneric<TProperty>.ReadOnly()
        {
            IsReadOnly = true;
            return this;
        }

        IPropertyMapperGeneric<TProperty> IPropertyMapperGeneric<TProperty>.Named(string attributeName)
        {
            AttributeName = attributeName.IsNullOrEmpty() ? null : attributeName;
            return this;
        }

        IPropertyMapper IPropertyMapper.Named(string attributeName)
        {
            AttributeName = attributeName.IsNullOrEmpty() ? null : attributeName;
            return this;
        }

        IPropertyMapper IPropertyMapper.DateTimeFormat(string format)
        {
           DateTimeFormat = format;
            return this;
        }

        IPropertyMapper IPropertyMapper.EnumStoredAsInt()
        {
            IsEnumStoredAsInt = true;
            return this;
        }

        IPropertyMapper IPropertyMapper.StoreGenerated()
        {
            IsStoreGenerated = true;
            return this;
        }
    }
}
