using LinqToLdap.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqToLdap.TestSupport
{
    /// <summary>
    /// A mock <see cref="IQueryProvider"/>.
    /// </summary>
    public class MockQueryProvider : IAsyncQueryProvider
    {
        private int _executionCount;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resultsToReturn">The results to return with the query is executed.</param>
        public MockQueryProvider(IList<object> resultsToReturn)
        {
            ResultsToReturn = resultsToReturn ?? new List<object>();
            ExecutedExpressions = new List<Expression>();
        }

        /// <summary>
        /// The results to return when the provider is executed.
        /// </summary>
        public IList<object> ResultsToReturn { get; private set; }

        /// <summary>
        /// The last expression passed into <see cref="CreateQuery"/>, <see cref="CreateQuery{T}"/>, <see cref="Execute"/>, or <see cref="Execute{T}"/>.
        /// </summary>
        public Expression CurrentExpression { get; private set; }

        /// <summary>
        /// All expressions executed by this provider. The most recent expression is at the end of the list.
        /// </summary>
        public IList<Expression> ExecutedExpressions { get; private set; }

        /// <summary>
        /// Creates a new query with the expression and sets <see cref="CurrentExpression"/> equal to <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns></returns>
        public IQueryable CreateQuery(Expression expression)
        {
            CurrentExpression = expression;
            var elementType = TypeSystem.GetElementType(expression.Type);

            var constructor = typeof(MockQuery<>)
                .MakeGenericType(elementType)
                .GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new[] { typeof(MockQueryProvider), typeof(Expression) }, null);

            return (IQueryable)constructor.Invoke(new object[] { this, expression });
        }

        /// <summary>
        /// Creates a new query with the expression and sets <see cref="CurrentExpression"/> equal to <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="TElement">The return type of the query.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            CurrentExpression = expression;
            return new MockQuery<TElement>(this, expression);
        }

        /// <summary>
        /// Sets <see cref="CurrentExpression"/> equal to <paramref name="expression"/> and adds it to <see cref="ExecutedExpressions"/>. Returns the a result from <see cref="ResultsToReturn"/> based on the number of execution calls that have occurred.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns></returns>
        public object Execute(Expression expression)
        {
            CurrentExpression = expression;
            ExecutedExpressions.Add(expression);
            return ResultsToReturn.Count == 0
                ? Activator.CreateInstance(TypeSystem.GetElementType(expression.Type))
                : ResultsToReturn[_executionCount++];
        }

        /// <summary>
        /// Sets <see cref="CurrentExpression"/> equal to <paramref name="expression"/> and adds it to <see cref="ExecutedExpressions"/>. Returns the a result from <see cref="ResultsToReturn"/> based on the number of execution calls that have occurred.
        /// </summary>
        /// <typeparam name="TResult">The type of the result</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public TResult Execute<TResult>(Expression expression)
        {
            CurrentExpression = expression;
            ExecutedExpressions.Add(expression);
            return ResultsToReturn.Count == 0
                ? default
                : (TResult)ResultsToReturn[_executionCount++];
        }

#if !NET35 && !NET40

        /// <summary>
        /// Sets <see cref="CurrentExpression"/> equal to <paramref name="expression"/> and adds it to <see cref="ExecutedExpressions"/>. Returns the a result from <see cref="ResultsToReturn"/> based on the number of execution calls that have occurred.
        /// </summary>
        /// <param name="expression">The expression</param>
        /// <returns></returns>
        public System.Threading.Tasks.Task<object> ExecuteAsync(Expression expression)
        {
            return System.Threading.Tasks.Task.FromResult(Execute(expression));
        }

        /// <summary>
        /// Sets <see cref="CurrentExpression"/> equal to <paramref name="expression"/> and adds it to <see cref="ExecutedExpressions"/>. Returns the a result from <see cref="ResultsToReturn"/> based on the number of execution calls that have occurred.
        /// </summary>
        /// <typeparam name="TResult">The type of the result</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public System.Threading.Tasks.Task<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return System.Threading.Tasks.Task.FromResult(Execute<TResult>(expression));
        }

#endif
    }
}