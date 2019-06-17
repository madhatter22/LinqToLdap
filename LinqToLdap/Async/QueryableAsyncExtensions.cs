#if (!NET35 && !NET40)

using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Executes Single on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <returns></returns>
        public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source)
        {
            return Task.Factory.StartNew(() => source.Single());
        }

        /// <summary>
        /// Executes Single on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="predicate">The condition by which to filter.</param>
        /// <returns></returns>
        public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return Task.Factory.StartNew(() => source.Single(predicate));
        }

        /// <summary>
        /// Executes SingleOrDefault on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <param name="source">The query</param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            return Task.Factory.StartNew(() => source.SingleOrDefault());
        }

        /// <summary>
        /// Executes SingleOrDefault on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="predicate">The condition by which to filter.</param>
        /// <returns></returns>
        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return Task.Factory.StartNew(() => source.SingleOrDefault(predicate));
        }

        /// <summary>
        /// Executes <see cref="QueryableExtensions.ListAttributes{TSource}"/> on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="attributes">The attributes to load.</param>
        /// <returns></returns>
        public static Task<IEnumerable<KeyValuePair<string, IEnumerable<KeyValuePair<string, object>>>>> ListAttributesAsync<TSource>(this IQueryable<TSource> source, params string[] attributes)
        {
            return Task.Factory.StartNew(() => source.ListAttributes(attributes));
        }
    }
}

#endif