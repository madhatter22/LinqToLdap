using LinqToLdap.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LinqToLdap.Helpers
{
    internal static class ObjectActivator
    {
#if (NET35 || NET40)
        private readonly static LinqToLdap.Collections.SafeDictionary<string, UnknownCtorWithParams> _constructors = new LinqToLdap.Collections.SafeDictionary<string, UnknownCtorWithParams>();
#else
        private readonly static System.Collections.Concurrent.ConcurrentDictionary<string, UnknownCtorWithParams> _constructors = new System.Collections.Concurrent.ConcurrentDictionary<string, UnknownCtorWithParams>();
#endif

        public static object CreateGenericInstance(Type instanceType, Type genericParameterType, object[] parameters, string key)
        {
            return _constructors.GetOrAdd(instanceType.FullName + genericParameterType.FullName + (key ?? string.Empty), t =>
            {
                var type = instanceType.MakeGenericType(genericParameterType);
                var constructors = type.GetConstructors().Where(x => x.GetParameters().Length == parameters.Length).ToArray();

                ConstructorInfo constructor = constructors.Length == 1 ? constructors[0] : null;

                if (constructor == null)
                {
                    var ranked = AnonList(new { Key = default(int), Value = default(ConstructorInfo) });
                    foreach (var ctor in constructors)
                    {
                        var constructorParameters = ctor.GetParameters();
                        //find best parameter fit
                        int matches = 0;
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var parameter = parameters[i];
                            var constructorParameter = constructorParameters[i];
                            if (parameter == null) continue;

                            if (constructorParameter.ParameterType.IsAssignableFrom(parameter.GetType()))
                            {
                                matches++;
                            }
                        }
                        ranked.Add(new { Key = matches, Value = ctor });
                    }

                    constructor = ranked.OrderByDescending(x => x.Key).Select(x => x.Value).FirstOrDefault();
                }
                if (constructor == null) throw new MissingConstructorException($"Missing constructor for {type.FullName}");

                return DelegateBuilder.BuildUnknownCtorWithParams(constructor);
            }).Invoke(parameters);
        }

        private static List<T> AnonList<T>(T _)
        {
            return new List<T>();
        }
    }
}