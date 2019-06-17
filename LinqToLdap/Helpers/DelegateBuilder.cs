using LinqToLdap.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace LinqToLdap.Helpers
{
    internal delegate T CtorWithParams<out T>(params object[] args);

    internal delegate T Ctor<out T>();

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

        internal static Func<object> BuildUnknownCtor(Type concreteType)
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
            return (Func<object>)dyn.CreateDelegate(typeof(Func<object>));
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
}