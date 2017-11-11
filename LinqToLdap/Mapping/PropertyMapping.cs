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

namespace LinqToLdap.Mapping
{
    internal abstract class PropertyMapping : IPropertyMapping
    {
        protected PropertyMapping(Type propertyType, string propertyName, string attributeName, bool isStoreGenerated, bool isDistinguishedName, bool isReadOnly)
        {
            IsDistinguishedName = isDistinguishedName;
            IsReadOnly = isReadOnly || isDistinguishedName;
            IsStoreGenerated = isStoreGenerated;
            PropertyType = propertyType;
            PropertyName = propertyName;
            AttributeName = attributeName;
            
            IsNullable = DetermineIfNullable();
            UnderlyingType = IsNullable && PropertyType.IsValueType
                                  ? Nullable.GetUnderlyingType(PropertyType)
                                  : PropertyType;

            DefaultValue = GetType()
                .GetMethod("GetDefault")
                .MakeGenericMethod(PropertyType).Invoke(this, null);

        }

        private bool DetermineIfNullable()
        {
            if (!PropertyType.IsValueType) return true;
            return Nullable.GetUnderlyingType(PropertyType) != null;
        }

        public bool IsNullable { get; private set; }
        protected object DefaultValue { get; private set; }

        public Type PropertyType { get; private set; }
        public Type UnderlyingType { get; private set; }
        public bool IsStoreGenerated { get; private set; }
        public bool IsDistinguishedName { get; private set; }
        public bool IsReadOnly { get; private set; }

        public string PropertyName { get; private set; }
        public string AttributeName { get; private set; }
        
        public abstract object GetValue(object instance);
        public abstract void SetValue(object instance, object value);

        public virtual object Default()
        {
            return DefaultValue;
        }

        public abstract object FormatValueFromDirectory(DirectoryAttribute value, string dn);

        public abstract string FormatValueToFilter(object value);
        public abstract DirectoryAttributeModification GetDirectoryAttributeModification(object instance);

        public virtual DirectoryAttribute GetDirectoryAttribute(object instance)
        {
            return GetDirectoryAttributeModification(instance);
        }

        public virtual bool IsEqual(object instance, object value, out DirectoryAttributeModification modification)
        {
            if (!Equals(GetValue(instance), value))
            {
                modification = GetDirectoryAttributeModification(instance);
                return false;
            }
            modification = null;
            return true;
        }

        public abstract object GetValueForDirectory(object instance);

        public TType GetDefault<TType>()
        {
            return default(TType);
        }
    }
}
