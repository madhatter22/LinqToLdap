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
using System.Linq;
using LinqToLdap.Collections;
using LinqToLdap.Exceptions;

namespace LinqToLdap.Mapping
{
    internal abstract class ObjectMapping : IObjectMapping
    {

#if NET45
        private readonly System.Collections.ObjectModel.ReadOnlyDictionary<string, IPropertyMapping> _propertyMappings;
        private readonly System.Collections.ObjectModel.ReadOnlyDictionary<string, IPropertyMapping> _attributePropertyMappings;
        private System.Collections.ObjectModel.ReadOnlyDictionary<string, string> _propertyNames;
#else
        private readonly Collections.ReadOnlyDictionary<string, IPropertyMapping> _propertyMappings;
        private readonly Collections.ReadOnlyDictionary<string, IPropertyMapping> _attributePropertyMappings;
        private Collections.ReadOnlyDictionary<string, string> _propertyNames;
#endif
        private readonly IPropertyMapping _distinguishedName;
        private readonly IPropertyMapping _catchAll;
        private readonly ReadOnlyCollection<IPropertyMapping> _updateablePropertyMappings;
        private ReadOnlyCollection<IObjectMapping> _readOnlySubTypeMappings;

        protected ObjectMapping(string namingContext, IEnumerable<IPropertyMapping> propertyMappings,
            string objectCategory = null, bool includeObjectCategory = true, IEnumerable<string> objectClass = null, bool includeObjectClasses = true)
        {
            NamingContext = namingContext;
            ObjectCategory = objectCategory;
            ObjectClasses = objectClass;

            var localPropertyMappings = propertyMappings.ToList();
            _propertyMappings = localPropertyMappings.ToDictionary(pm => pm.PropertyName).ToReadOnlyDictionary();

            try
            {
                _attributePropertyMappings =
                localPropertyMappings.ToDictionary(pm => pm.AttributeName, pm => pm, StringComparer.OrdinalIgnoreCase).ToReadOnlyDictionary();
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException("The same attribute cannot be mapped for multiple properties.", ex);
            }

            _propertyNames = InitializePropertyNames();

            _distinguishedName = localPropertyMappings.FirstOrDefault(p => p.IsDistinguishedName);

            _catchAll =
                localPropertyMappings.FirstOrDefault(p => typeof (IDirectoryAttributes).IsAssignableFrom(p.PropertyType));

            _updateablePropertyMappings =
                new ReadOnlyCollection<IPropertyMapping>(
                    _propertyMappings.Values.Where(p => !p.IsStoreGenerated && !p.IsDistinguishedName && !p.IsReadOnly)
                        .ToList());

            IncludeObjectCategory = includeObjectCategory;
            IncludeObjectClasses = includeObjectClasses;
        }

        public IDictionary<string, IObjectMapping> SubTypeMappingsObjectClassDictionary { get; } =
            new Dictionary<string, IObjectMapping>(StringComparer.OrdinalIgnoreCase);
        public IDictionary<Type, IObjectMapping> SubTypeMappingsTypeDictionary { get; } =
            new Dictionary<Type, IObjectMapping>();

        public abstract Type Type { get; }
        public abstract bool IsForAnonymousType { get; }

        public string NamingContext { get; }
        public string ObjectCategory { get; }
        public bool IncludeObjectCategory { get; }
        public IEnumerable<string> ObjectClasses { get; }
        public bool HasCatchAllMapping => _catchAll != null;
        public bool IncludeObjectClasses { get; }
        public bool HasSubTypeMappings => SubTypeMappings != null && SubTypeMappings.Count > 0;

#if NET45
        public System.Collections.ObjectModel.ReadOnlyDictionary<string, string> Properties => _propertyNames ?? (_propertyNames = InitializePropertyNames());
#else
        public Collections.ReadOnlyDictionary<string, string> Properties => _propertyNames ?? (_propertyNames = InitializePropertyNames());
#endif

        public ReadOnlyCollection<IObjectMapping> SubTypeMappings => _readOnlySubTypeMappings ??
                                                                     (_readOnlySubTypeMappings = new ReadOnlyCollection<IObjectMapping>(SubTypeMappingsObjectClassDictionary.Values.ToList()));

        public IEnumerable<IPropertyMapping> GetPropertyMappings()
        {
            return _propertyMappings.Values;
        }

        public IEnumerable<IPropertyMapping> GetUpdateablePropertyMappings()
        {
            return _updateablePropertyMappings;
        }

