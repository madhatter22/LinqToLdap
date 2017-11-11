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
using LinqToLdap.Collections;

namespace LinqToLdap.Mapping
{
    internal class DynamicObjectMapping : IObjectMapping
    {
        public DynamicObjectMapping(string namingContext, IEnumerable<string> objectClasses, string objectCategory, string objectClass = null)
        {
            if (namingContext.IsNullOrEmpty()) throw new ArgumentNullException(nameof(namingContext));

            NamingContext = namingContext;
            ObjectCategory = objectCategory;
            if (objectClass != null)
            {
                if (objectClasses != null)
                    throw new ArgumentException("objectClass and objectClasses cannot both have a value.");

                objectClasses = new[] { objectClass };
            }
            ObjectClasses = objectClasses;
        }

        public Type Type => typeof(DirectoryAttributes);

        public bool IsForAnonymousType => false;

        public string NamingContext { get; }

        public string ObjectCategory { get; }

        public bool IncludeObjectCategory => true;

        public IEnumerable<string> ObjectClasses { get; }

        public bool HasCatchAllMapping => false;
        public bool IncludeObjectClasses => true;

#if NET45
        private System.Collections.ObjectModel.ReadOnlyDictionary<string, string> _properties;
        public System.Collections.ObjectModel.ReadOnlyDictionary<string, string> Properties => _properties ?? (_properties = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(new Dictionary<string, string>()));
#else
        private Collections.ReadOnlyDictionary<string, string> _properties;
        public Collections.ReadOnlyDictionary<string, string> Properties => _properties ?? (_properties = new Collections.ReadOnlyDictionary<string, string>(new Dictionary<string, string>()));
#endif
        public IEnumerable<IPropertyMapping> GetPropertyMappings()
        {
            throw new NotSupportedException("Properties are not mapped for DirectoryAttributes.");
        }

        public IEnumerable<IPropertyMapping> GetUpdateablePropertyMappings()
        {
            throw new NotSupportedException("Properties are not mapped for DirectoryAttributes.");
        }

        public IPropertyMapping GetPropertyMapping(string name, Type owningType = null)
        {
            throw new NotSupportedException("Properties are not mapped for DirectoryAttributes.");
        }

        public IPropertyMapping GetPropertyMappingByAttribute(string name, Type owningType = null)
        {
            throw new NotSupportedException("Properties are not mapped for DirectoryAttributes.");
        }

        public IPropertyMapping GetDistinguishedNameMapping()
        {
            throw new NotSupportedException("Properties are not mapped for DirectoryAttributes.");
        }

        public IPropertyMapping GetCatchAllMapping()
        {
            throw new NotSupportedException("Properties are not mapped for DirectoryAttributes.");
        }

        public object Create(object[] parameters, object[] objectClasses = null)
        {
            throw new NotSupportedException();
        }
        
        public ReadOnlyCollection<IObjectMapping> SubTypeMappings => null;

        public void AddSubTypeMapping(IObjectMapping mapping)
        {
            throw new NotSupportedException(GetType().Name + " does not support SubTypes");
        }

        public bool HasSubTypeMappings => false;
    }
}
