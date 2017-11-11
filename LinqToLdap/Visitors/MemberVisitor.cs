/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

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
