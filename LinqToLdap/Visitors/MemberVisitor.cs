using System.Linq.Expressions;

namespace LinqToLdap.Visitors
{
    internal class MemberVisitor : ExpressionVisitor
    {
        private Expression _member;

        public Expression GetMember(Expression expression)
        {
            Visit(expression);
            return _member;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m != null && m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                _member = m;
            }
            return m;
        }
    }
}