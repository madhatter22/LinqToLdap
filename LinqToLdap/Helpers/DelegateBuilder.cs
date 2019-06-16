using LinqToLdap.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace LinqToLdap.Helpers
{
    internal delegate T CtorWithParams<out T>(params object[] args);

    internal delegate T Ctor<out T>();

    internal delegate object UnknownCtor();

    internal delegate object UnknownCtorWithParams(params object[] args);

    internal static class DelegateBuilder
    {
        internal static Action<T, object> BuildSetter<T>(PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof(T), "i");
            var argument = Expression.Parameter(typeof(object), "a");
            var setterCall = Expression.Call(
                instance,
                propertyInfo.GetSetMethod(true),
                Expression.Convert(argument, propertyInfo.PropertyType));
            var setter = Expression.Lambda(setterCall, instance, argument);

            return (Action<T, object>)setter.Compile();
        }

        internal static Func<T, object> BuildGetter<T>(PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof(T), "i");
            var getterCall = Expression.Call(
                instance,
                propertyInfo.GetGetMethod(true));
            var conversion = Expression.Convert(getterCall, typeof(object));
            var getter = Expression.Lambda(conversion, instance);

            return (Func<T, object>)getter.Compile();
        }

        private static Type GetConcreteType(Type type)
        {
            if (type.IsInterface)
            {
                if (type.IsGenericType)
                {
                    Type typeDef = type.GetGenericTypeDefinition(), listType = null;

                    if (typeDef == typeof(IList<>)) listType = typeof(List<>);
                    else if (typeDef == typeof(IDictionary<,>)) listType = typeof(Dictionary<,>);
                    if (listType != null)
                    {
                        type = listType.MakeGenericType(type.GetGenericArguments());
                    }
                }
            }
            return type;
        }

        private static Ctor<T> BuildCtor<T>(Type concreteType)
        {
            var ctor = concreteType.GetConstructor(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null, Type.EmptyTypes, null);

            if (ctor == null)
            {
                string message = "No parameterless constructor found for " + typeof(T).FullName;
                return delegate
                {
                    throw new MissingConstructorException(message);
                };
            }

            var dyn = new DynamicMethod("ctorWrapper", MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard, typeof(T), Type.EmptyTypes, concreteType, true);
            ILGenerator il = dyn.GetILGenerator();
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ret);
            return (Ctor<T>)dyn.CreateDelegate(typeof(Ctor<T>));
        }

        internal static Ctor<T> BuildCtor<T>()
        {
            Type type = GetConcreteType(typeof(T));
            return BuildCtor<T>(type);
        }

        internal static CtorWithParams<T> BuildCtorWithParams<T>(ConstructorInfo ctor)
        {
            var paramsInfo = ctor.GetParameters();

            var param = Expression.Parameter(typeof(object[]), "args");
            var argsExp = new Expression[paramsInfo.Length];

            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp = Expression.ArrayIndex(param, index);
                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            var newExp = Expression.New(ctor, argsExp);
            var lambda = Expression.Lambda(typeof(CtorWithParams<T>), newExp, param);

            return (CtorWithParams<T>)lambda.Compile();
        }

        internal static UnknownCtor BuildUnknownCtor(Type concreteType)
        {
            var ctor = concreteType.GetConstructor(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null, Type.EmptyTypes, null);

            if (ctor == null)
            {
                string message = "No parameterless constructor found for " + concreteType.FullName;
                return delegate
                {
                    throw new MissingConstructorException(message);
                };
            }

            var dyn = new DynamicMethod("ctorWrapper", MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard, concreteType, Type.EmptyTypes, concreteType, true);
            ILGenerator il = dyn.GetILGenerator();
            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ret);
            return (UnknownCtor)dyn.CreateDelegate(typeof(UnknownCtor));
        }

        internal static UnknownCtorWithParams BuildUnknownCtorWithParams(ConstructorInfo ctor)
        {
            var paramsInfo = ctor.GetParameters();

            var param = Expression.Parameter(typeof(object[]), "args");
            var argsExp = new Expression[paramsInfo.Length];

            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;

                Expression paramAccessorExp = Expression.ArrayIndex(param, index);
                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            var newExp = Expression.New(ctor, argsExp);
            var lambda = Expression.Lambda(typeof(UnknownCtorWithParams), newExp, param);

            return (UnknownCtorWithParams)lambda.Compile();
        }
    }

    internal static class ObjectActivator
    {
#if (NET35 || NET40)
        private readonly static LinqToLdap.Collections.SafeDictionary<string, UnknownCtorWithParams> _constructors = new LinqToLdap.Collections.SafeDictionary<string, UnknownCtorWithParams>();
#else
        private readonly static System.Collections.Concurrent.ConcurrentDictionary<string, UnknownCtorWithParams> _constructors = new System.Collections.Concurrent.ConcurrentDictionary<string, UnknownCtorWithParams>();
#endif

        public static object CreateInstance(Type instanceType, params object[] parameters)
        {
            return _constructors.GetOrAdd(instanceType.FullName, _ =>
            {
                var constrcutor = instanceType.GetConstructor(parameters.Select(x => x.GetType()).ToArray());

                return DelegateBuilder.BuildUnknownCtorWithParams(constrcutor);
            }).Invoke(parameters);
        }

        public static object CreateGenericInstance(Type instanceType, Type genericParameterType, params object[] parameters)
        {
            return _constructors.GetOrAdd(instanceType.FullName + genericParameterType.FullName, t =>
            {
                var type = instanceType.MakeGenericType(genericParameterType);
                var constrcutor = type.GetConstructor(parameters.Select(x => x.GetType()).ToArray());

                return DelegateBuilder.BuildUnknownCtorWithParams(constrcutor);
            }).Invoke(parameters);
        }
    }
}