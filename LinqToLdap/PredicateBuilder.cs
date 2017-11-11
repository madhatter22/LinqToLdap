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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LinqToLdap.Collections;
using LinqToLdap.Helpers;
using LinqToLdap.Visitors;

namespace LinqToLdap
{
    /// <summary>
    /// Class containing useful methods for expression building.
    /// </summary>
    public static class PredicateBuilder
    {
        /// <summary>
        /// Gets the property name from the member expression.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="TProperty">Property type</typeparam>
        /// <param name="property">The expression</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="property"/> is not a valid member expression.</exception>
        public static string GetPropertyName<T, TProperty>(this Expression<Func<T, TProperty>> property)
        {
            var member = new MemberVisitor().GetMember(property) as MemberExpression;
            if (member == null) throw new ArgumentException("Could not get MemberExpression", "property");

            return member.Member.Name;
        }

        /// <summary>
        /// Builds a property getter expression.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="propertyName">THe property name</param>
        /// <returns></returns>
        public static Expression<Func<T, object>> GetPropertyExpression<T>(string propertyName)
        {
            if (propertyName.IsNullOrEmpty()) throw new ArgumentNullException("propertyName");

            var member = typeof (T).GetMember(propertyName).FirstOrDefault();
            if (member == null) throw new ArgumentException(
                    string.Format("Member {0} not found for Type {1}", propertyName, typeof (T).FullName));

            var instance = Expression.Parameter(typeof(T), "i");
            var memberAccess = Expression.MakeMemberAccess(instance, member);
            var convert = Expression.Convert(memberAccess, typeof (object));
            var getter = (Expression<Func<T, object>>)Expression.Lambda(convert, instance);
            return getter;
        }

        //public static Expression<Func<T, object>> GetPropertyExpression<T>(this T example, string propertyName)
        //{
        //    return GetPropertyExpression<T>(propertyName);
        //}

        /// <summary>
        /// Allows for easy inline expression creation.  The returned expression is null.  
        /// This method only serves as a quick starting point for building predicates.
        /// </summary>
        /// <typeparam name="T">Expression parameter</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Create<T>()
        {
            return default(Expression<Func<T, bool>>);
        }

        /// <summary>
        /// Allows for easy inline expression creation.  The returned expression is null.  
        /// This method only serves as a quick starting point for building predicates.
        /// </summary>
        /// <typeparam name="T">Expression parameter</typeparam>
        /// <typeparam name="TResult">Return type of expression</typeparam>
        /// <param name="example">Anonymous object to use for <typeparamref name="T"/></param>
        /// <param name="expression">Expression to return</param>
        /// <returns></returns>
        public static Expression<Func<T, TResult>> CreateExpression<T, TResult>(this T example, 
            Expression<Func<T, TResult>> expression)
        {
            return expression;
        }

        /// <summary>
        /// Allows for dynamic Or chaining of expressions.  <paramref name="appendExpression"/> 
        /// is returned if <paramref name="sourceExpression"/> is null.
        /// </summary>
        /// <typeparam name="T">Expression parameter</typeparam>
        /// <param name="sourceExpression">The original expression</param>
        /// <param name="appendExpression">The expression to chain</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> sourceExpression,
                                                      Expression<Func<T, bool>> appendExpression)
        {
            if (sourceExpression == null)
            {
                return appendExpression;
            }
#if NET35
            var invokedExpr = Expression.Invoke(appendExpression, sourceExpression.Parameters.Cast<Expression>());
#else
            var invokedExpr = Expression.Invoke(appendExpression, sourceExpression.Parameters);
#endif
            return Expression.Lambda<Func<T, bool>>(
                Expression.OrElse(sourceExpression.Body, invokedExpr), sourceExpression.Parameters);
        }

