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
using System.Linq.Expressions;

namespace LinqToLdap
{
    /// <summary>
    /// Class for specifying custom filters
    /// </summary>
    public static class Filter
    {
        /// <summary>
        /// Creates an equals filter.
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="attributeName">The LDAP attribute name</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool Equal<T>(T t, string attributeName, string attributeValue, bool shouldCleanValue)
        { 
            return true;
        }

        /// <summary>
        /// Creates an equals filter.
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="property">The property</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool Equal<T, TProperty>(T t, Expression<Func<T, TProperty>> property, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates an approximately equal filter.
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="attributeName">The LDAP attribute name</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool Approximately<T>(T t, string attributeName, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates an approximately equal filter.
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="property">The property</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool Approximately<T, TProperty>(T t, Expression<Func<T, TProperty>> property, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a starts with filter
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="property">The property</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool StartsWith<T, TProperty>(T t, Expression<Func<T, TProperty>> property, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a starts with filter
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="attributeName">The LDAP attribute name</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool StartsWith<T>(T t, string attributeName, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a ends with filter
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="property">The property</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool EndsWith<T, TProperty>(T t, Expression<Func<T, TProperty>> property, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a ends with filter
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="attributeName">The LDAP attribute name</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool EndsWith<T>(T t, string attributeName, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a like filter
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="property">The property</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool Like<T, TProperty>(T t, Expression<Func<T, TProperty>> property, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a like filter
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="attributeName">The LDAP attribute name</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool Like<T>(T t, string attributeName, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a greater than or equal filter
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="property">The property</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool GreaterThanOrEqual<T, TProperty>(T t, Expression<Func<T, TProperty>> property, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a greater than or equal filter
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="attributeName">The LDAP attribute name</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool GreaterThanOrEqual<T>(T t, string attributeName, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a less than or equal filter
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="property">The property</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool LessThanOrEqual<T, TProperty>(T t, Expression<Func<T, TProperty>> property, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a less than or equal filter
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="attributeName">The LDAP attribute name</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool LessThanOrEqual<T>(T t, string attributeName, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a less than filter
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="property">The property</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool LessThan<T, TProperty>(T t, Expression<Func<T, TProperty>> property, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a less than filter
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="attributeName">The LDAP attribute name</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool LessThan<T>(T t, string attributeName, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a greater than filter
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="property">The property</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool GreaterThan<T, TProperty>(T t, Expression<Func<T, TProperty>> property, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a greater than filter
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="attributeName">The LDAP attribute name</param>
        /// <param name="attributeValue">The value by which to filter</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="attributeValue"/> should be cleaned</param>
        /// <returns></returns>
        public static bool GreaterThan<T>(T t, string attributeName, string attributeValue, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates an equals filter.
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="attributeName">The LDAP attribute name</param>
        /// <param name="values">The values to filter for</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="values"/> should be cleaned</param>
        /// <returns></returns>
        public static bool EqualAny<T>(T t, string attributeName, IEnumerable<string> values, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates an equals filter.
        /// </summary>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="property">The property</param>
        /// <param name="values">The values to filter for</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="values"/> should be cleaned</param>
        /// <returns></returns>
        public static bool EqualAny<T, TProperty>(T t, Expression<Func<T, TProperty>> property, IEnumerable<string> values, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates an equal anything filter
        /// </summary>
        /// <example>
        /// x=*
        /// </example>
        /// <typeparam name="T">Type for expression translation</typeparam>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="attributeName">The LDAP attribute name</param>
        /// <returns></returns>
        public static bool EqualAnything<T>(T t, string attributeName)
        {
            return true;
        }

        /// <summary>
        /// Creates an equal anything filter
        /// </summary>
        /// <example>
        /// x=*
        /// </example>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="property">The property</param>
        /// <returns></returns>
        public static bool EqualAnything<T, TProperty>(T t, Expression<Func<T, TProperty>> property)
        {
            return true;
        }

        /// <summary>
        /// Creates a matching rule filter
        /// </summary>
        /// <example>
        /// attributename:ruleOID:=value
        /// </example>
        /// <typeparam name="T">Type for expression translation</typeparam>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="attributeName">The LDAP attribute name</param>
        /// <param name="ruleOID">The LDAP matching rule</param>
        /// <param name="value">The value</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="value"/> should be cleaned</param>
        /// <returns></returns>
        public static bool MatchingRule<T>(T t, string attributeName, string ruleOID, string value, bool shouldCleanValue)
        {
            return true;
        }

        /// <summary>
        /// Creates a matching rule filter
        /// </summary>
        /// <example>
        /// attributename:ruleOID:=value
        /// </example>
        /// <param name="t">Instance for expression translation</param>
        /// <param name="property">The property</param>
        /// <param name="ruleOID">The LDAP matching rule</param>
        /// <param name="value">The value</param>
        /// <param name="shouldCleanValue">Indicates if <paramref name="value"/> should be cleaned</param>
        /// <returns></returns>
        public static bool MatchingRule<T, TProperty>(T t, Expression<Func<T, TProperty>> property, string ruleOID, string value, bool shouldCleanValue)
        {
            return true;
        }
    }
}
