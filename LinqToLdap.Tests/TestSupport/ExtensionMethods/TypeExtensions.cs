using System;
using System.Linq;
using System.Reflection;

namespace LinqToLdap.Tests.TestSupport.ExtensionMethods
{
    public static class TypeExtensions
    {
        private const BindingFlags Flags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        
        public static T Create<T>(this Type type, params object[] parameters)
        {
            var constructor = type.GetConstructor(Flags, null,
                                                  parameters != null
                                                      ? parameters.Select(o => o.GetType()).ToArray()
                                                      : Type.EmptyTypes,
                                                  null);

            return constructor.Invoke(parameters).CastTo<T>();
        }
    }
}