        /// <summary>
        /// Allows for dynamic And chaining of expressions.  <paramref name="appendExpression"/> 
        /// is returned if <paramref name="sourceExpression"/> is null.
        /// </summary>
        /// <typeparam name="T">Expression parameter</typeparam>
        /// <param name="sourceExpression">The original expression</param>
        /// <param name="appendExpression">The expression to chain</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> sourceExpression,
                                                             Expression<Func<T, bool>> appendExpression)
        {
            if (sourceExpression == null)
            {
                return appendExpression;
            }
#if NET35
            var invokedExpr = Expression.Invoke(appendExpression, sourceExpression.Parameters.Cast<Expression>());
#else
            var invokedExpr = Expression.Invoke(appendExpression, sourceExpression.Parameters);
#endif

            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(sourceExpression.Body, invokedExpr), sourceExpression.Parameters);
        }

        #region OrderBy //pulled from stackoverflow

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key. 
        /// </summary>
        /// <param name="source">Query source</param>
        /// <param name="propertyName">The name of the property to sort by</param>
        /// <typeparam name="T">Type to query against</typeparam>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            //Safe to remove type check code. Specifically present to handle LINQ to LDAP.
            if (typeof(T) == typeof(IDirectoryAttributes))
            {
                return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(
                            Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
                                new[]
                                {
                                    source.Expression,
                                    Expression.Constant(propertyName)
                                }));
            }
            return ApplyOrder(source, propertyName, "OrderBy");
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key. 
        /// </summary>
        /// <param name="source">Query source</param>
        /// <param name="propertyName">The name of the property to sort by</param>
        /// <typeparam name="T">Type to query against</typeparam>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            //Safe to remove type check code. Specifically present to handle LINQ to LDAP.
            if (typeof(T) == typeof(IDirectoryAttributes))
            {
                return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(
                            Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T)),
                                new[]
                                {
                                    source.Expression,
                                    Expression.Constant(propertyName)
                                }));
            }
            return ApplyOrder(source, propertyName, "OrderByDescending");
        }

        private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            var pair = GetParameterExpression(typeof(T), property);
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), pair.Item2);
            LambdaExpression lambda = Expression.Lambda(delegateType, pair.Item3, pair.Item1);
            object result = typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                    && method.IsGenericMethodDefinition
                    && method.GetGenericArguments().Length == 2
                    && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), pair.Item2)
                    .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }

        #endregion OrderBy

        #region Support methods

        private static ConstantExpression GetConstantExpression(Type propertyType, object propertyValue)
        {
            ConstantExpression constant;
            if (propertyValue == null)
            {
                constant = Expression.Constant(null);
            }
            else
            {
                Type propertyValueType;
                if (propertyType != typeof(string) && propertyValue is string)
                {
                    if (propertyType.IsEnum)
                    {
                        propertyValue = Enum.Parse(propertyType, propertyValue as string, true);
                        propertyValueType = propertyType;
                    }
                    else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var nullableType = propertyType.GetGenericArguments()[0];
                        propertyValueType = typeof(Nullable<>).MakeGenericType(nullableType);
                        propertyValue = nullableType.IsEnum
                                       ? Enum.Parse(nullableType, propertyValue as string)
                                       : Convert.ChangeType(propertyValue, nullableType);
                    }
                    else
                    {
                        propertyValue = Convert.ChangeType(propertyValue, propertyType);
                        propertyValueType = propertyType;
                    }
                }
                else
                {
                    propertyValueType = propertyValue.GetType();
                }
                constant = Expression.Constant(propertyValue, propertyValueType);
            }

            return constant;
        }
