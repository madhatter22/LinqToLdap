using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToLdap.Visitors
{
    /// <summary>
    /// Pulled from http://blogs.msdn.com/b/mattwar/archive/2008/11/18/linq-links.aspx
    /// </summary>
    internal static class Evaluator
    {
        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        private static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
        {
            expression = new BooleanRewriter().Rewrite(expression);
            expression = new BooleanReducer().Reduce(expression);
            return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(Expression expression)
        {
            return PartialEval(expression, CanBeEvaluatedLocally);
        }

        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                return !(((ConstantExpression)expression).Value is IQueryable);
            }

            return expression.NodeType != ExpressionType.Parameter;
        }

        #region Nested type: SubtreeEvaluator

        /// <summary>
        /// Evaluates and replaces sub-trees when first candidate is reached (top-down)
        /// </summary>

        private class SubtreeEvaluator : ExpressionVisitor
        {
            private readonly HashSet<Expression> _candidates;

            public SubtreeEvaluator(HashSet<Expression> candidates)
            {
                _candidates = candidates;
            }

            public Expression Eval(Expression exp)
            {
                return Visit(exp);
            }

            protected override Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return null;
                }
                if (_candidates.Contains(exp))
                {
                    return Evaluate(exp);
                }
                return base.Visit(exp);
            }

            private static Expression Evaluate(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant)
                    return e;

                if (e.NodeType == ExpressionType.Lambda)
                    return e;

                LambdaExpression lambda = Expression.Lambda(e);
                Delegate fn = lambda.Compile();
                return Expression.Constant(fn.DynamicInvoke(null), e.Type);
            }
        }

        #endregion Nested type: SubtreeEvaluator
    }
}