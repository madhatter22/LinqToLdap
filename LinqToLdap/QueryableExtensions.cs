using LinqToLdap.Collections;
using LinqToLdap.Exceptions;
using LinqToLdap.TestSupport;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqToLdap
{
    /// <summary>
    /// Contains <see cref="IQueryable{T}"/> extension methods.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Allows you to specify add <see cref="DirectoryControl"/>s to the query.
        /// </summary>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="controls">The <see cref="DirectoryControl"/>s to use.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> or <paramref name="controls"/> is null.
        /// </exception>
        /// <exception cref="FilterException">
        /// Thrown if <paramref name="controls"/> has 0 elements.
        /// </exception>
        /// <returns></returns>
        public static IQueryable<TSource> WithControls<TSource>(this IQueryable<TSource> source, IEnumerable<DirectoryControl> controls)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (controls == null) throw new ArgumentNullException("controls");

            if (!controls.Any()) throw new FilterException("Must specify at least one control when using WithControls.");

            return
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[] { source.Expression, Expression.Constant(controls) }));
        }

        /// <summary>
        /// Creates an attribute scoped query using <see cref="AsqRequestControl"/>.
        /// </summary>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="attributeName">The name of the attribute to which this query will be scoped.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is null.
        /// </exception>
        /// <returns></returns>
        public static IQueryable<TSource> ScopedToAttribute<TSource>(this IQueryable<TSource> source, string attributeName)
        {
            return source.WithControls(new[] { new AsqRequestControl(attributeName) });
        }

        /// <summary>
        /// Adds a <see cref="SearchOptionsControl"/> to the search with the specified <paramref name="option"/>.
        /// </summary>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="option">The <see cref="SearchOption"/> for the search</param>
        /// <returns></returns>
        public static IQueryable<TSource> SearchWith<TSource>(this IQueryable<TSource> source, SearchOption option)
        {
            return source.WithControls(new[] { new SearchOptionsControl(option) });
        }

        /// <summary>
        /// Lists found attributes for all entries returned by the query.
        /// </summary>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="attributes">Specify specific attributes to load</param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, IEnumerable<KeyValuePair<string, object>>>> ListAttributes<TSource>(
            this IQueryable<TSource> source, params string[] attributes)
        {
            return source.Provider.Execute<IEnumerable<KeyValuePair<string, IEnumerable<KeyValuePair<string, object>>>>>(
                Expression.Call(null,
                                ((MethodInfo)MethodBase.GetCurrentMethod())
                                    .MakeGenericMethod(new[] { typeof(TSource) }),
                                new[] { source.Expression, Expression.Constant(attributes) }));
        }

        /// <summary>
        /// Allows you to specify a custom filter for querying.
        /// </summary>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="filter">The filter to use for querying</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is null.
        /// </exception>
        /// <exception cref="FilterException">
        /// Thrown if <paramref name="filter"/> is null, empty, or white-space.
        /// </exception>
        /// <returns></returns>
        public static IQueryable<TSource> FilterWith<TSource>(this IQueryable<TSource> source, string filter)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (filter.IsNullOrEmpty()) throw new FilterException("Filters cannot be null, empty, or white-space.");

            if (!filter.StartsWith("("))
            {
                filter = "(" + filter + ")";
            }

            return
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[] { source.Expression, Expression.Constant(filter) }));
        }

        /// <summary>
        /// Allows you to specify a custom filter for querying.
        /// </summary>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="filter">The filter to use for querying</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is null.
        /// </exception>
        /// <exception cref="FilterException">
        /// Thrown if <paramref name="filter"/> is null, empty, or white-space.
        /// </exception>
        /// <returns></returns>
        public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, string filter)
        {
            return FilterWith(source, filter);
        }

        /// <summary>
        /// When this method is included in a query the object class or the object category will not be used in the filter.
        /// </summary>
        /// <param name="source">The query.</param>
        /// <typeparam name="TSource">The type to query against.</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is null.
        /// </exception>
        public static IQueryable<TSource> IgnoreOC<TSource>(this IQueryable<TSource> source)
        {
            return IgnoreOC(source, OC.Both);
        }

        /// <summary>
        /// When this method is included in a query the object class or the object category will not be used in the filter.
        /// </summary>
        /// <param name="source">The query.</param>
        /// <param name="oc">Indicates the type of <see cref="OC"/> to ignore.</param>
        /// <typeparam name="TSource">The type to query against.</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is null.
        /// </exception>
        public static IQueryable<TSource> IgnoreOC<TSource>(this IQueryable<TSource> source, OC oc)
        {
            if (source == null) throw new ArgumentNullException("source");

            return
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[] { source.Expression, Expression.Constant(oc) }));
        }

        /// <summary>
        /// When this method is included in a query the object class or the object category will not be used in the filter.
        /// </summary>
        /// <param name="source">The query.</param>
        /// <param name="oc">Indicates the type of <see cref="OC"/> to include.</param>
        /// <typeparam name="TSource">The type to query against.</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is null.
        /// </exception>
        public static IQueryable<TSource> IncludeOC<TSource>(this IQueryable<TSource> source, OC oc)
        {
            if (source == null) throw new ArgumentNullException("source");

            return
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[] { source.Expression, Expression.Constant(oc) }));
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key and matching rule.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="keySelector">The property to sort by</param>
        /// <param name="matchingRule">The LDAP matching rule to use for the query</param>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <typeparam name="TKey">The type of the property</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/>, <paramref name="matchingRule"/>, or <paramref name="keySelector"/> are null
        /// </exception>
        public static IOrderedQueryable<TSource> OrderByDescending<TSource, TKey>(this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector, string matchingRule)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (matchingRule.IsNullOrEmpty()) throw new ArgumentNullException("matchingRule");

            if (keySelector == null) throw new ArgumentNullException("keySelector");

            return (IOrderedQueryable<TSource>)
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource), typeof(TKey) }),
                                    new[] { source.Expression, keySelector, Expression.Constant(matchingRule) }));
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key and matching rule.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="keySelector">The property to sort by</param>
        /// <param name="matchingRule">The LDAP matching rule to use for the query</param>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <typeparam name="TKey">The type of the property</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/>, <paramref name="matchingRule"/>, or <paramref name="keySelector"/> are null
        /// </exception>
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector, string matchingRule)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (matchingRule.IsNullOrEmpty()) throw new ArgumentNullException("matchingRule");

            if (keySelector == null) throw new ArgumentNullException("keySelector");

            return (IOrderedQueryable<TSource>)
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource), typeof(TKey) }),
                                    new[] { source.Expression, keySelector, Expression.Constant(matchingRule) }));
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key and matching rule.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="attribute">The attribute by which to order.</param>
        /// <param name="matchingRule">The LDAP matching rule to use for the query</param>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/>, <paramref name="matchingRule"/>, or <paramref name="attribute"/> are null
        /// </exception>
        public static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IQueryable<TSource> source,
            string attribute, string matchingRule)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (matchingRule.IsNullOrEmpty()) throw new ArgumentNullException("matchingRule");
            if (attribute.IsNullOrEmpty()) throw new ArgumentNullException("attribute");

            return (IOrderedQueryable<TSource>)
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[] { source.Expression, Expression.Constant(attribute), Expression.Constant(matchingRule) }));
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key and matching rule.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="attribute">The attribute by which to sort.</param>
        /// <param name="matchingRule">The LDAP matching rule to use for the query</param>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/>, <paramref name="matchingRule"/>, or <paramref name="attribute"/> are null
        /// </exception>
        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source,
            string attribute, string matchingRule)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (matchingRule.IsNullOrEmpty()) throw new ArgumentNullException("matchingRule");

            if (attribute.IsNullOrEmpty()) throw new ArgumentNullException("attribute");

            return (IOrderedQueryable<TSource>)
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[] { source.Expression, Expression.Constant(attribute), Expression.Constant(matchingRule) }));
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key and matching rule.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="keySelector">The property to sort by</param>
        /// <param name="matchingRule">The LDAP matching rule to use for the query</param>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <typeparam name="TKey">The type of the property</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/>, <paramref name="matchingRule"/>, or <paramref name="keySelector"/> are null
        /// </exception>
        public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey>(this IOrderedQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector, string matchingRule)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (matchingRule.IsNullOrEmpty()) throw new ArgumentNullException("matchingRule");

            if (keySelector == null) throw new ArgumentNullException("keySelector");

            return (IOrderedQueryable<TSource>)
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource), typeof(TKey) }),
                                    new[] { source.Expression, keySelector, Expression.Constant(matchingRule) }));
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key and matching rule.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="keySelector">The property to sort by</param>
        /// <param name="matchingRule">The LDAP matching rule to use for the query</param>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <typeparam name="TKey">The type of the property</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/>, <paramref name="matchingRule"/>, or <paramref name="keySelector"/> are null
        /// </exception>
        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(this IOrderedQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector, string matchingRule)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (matchingRule.IsNullOrEmpty()) throw new ArgumentNullException("matchingRule");

            if (keySelector == null) throw new ArgumentNullException("keySelector");

            return (IOrderedQueryable<TSource>)
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource), typeof(TKey) }),
                                    new[] { source.Expression, keySelector, Expression.Constant(matchingRule) }));
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key and matching rule.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="attribute">The attribute by which to order.</param>
        /// <param name="matchingRule">The LDAP matching rule to use for the query</param>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/>, <paramref name="matchingRule"/>, or <paramref name="attribute"/> are null
        /// </exception>
        public static IOrderedQueryable<TSource> ThenByDescending<TSource>(this IOrderedQueryable<TSource> source,
            string attribute, string matchingRule)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (matchingRule.IsNullOrEmpty()) throw new ArgumentNullException("matchingRule");
            if (attribute.IsNullOrEmpty()) throw new ArgumentNullException("attribute");

            return (IOrderedQueryable<TSource>)
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[] { source.Expression, Expression.Constant(attribute), Expression.Constant(matchingRule) }));
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key and matching rule.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="attribute">The attribute by which to sort.</param>
        /// <param name="matchingRule">The LDAP matching rule to use for the query</param>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/>, <paramref name="matchingRule"/>, or <paramref name="attribute"/> are null
        /// </exception>
        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IOrderedQueryable<TSource> source,
            string attribute, string matchingRule)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (matchingRule.IsNullOrEmpty()) throw new ArgumentNullException("matchingRule");

            if (attribute.IsNullOrEmpty()) throw new ArgumentNullException("attribute");

            return (IOrderedQueryable<TSource>)
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[] { source.Expression, Expression.Constant(attribute), Expression.Constant(matchingRule) }));
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key and matching rule.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="attribute">The attribute by which to order.</param>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> or <paramref name="attribute"/> are null
        /// </exception>
        public static IOrderedQueryable<TSource> ThenByDescending<TSource>(this IOrderedQueryable<TSource> source, string attribute)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (attribute.IsNullOrEmpty()) throw new ArgumentNullException("attribute");

            return (IOrderedQueryable<TSource>)
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[] { source.Expression, Expression.Constant(attribute) }));
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key and matching rule.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="attribute">The attribute by which to sort.</param>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> or <paramref name="attribute"/> are null
        /// </exception>
        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IOrderedQueryable<TSource> source, string attribute)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (attribute.IsNullOrEmpty()) throw new ArgumentNullException("attribute");

            return (IOrderedQueryable<TSource>)
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[] { source.Expression, Expression.Constant(attribute) }));
        }

        /// <summary>
        /// Immediately pages the query into a <see cref="ILdapPage{T}"/> to allow manual paging.
        /// </summary>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <param name="source">The query</param>
        /// <param name="pageSize">The size of the page</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="pageSize"/> is less than one.
        /// </exception>
        /// <returns></returns>
        public static ILdapPage<TSource> ToPage<TSource>(this IQueryable<TSource> source, int pageSize)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (pageSize < 1) throw new ArgumentException("pageSize must be greater than 0", "pageSize");

            return
                source.Provider.Execute<ILdapPage<TSource>>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[] { source.Expression, Expression.Constant(pageSize) }));
        }

        /// <summary>
        /// Immediately pages the query into a <see cref="ILdapPage{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <param name="source">The query</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="nextPage">The cookie of the next page</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="pageSize"/> is less than one.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="nextPage"/> is null.
        /// </exception>
        /// <returns></returns>
        public static ILdapPage<TSource> ToPage<TSource>(this IQueryable<TSource> source, int pageSize, byte[] nextPage)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (pageSize < 1) throw new ArgumentException("pageSize must be greater than 0", "pageSize");

            if (nextPage == null) throw new ArgumentException("nextPage cannot be null when paging", "nextPage");

            return
                source.Provider.Execute<ILdapPage<TSource>>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[]
                                        {
                                            source.Expression, Expression.Constant(pageSize), Expression.Constant(nextPage)
                                        }));
        }

        /// <summary>
        /// Immediately pages the query into a <see cref="ILdapPage{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <param name="source">The query</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="nextPage">The cookie of the next page</param>
        /// <param name="filter">The filter used to build the previous page request.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="pageSize"/> is less than one.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="nextPage"/> is null.
        /// </exception>
        /// <returns></returns>
        public static ILdapPage<TSource> ToPage<TSource>(this IQueryable<TSource> source, int pageSize, byte[] nextPage, string filter)
        {
            if (source == null) throw new ArgumentNullException("source");

            if (pageSize < 1) throw new ArgumentException("pageSize must be greater than 0", "pageSize");

            if (nextPage == null) throw new ArgumentException("nextPage cannot be null when paging", "nextPage");

            if (filter.IsNullOrEmpty()) throw new ArgumentNullException("filter");

            return
                source.Provider.Execute<ILdapPage<TSource>>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[]
                                        {
                                            source.Expression, Expression.Constant(pageSize), Expression.Constant(nextPage), Expression.Constant(filter)
                                        }));
        }

        /// <summary>
        /// Performs a select projection for a dynamic query.
        /// </summary>
        /// <param name="source">The query.</param>
        /// <param name="attributes">The requested attributes.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="attributes"/> is null.</exception>
        public static IQueryable<IDirectoryAttributes> Select(this IQueryable<IDirectoryAttributes> source, params string[] attributes)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (attributes == null) throw new ArgumentNullException("attributes");

            return
                source.Provider.CreateQuery<IDirectoryAttributes>(
                    Expression.Call(null,
                                    (MethodInfo)MethodBase.GetCurrentMethod(),
                                    new[] { source.Expression, Expression.Constant(attributes) }));
        }

        /// <summary>
        /// Pages through all the results and returns them in a <see cref="List{T}"/>.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="pageSize">The size of each page</param>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="pageSize"/> is not greater than 0</exception>
        /// <returns></returns>
        public static List<TSource> InPagesOf<TSource>(this IQueryable<TSource> source, int pageSize)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (pageSize < 1) throw new ArgumentException("pageSize must be greater than 0");

            return
                source.Provider.Execute<List<TSource>>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[] { source.Expression, Expression.Constant(pageSize) }));
        }

        /// <summary>
        /// Creates a <see cref="System.Collections.Generic.List{T}"/> from an <see cref="IQueryable{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <see cref="IQueryable{T}"/> from which to create a <see cref="System.Collections.Generic.List{T}"/>.</param>
        /// <returns>A <see cref="System.Collections.Generic.List{T}"/> that contains elements from the input sequence.
        /// If this method is used with LINQ to LDAP then an <see cref="LdapPage{T}"/> may be created if <see cref="WithControls{T}"/> is used with a
        /// <see cref="PageResultRequestControl"/>.
        /// </returns>
        public static List<TSource> ToList<TSource>(this IQueryable<TSource> source)
        {
            if (source is DirectoryQuery<TSource> || source is MockQuery<TSource>)
            {
                return source.Provider.Execute<List<TSource>>(
                        Expression.Call(null,
                                        ((MethodInfo)MethodBase.GetCurrentMethod())
                                            .MakeGenericMethod(new[] { typeof(TSource) }),
                                        new[] { source.Expression }));
            }

            return Enumerable.ToList(source);
        }

        /// <summary>
        /// Builds the <see cref="SearchRequest"/> without sending it to the server.
        /// </summary>
        /// <param name="source">The query.</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is null.
        /// </exception>
        /// <returns></returns>
        public static SearchRequest GetRequest<TSource>(this IQueryable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException("source");

            return source.Provider.Execute<SearchRequest>(
                Expression.Call(null,
                                ((MethodInfo)MethodBase.GetCurrentMethod())
                                    .MakeGenericMethod(new[] { typeof(TSource) }),
                                new[] { source.Expression }));
        }

        /// <summary>
        /// Will execute the query without paging any results.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <exception cref="DirectoryOperationException">Potentially thrown if the result set exceeds the size defined by the server.</exception>
        /// <returns></returns>
        public static IQueryable<TSource> WithoutPaging<TSource>(this IQueryable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException("source");

            return
                source.Provider.CreateQuery<TSource>(
                    Expression.Call(null,
                                    ((MethodInfo)MethodBase.GetCurrentMethod())
                                        .MakeGenericMethod(new[] { typeof(TSource) }),
                                    new[] { source.Expression }));
        }
    }
}