        public IPropertyMapping GetPropertyMapping(string name, Type owningType = null)
        {
            if (owningType == null || owningType == Type)
            {
                IPropertyMapping mapping;
                if (_propertyMappings.TryGetValue(name, out mapping))
                {
                    return mapping;
                }
                if (HasSubTypeMappings)
                {
                    return null;
                }
            }
            else if (HasSubTypeMappings)
            {
                IObjectMapping subTypeMapping;
                if (SubTypeMappingsTypeDictionary.TryGetValue(owningType, out subTypeMapping))
                {
                    return subTypeMapping.GetPropertyMapping(name);
                }
            }

            throw new MappingException($"Property mapping with name '{name}' was not found for '{Type.FullName}'");
        }

        public IPropertyMapping GetPropertyMappingByAttribute(string name, Type owningType = null)
        {
            if (owningType == null || owningType == Type)
            {
                IPropertyMapping mapping;
                if (_attributePropertyMappings.TryGetValue(name, out mapping))
                {
                    return mapping;
                }
            }
            else if (HasSubTypeMappings)
            {
                IObjectMapping subTypeMapping;
                if (SubTypeMappingsTypeDictionary.TryGetValue(owningType, out subTypeMapping))
                {
                    return subTypeMapping.GetPropertyMappingByAttribute(name);
                }
            }

            return null;
        }

        public IPropertyMapping GetDistinguishedNameMapping()
        {
            return _distinguishedName;
        }

        public IPropertyMapping GetCatchAllMapping()
        {
            return _catchAll;
        }

        public abstract object Create(object[] parameters = null, object[] objectClasses = null);

        public virtual void AddSubTypeMapping(IObjectMapping mapping)
        {
            if (SubTypeMappingsObjectClassDictionary.Values.Contains(mapping)) return;

            var currentMappings = SortByInheritanceDescending(SubTypeMappingsObjectClassDictionary.Values.Union(new[] {mapping}));

            SubTypeMappingsObjectClassDictionary.Clear();
            SubTypeMappingsTypeDictionary.Clear();
            
            foreach (var currentMapping in currentMappings)
            {
                var objectClasses = currentMapping.ObjectClasses.ToList();

                //find direct ancestor object classes or default to this class' object classes if a direct ancestor hasn't been mapped yet.
                var parentObjectClasses = currentMappings
                    .Where(x => currentMapping.Type.IsSubclassOf(x.Type))
                    .Select(x => x.ObjectClasses)
                    .FirstOrDefault() ?? ObjectClasses;

                objectClasses = objectClasses.Except(parentObjectClasses, StringComparer.OrdinalIgnoreCase).ToList();

                if (objectClasses.Count == 0)
                    throw new InvalidOperationException("Unable to identify distinct object class based on mapped inheritance");

                SubTypeMappingsObjectClassDictionary.Add(objectClasses[0], currentMapping);
                SubTypeMappingsTypeDictionary.Add(currentMapping.Type, currentMapping);
            }

            _readOnlySubTypeMappings = null;
            _propertyNames = InitializePropertyNames();
        }

#if NET45
        private System.Collections.ObjectModel.ReadOnlyDictionary<string, string> InitializePropertyNames()
#else
        private Collections.ReadOnlyDictionary<string, string> InitializePropertyNames()
#endif
        {
            var properties = _propertyMappings.ToDictionary(x => x.Key, x => x.Value.AttributeName, StringComparer.OrdinalIgnoreCase);

            if (HasSubTypeMappings)
            {
                foreach (var subTypeMapping in SubTypeMappings)
                {
                    foreach (var subTypeProperty in subTypeMapping.Properties)
                    {
                        if (!properties.ContainsKey(subTypeProperty.Key))
                        {
                            properties.Add(subTypeProperty.Key, subTypeProperty.Value);
                        }
                    }
                }
            }

#if NET45
        return new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(properties);
#else
        return new Collections.ReadOnlyDictionary<string, string>(properties);
#endif
        }

        private List<IObjectMapping> SortByInheritanceDescending(IEnumerable<IObjectMapping> mappings)
        {
            Dictionary<int, List<IObjectMapping>> hiearchy = new Dictionary<int, List<IObjectMapping>>();

            foreach (var objectMapping in mappings)
            {
                int count = 0;
                var baseType = objectMapping.Type.BaseType;
                while (baseType != Type && baseType != null)
                {
                    count++;
                    baseType = baseType.BaseType;
                }

                List<IObjectMapping> list;
                if (hiearchy.TryGetValue(count, out list))
                {
                    list.Add(objectMapping);
                }
                else
                {
                    hiearchy[count] = new List<IObjectMapping> {objectMapping};
                }
            }

            return hiearchy.OrderByDescending(x => x.Key).SelectMany(x => x.Value).ToList();
        }
    }
}
