using System.Collections;
using System.Linq.Expressions;

namespace LinqToLdap.Visitors
{
    internal class EnumerableVisitor : ExpressionVisitor
    {
        private IEnumerable _list;

        public IEnumerable GetList(Expression expression)
        {
            Visit(expression);
            return _list;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            _list = c.Value as IEnumerable;
            return base.VisitConstant(c);
        }
    }
}