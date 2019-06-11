#if !NET35

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LinqToLdap.Contrib
{
    /// <summary>
    /// Async extension methids for <see cref="IDirectoryContext"/>.
    /// </summary>
    public static class QueryableAsyncExtensions
    {
        /// <summary>
        /// Executes FirstOrDefault on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <param name="source">The query.</param>
        /// <typeparam name="TSource">The element type to return.</typeparam>
        /// <returns></returns>
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            return Task.Factory.StartNew(() => source.FirstOrDefault());
        }

        /// <summary>
        /// Executes FirstOrDefault on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <param name="source">The query</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The element type to return.</typeparam>
        /// <returns></returns>
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return Task.Factory.StartNew(() => source.FirstOrDefault(predicate));
        }

        /// <summary>
        /// Executes First on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <returns></returns>
        public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source)
        {
            return Task.Factory.StartNew(() => source.First());
        }

        /// <summary>
        /// Executes First on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <param name="predicate">The condition by which to filter.</param>
        /// <returns></returns>
        public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return Task.Factory.StartNew(() => source.First(predicate));
        }

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
        /// Executes <see cref="QueryableExtensions.ToList{TSource}"/> on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <returns></returns>
        public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source)
        {
            return Task.Factory.StartNew(() => source.ToList());
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

        /// <summary>
        /// Executes Count on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <returns></returns>
        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source)
        {
            return Task.Factory.StartNew(() => source.Count());
        }

        /// <summary>
        /// Executes Count on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// /// <param name="predicate">The condition by which to filter.</param>
        /// <returns></returns>
        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return Task.Factory.StartNew(() => source.Count(predicate));
        }

        /// <summary>
        /// Executes LongCount on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// <returns></returns>
        public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source)
        {
            return Task.Factory.StartNew(() => source.LongCount());
        }

        /// <summary>
        /// Executes LongCount on <paramref name="source"/> in a <see cref="Task"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The query.</param>
        /// /// <param name="predicate">The condition by which to filter.</param>
        /// <returns></returns>
        public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return Task.Factory.StartNew(() => source.LongCount(predicate));
        }
    }
}

#endif