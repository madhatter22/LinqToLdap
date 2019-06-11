using System.Collections.Generic;

namespace LinqToLdap.Collections
{
    /// <summary>
    /// Interface for a collection of items in a view.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IVirtualListView<T> : IEnumerable<T>
    {
        /// <summary>
        /// The number of items in this section of the view.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// The total number of items in the view.
        /// </summary>
        int ContentCount { get; }

        /// <summary>
        /// The context ID assigned by the server to identify this search.
        /// </summary>
        byte[] ContextId { get; }

        /// <summary>
        /// The list index position of the target entry.
        /// </summary>
        int TargetPosition { get; }
    }
}