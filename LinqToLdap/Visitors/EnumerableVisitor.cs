/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

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
