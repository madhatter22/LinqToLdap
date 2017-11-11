using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToLdap.TestSupport
{
    /// <summary>
    /// Mock implementation of <see cref="IOrderedQueryable{T}"/> for unit testing.
    /// </summary>
    /// <typeparam name="T">Type of the query</typeparam>
    public class MockQuery<T> : IOrderedQueryable<T>
    {
        private readonly MockQueryProvider _provider;
        private readonly Expression _expression;

        /// <summary>
        /// Constructor for <see cref="MockQuery{T}"/>.
        /// </summary>
        /// <param name="resultsToReturn">The results to return in order of execution. Supports multiple result types for query reuse.  Pass null if you don't care about the results.</param>
        public MockQuery(IList<object> resultsToReturn)
        {
            _provider = new MockQueryProvider(resultsToReturn);
            _expression = Expression.Constant(this);
        }

        internal MockQuery(MockQueryProvider provider)
        {
            _provider = provider;
            _expression = Expression.Constant(this);
        }

        internal MockQuery(MockQueryProvider provider, Expression expression)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            if (expression == null) throw new ArgumentNullException("expression");

            _provider = provider;
            _expression = expression;
        }

        /// <summary>
        /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.Expressions.Expression"/> that is associated with this instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </returns>
        public Expression Expression
        {
            get { return _expression; }
        }

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
        /// </returns>
        public Type ElementType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source. This is the same instance as <see cref="MockProvider"/>.
        /// </returns>
        public IQueryProvider Provider
        {
            get { return MockProvider; }
        }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.MockQueryProvider"/> that is associated with this data source. This is the same instance as <see cref="Provider"/>.
        /// </returns>
        public MockQueryProvider MockProvider
        {
            get { return _provider; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            var enumerable = (IEnumerable<T>) _provider.Execute(_expression);
            return enumerable != null ? enumerable.GetEnumerator() : new List<T>().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}