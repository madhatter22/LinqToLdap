using System;
using System.Collections.Generic;

namespace LinqToLdap.Mapping.PropertyMappings
{
    internal class PropertyMappingArguments<T>
    {
        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }
        public string AttributeName { get; set; }
        public Func<T, object> Getter { get; set; }
        public Action<T, object> Setter { get; set; }
        public bool IsDistinguishedName { get; set; }
        public ReadOnly ReadOnly { get; set; }
        public Dictionary<string, object> DirectoryMappings { get; set; }
        public Dictionary<object, string> InstanceMappings { get; set; }
    }
}