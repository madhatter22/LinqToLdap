/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToLdap.Visitors
{
    internal class SelectProjector : ExpressionVisitor
    {
        protected readonly IDictionary<string, string> MappedProperties;
        protected readonly IDictionary<string, string> Properties;
        protected SelectProjection Projection;

        public SelectProjector(IDictionary<string, string> mappedProperties)
        {
            Properties = new Dictionary<string, string>();
            MappedProperties = mappedProperties;
        }

        public virtual SelectProjection ProjectProperties(LambdaExpression p)
        {
            Visit(p);
            Projection = Properties.Count == 0
                              ? new SelectProjection(MappedProperties, p)
                              : new SelectProjection(Properties, p);
            return Projection;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && (m.Expression.NodeType == ExpressionType.Parameter || m.Expression.NodeType == ExpressionType.TypeAs || m.Expression.NodeType == ExpressionType.Convert))
            {
                string name;
                Properties[m.Member.Name] = MappedProperties.TryGetValue(m.Member.Name, out name) ? name : m.Member.Name;

                return m;
            }
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }
    }
}
