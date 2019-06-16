using LinqToLdap.Helpers;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToLdap
{
    internal abstract class QueryProvider : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = TypeSystem.GetElementType(expression.Type);

            return (IQueryable)ObjectActivator.CreateGenericInstance(typeof(DirectoryQuery<>), elementType, new object[] { this, expression });
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new DirectoryQuery<TElement>(this, expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

#if (!NET35 && !NET40)

        async System.Threading.Tasks.Task<object> IAsyncQueryProvider.ExecuteAsync(Expression expression)
        {
            return await ExecuteAsync(expression);
        }

        public async System.Threading.Tasks.Task<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return (TResult)(await ExecuteAsync(expression));
        }

        public abstract System.Threading.Tasks.Task<object> ExecuteAsync(Expression expression);

#endif

        public abstract string GetQueryText(Expression expression);

        public abstract object Execute(Expression expression);
    }
}