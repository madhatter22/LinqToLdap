using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LinqToLdap.Mapping
{
    internal class ServerObjectMapping : IObjectMapping
    {
        public Type Type => GetType();

        public bool IsForAnonymousType => false;

        public string NamingContext => null;

        public string ObjectCategory => null;

        public bool IncludeObjectCategory => false;

        public IEnumerable<string> ObjectClasses => null;

        public bool HasCatchAllMapping => false;
        public bool IncludeObjectClasses => false;

        public bool FlattenHierarchy => true;

#if (!NET35 && !NET40)
        public System.Collections.ObjectModel.ReadOnlyDictionary<string, string> Properties => new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
#else
        public Collections.ReadOnlyDictionary<string, string> Properties => new Collections.ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
#endif

        public IEnumerable<IPropertyMapping> GetPropertyMappings()
        {
            return new List<IPropertyMapping>(0);
        }

        public IEnumerable<IPropertyMapping> GetPropertyMappingsForAdd()
        {
            return new List<IPropertyMapping>(0);
        }

        public IEnumerable<IPropertyMapping> GetPropertyMappingsForUpdate()
        {
            return new List<IPropertyMapping>(0);
        }

        public IPropertyMapping GetPropertyMapping(string name, Type owningType = null)
        {
            return null;
        }

        public IPropertyMapping GetPropertyMappingByAttribute(string name, Type owningType = null)
        {
            return null;
        }

        public IPropertyMapping GetDistinguishedNameMapping()
        {
            throw new NotSupportedException();
        }

        public IPropertyMapping GetCatchAllMapping()
        {
            throw new NotSupportedException();
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