#if NET35
        private static ThreeTuple<ParameterExpression, Type, Expression> GetParameterExpression(
#else
        private static Tuple<ParameterExpression, Type, Expression> GetParameterExpression(
#endif
            Type type, string propertyName)
        {
            string[] props = propertyName.Split('.');
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            Type propertyType = type;
            foreach (string prop in props)
            {
                PropertyInfo pi = propertyType.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                propertyType = pi.PropertyType;
            }
#if NET35
            return new ThreeTuple<ParameterExpression, Type, Expression>(arg, propertyType, expr);
#else
            return new Tuple<ParameterExpression, Type, Expression>(arg, propertyType, expr);
#endif
        }

        #endregion Support Methods

        #region Where Expressions

        /// <summary>
        /// Build a dynamic equal expression from the property name and a value.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereEqual<T>(string propertyName, object propertyValue)
        {
            var pair = GetParameterExpression(typeof(T), propertyName);
            ConstantExpression con = GetConstantExpression(pair.Item2, propertyValue);
            BinaryExpression be = Expression.Equal(pair.Item3, con);
            return Expression.Lambda<Func<T, Boolean>>(be, pair.Item1);
        }

        /// <summary>
        /// Build a dynamic equal expression from the property name and a value.
        /// </summary>
        /// <param name="example">An example for <typeparamref name="T"/></param>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereEqual<T>(this T example, string propertyName, object propertyValue)
        {
            return WhereEqual<T>(propertyName, propertyValue);
        }

        /// <summary>
        /// Build a dynamic not equal expression from the property name and a value.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereNotEqual<T>(string propertyName, object propertyValue)
        {
            var pair = GetParameterExpression(typeof(T), propertyName);
            ConstantExpression con = GetConstantExpression(pair.Item2, propertyValue);
            BinaryExpression be = Expression.NotEqual(pair.Item3, con);
            return Expression.Lambda<Func<T, Boolean>>(be, pair.Item1);
        }

        /// <summary>
        /// Build a dynamic not equal expression from the property name and a value.
        /// </summary>
        /// <param name="example">An example for <typeparamref name="T"/></param>.
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereNotEqual<T>(this T example, string propertyName, object propertyValue)
        {
            return WhereNotEqual<T>(propertyName, propertyValue);
        }

        /// <summary>
        /// Build a dynamic greater than expression from the property name and a value.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereGreaterThan<T>(string propertyName, object propertyValue)
        {
            var pair = GetParameterExpression(typeof(T), propertyName);
            ConstantExpression con = GetConstantExpression(pair.Item2, propertyValue);
            BinaryExpression be = Expression.GreaterThan(pair.Item3, con);
            return Expression.Lambda<Func<T, Boolean>>(be, pair.Item1);
        }

        /// <summary>
        /// Build a dynamic greater than expression from the property name and a value.
        /// </summary>
        /// <param name="example">An example for <typeparamref name="T"/></param>.
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereGreaterThan<T>(this T example, string propertyName, object propertyValue)
        {
            return WhereGreaterThan<T>(propertyName, propertyValue);
        }

        /// <summary>
        /// Build a dynamic greater than or equal expression from the property name and a value.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereGreaterThanOrEqual<T>(string propertyName, object propertyValue)
        {
            var pair = GetParameterExpression(typeof(T), propertyName);
            ConstantExpression con = GetConstantExpression(pair.Item2, propertyValue);
            BinaryExpression be = Expression.GreaterThanOrEqual(pair.Item3, con);
            return Expression.Lambda<Func<T, Boolean>>(be, pair.Item1);
        }

        /// <summary>
        /// Build a dynamic greater than or equal expression from the property name and a value.
        /// </summary>
        /// <param name="example">An example for <typeparamref name="T"/></param>.
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereGreaterThanOrEqual<T>(this T example, string propertyName, object propertyValue)
        {
            return WhereGreaterThanOrEqual<T>(propertyName, propertyValue);
        }

        /// <summary>
        /// Build a dynamic less than expression from the property name and a value.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereLessThan<T>(string propertyName, object propertyValue)
        {
            var pair = GetParameterExpression(typeof(T), propertyName);
            ConstantExpression con = GetConstantExpression(pair.Item2, propertyValue);
            BinaryExpression be = Expression.LessThan(pair.Item3, con);
            return Expression.Lambda<Func<T, Boolean>>(be, pair.Item1);
        }

        //public static Expression<Func<T, bool>> WhereLessThan<T>(this T example, string propertyName, object propertyValue)
        //{
        //    return WhereLessThan<T>(propertyName, propertyValue);
        //}

        /// <summary>
        /// Build a dynamic less than or equal expression from the property name and a value.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereLessThanOrEqual<T>(string propertyName, object propertyValue)
        {
            var pair = GetParameterExpression(typeof(T), propertyName);
            ConstantExpression con = GetConstantExpression(pair.Item2, propertyValue);
            BinaryExpression be = Expression.LessThanOrEqual(pair.Item3, con);
            return Expression.Lambda<Func<T, Boolean>>(be, pair.Item1);
        }

        //public static Expression<Func<T, bool>> WhereLessThanOrEqual<T>(this T example, string propertyName, object propertyValue)
        //{
        //    return WhereLessThanOrEqual<T>(propertyName, propertyValue);
        //}

        private enum MatchMode { Contains, StartsWith, EndsWith }
        private static Expression<Func<T, Boolean>> GetLikeExpression<T>(string propertyName, string propertyValue,
            MatchMode mode, Boolean not)
        {
            var pair = GetParameterExpression(typeof(T), propertyName);
            ConstantExpression con = GetConstantExpression(pair.Item2, propertyValue);
            MethodInfo info;
            switch (mode)
            {
                case MatchMode.Contains:
                    info = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    break;
                case MatchMode.EndsWith:
                    info = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                    break;
                case MatchMode.StartsWith:
                    info = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                    break;
                default:
                    throw new ArgumentException(string.Format("'{0}' is not supported.", mode), "mode");
            }

// ReSharper disable PossiblyMistakenUseOfParamsMethod
            Expression e = Expression.Call(pair.Item3, info, con);
// ReSharper restore PossiblyMistakenUseOfParamsMethod
            return Expression.Lambda<Func<T, Boolean>>(not ? Expression.Not(e) : e, pair.Item1);
        }

        /// <summary>
        /// Build a dynamic string contains expression from the property name and a value.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereLike<T>(string propertyName, string propertyValue)
        {
            return GetLikeExpression<T>(propertyName, propertyValue, MatchMode.Contains, false);
        }

        //public static Expression<Func<T, bool>> WhereLike<T>(this T example, string propertyName, string propertyValue)
        //{
        //    return WhereLike<T>(propertyName, propertyValue);
        //}

        /// <summary>
        /// Build a dynamic not string contains expression from the property name and a value.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereNotLike<T>(string propertyName, string propertyValue)
        {
            return GetLikeExpression<T>(propertyName, propertyValue, MatchMode.Contains, true);
        }

        //public static Expression<Func<T, bool>> WhereNotLike<T>(this T example, string propertyName, string propertyValue)
        //{
        //    return WhereNotLike<T>(propertyName, propertyValue);
        //}

        /// <summary>
        /// Build a dynamic string starts with expression from the property name and a value.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereStartsWith<T>(string propertyName, string propertyValue)
        {
            return GetLikeExpression<T>(propertyName, propertyValue, MatchMode.StartsWith, false);
        }

        //public static Expression<Func<T, bool>> WhereStartsWith<T>(this T example, string propertyName, string propertyValue)
        //{
        //    return WhereStartsWith<T>(propertyName, propertyValue);
        //}

        /// <summary>
        /// Build a dynamic not string starts with expression from the property name and a value.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereNotStartsWith<T>(string propertyName, string propertyValue)
        {
            return GetLikeExpression<T>(propertyName, propertyValue, MatchMode.StartsWith, true);
        }

        //public static Expression<Func<T, bool>> WhereNotStartsWith<T>(this T example, string propertyName, string propertyValue)
        //{
        //    return WhereNotStartsWith<T>(propertyName, propertyValue);
        //}

        /// <summary>
        /// Build a dynamic string ends with expression from the property name and a value.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereEndsWith<T>(string propertyName, string propertyValue)
        {
            return GetLikeExpression<T>(propertyName, propertyValue, MatchMode.EndsWith, false);
        }

        //public static Expression<Func<T, bool>> WhereEndsWith<T>(this T example, string propertyName, string propertyValue)
        //{
        //    return GetLikeExpression<T>(propertyName, propertyValue, MatchMode.EndsWith, false);
        //}

        /// <summary>
        /// Build a dynamic not string ends with expression from the property name and a value.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereNotEndsWith<T>(string propertyName, string propertyValue)
        {
            return GetLikeExpression<T>(propertyName, propertyValue, MatchMode.EndsWith, true);
        }

        /// <summary>
        /// Builds a contains expression for collections using <see cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)"/>
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereContains<T>(string propertyName, object propertyValue)
        {
            return GetContainsExpression<T>(propertyName, propertyValue, false);
        }

        /// <summary>
        /// Builds a not contains expression for collections using <see cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)"/>
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="propertyValue">The value for the expression.</param>
        /// <typeparam name="T">The type for the expression</typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereNotContains<T>(string propertyName, object propertyValue)
        {
            return GetContainsExpression<T>(propertyName, propertyValue, true);
        }

        //public static Expression<Func<T, bool>> WhereContains<T>(this T example, string propertyName, object propertyValue)
        //{
        //    return WhereContains<T>(propertyName, propertyValue);
        //}

        //public static Expression<Func<T, bool>> WhereNotContains<T>(this T example, string propertyName, object propertyValue)
        //{
        //    return WhereNotContains<T>(propertyName, propertyValue);
        //}

        private static Expression<Func<T, bool>> GetContainsExpression<T>(string propertyName, object parameterValue, bool not)
        {
            var tuple = GetParameterExpression(typeof(T), propertyName);
            var genericArguments = tuple.Item2.GetGenericArguments();
            if (genericArguments == null || genericArguments.Length != 1)
                throw new ArgumentException(
                    "This method expects an enumerable class with a Contains method.");
            ConstantExpression con = GetConstantExpression(genericArguments[0], parameterValue);

            MethodInfo info = typeof(Enumerable).GetMethods()
                                   .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                                   .MakeGenericMethod(genericArguments);
            //MethodInfo info = pair.Item2.GetMethod("Contains", new[] {pair.Item2});

            Expression call = Expression.Call(null, info, tuple.Item3, con);
            var lambda = Expression.Lambda<Func<T, bool>>(not ? Expression.Not(call) : call, tuple.Item1);
            return lambda;
        }

        #endregion Where Expressions

        
    }
}
