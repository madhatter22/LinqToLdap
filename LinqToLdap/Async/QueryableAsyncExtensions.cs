#if (!NET35 && !NET40)

using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace LinqToLdap.Async
{
    /// <summary>
    /// Async extension methids for <see cref="IDirectoryContext"/>.
    /// </summary>
    public static class QueryableAsyncExtensions
    {
        private static readonly MethodInfo AnyAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "AnyAsync" && x.GetParameters().Length == 2);
        private static readonly MethodInfo AnyPredicateAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "AnyAsync" && x.GetParameters().Length == 3);
        private static readonly MethodInfo ToListAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "ToListAsync" && x.GetParameters().Length == 2);
        private static readonly MethodInfo CountAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "CountAsync" && x.GetParameters().Length == 2);
        private static readonly MethodInfo CountPredicateAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "CountAsync" && x.GetParameters().Length == 3);
        private static readonly MethodInfo LongCountAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "LongCountAsync" && x.GetParameters().Length == 2);
        private static readonly MethodInfo LongCountPredicateAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "LongCountAsync" && x.GetParameters().Length == 3);
        private static readonly MethodInfo FirstAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "FirstAsync" && x.GetParameters().Length == 2);
        private static readonly MethodInfo FirstPredicateAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "FirstAsync" && x.GetParameters().Length == 3);
        private static readonly MethodInfo FirstOrDefaultAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "FirstOrDefaultAsync" && x.GetParameters().Length == 2);
        private static readonly MethodInfo FirstOrDefaultPredicateAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "FirstOrDefaultAsync" && x.GetParameters().Length == 3);
        private static readonly MethodInfo SingleAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "SingleAsync" && x.GetParameters().Length == 2);
        private static readonly MethodInfo SinglePredicateAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "SingleAsync" && x.GetParameters().Length == 3);
        private static readonly MethodInfo SingleOrDefaultAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "SingleOrDefaultAsync" && x.GetParameters().Length == 2);
        private static readonly MethodInfo SingleOrDefaultPredicateAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "SingleOrDefaultAsync" && x.GetParameters().Length == 3);
        private static readonly MethodInfo ListAttributesAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "ListAttributesAsync" && x.GetParameters().Length == 3);
        private static readonly MethodInfo InPagesOfAsyncMethod = typeof(QueryableAsyncExtensions).GetMethods().Single(x => x.Name == "InPagesOfAsync" && x.GetParameters().Length == 3);

        /// <summary>
        /// Executes Any on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <param name="source">The query.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <typeparam name="TSource">The element type to return.</typeparam>
        /// <returns></returns>
        public static async Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<bool>(
                    Expression.Call(null, AnyAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.Any();
        }

        /// <summary>
        /// Executes Any on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <param name="source">The query.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The element type to return.</typeparam>
        /// <returns></returns>
        public static async Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<bool>(
                    Expression.Call(null, AnyPredicateAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, predicate, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.Any(predicate);
        }

        /// <summary>
        /// Executes <see cref="QueryableExtensions.ToList{TSource}"/> on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        public static async Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<List<TSource>>(
                    Expression.Call(null, ToListAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.ToList();
        }

        /// <summary>
        /// Executes Count on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        public static async Task<int> CountAsync<TSource>(this IQueryable<TSource> source, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<int>(
                    Expression.Call(null, CountAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.Count();
        }

        /// <summary>
        /// Executes Count on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// /// <param name="predicate">The condition by which to filter.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        public static async Task<int> CountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<int>(
                    Expression.Call(null, CountPredicateAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, predicate, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.Count(predicate);
        }

        /// <summary>
        /// Executes LongCount on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        public static async Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<long>(
                    Expression.Call(null, LongCountAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.LongCount();
        }

        /// <summary>
        /// Executes LongCount on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// /// <param name="predicate">The condition by which to filter.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        public static async Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<long>(
                    Expression.Call(null, LongCountPredicateAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, predicate, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.LongCount(predicate);
        }

        /// <summary>
        /// Executes FirstOrDefault on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <param name="source">The query.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <typeparam name="TSource">The element type to return.</typeparam>
        /// <returns></returns>
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<TSource>(
                    Expression.Call(null, FirstOrDefaultAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.FirstOrDefault();
        }

        /// <summary>
        /// Executes FirstOrDefault on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <typeparam name="TSource">The element type to return.</typeparam>
        /// <returns></returns>
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<TSource>(
                    Expression.Call(null, FirstOrDefaultPredicateAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, predicate, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Executes First on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        public static async Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<TSource>(
                    Expression.Call(null, FirstAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.First();
        }

        /// <summary>
        /// Executes First on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="predicate">The condition by which to filter.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        public static async Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<TSource>(
                    Expression.Call(null, FirstPredicateAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, predicate, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.First(predicate);
        }

        /// <summary>
        /// Executes Single on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        public static async Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<TSource>(
                    Expression.Call(null, SingleAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.Single();
        }

        /// <summary>
        /// Executes Single on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="predicate">The condition by which to filter.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <returns></returns>
        public static async Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<TSource>(
                    Expression.Call(null, SinglePredicateAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, predicate, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.Single(predicate);
        }

        /// <summary>
        /// Executes SingleOrDefault on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static async Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<TSource>(
                    Expression.Call(null, SingleOrDefaultAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.SingleOrDefault();
        }

        /// <summary>
        /// Executes SingleOrDefault on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <param name="predicate">The condition by which to filter.</param>
        /// <returns></returns>
        public static async Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<TSource>(
                    Expression.Call(null, SingleOrDefaultPredicateAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, predicate, Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.SingleOrDefault(predicate);
        }

        /// <summary>
        /// Executes <see cref="QueryableExtensions.ListAttributes{TSource}"/> on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <param name="attributes">The attributes to load.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<KeyValuePair<string, IEnumerable<KeyValuePair<string, object>>>>> ListAttributesAsync<TSource>(this IQueryable<TSource> source, string[] attributes = null, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<IEnumerable<KeyValuePair<string, IEnumerable<KeyValuePair<string, object>>>>>(
                    Expression.Call(null, ListAttributesAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, Expression.Constant(attributes ?? new string[0]), Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }
            return source.ListAttributes();
        }

        /// <summary>
        /// Pages through all the results and returns them in a <see cref="List{T}"/>.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="pageSize">The size of each page</param>
        /// <param name="resultProcessing">How the async results are processed</param>
        /// <typeparam name="TSource">The type to query against</typeparam>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="pageSize"/> is not greater than 0</exception>
        /// <returns></returns>
        public static async Task<List<TSource>> InPagesOfAsync<TSource>(this IQueryable<TSource> source, int pageSize, PartialResultProcessing resultProcessing = LdapConfiguration.DefaultAsyncResultProcessing)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (pageSize < 1) throw new ArgumentException("pageSize must be greater than 0");

            if (source.Provider is IAsyncQueryProvider asyncProvider)
            {
                return await asyncProvider.ExecuteAsync<List<TSource>>(
                    Expression.Call(null, InPagesOfAsyncMethod.MakeGenericMethod(
                        new[] { typeof(TSource) }),
                        new[] { source.Expression, Expression.Constant(pageSize), Expression.Constant(resultProcessing) })).ConfigureAwait(false);
            }

            return source.InPagesOf(pageSize);
        }
    }
}

#endif