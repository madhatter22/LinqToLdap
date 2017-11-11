using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToLdap.Visitors
{
    /// <summary>
    /// Performs bottom-up analysis to determine which nodes can possibly
    /// be part of an evaluated sub-tree.
    /// </summary>
    internal class Nominator : ExpressionVisitor
    {
        private readonly Func<Expression, bool> _fnCanBeEvaluated;
        private HashSet<Expression> _candidates;
        private bool _cannotBeEvaluated;

        public Nominator(Func<Expression, bool> fnCanBeEvaluated)
        {
            _fnCanBeEvaluated = fnCanBeEvaluated;
        }

        public HashSet<Expression> Nominate(Expression expression)
        {
            _candidates = new HashSet<Expression>();
            Visit(expression);
            return _candidates;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        //protected override Expression VisitUnary(UnaryExpression u)
        //{
        //    return u.NodeType == ExpressionType.Quote ? StripQuotes(u) : base.VisitUnary(u);
        //}

        protected override Expression Visit(Expression expression)
        {
            if (expression != null)
            {
                bool saveCannotBeEvaluated = _cannotBeEvaluated;
                _cannotBeEvaluated = false;
                base.Visit(expression);
                if (!_cannotBeEvaluated)
                {
                    if (_fnCanBeEvaluated(expression))
                    {
                        _candidates.Add(expression);
                    }
                    else
                    {
                        _cannotBeEvaluated = true;
                    }
                }
                _cannotBeEvaluated |= saveCannotBeEvaluated;
            }
            return expression;
        }
    }
}