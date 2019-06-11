using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToLdap.Visitors
{
    internal class BooleanRewriter : ExpressionVisitor
    {
        private HashSet<Expression> _candidates;

        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                return !(((ConstantExpression)expression).Value is IQueryable);
            }
            if (expression.NodeType == ExpressionType.Conditional)
            {
                return true;
            }

            return expression.NodeType != ExpressionType.Parameter;
        }

        public Expression Rewrite(Expression expression)
        {
            _candidates = new Nominator(CanBeEvaluatedLocally).Nominate(expression);

            return Visit(expression);
        }

        protected override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }
            return _candidates.Contains(exp)
                ? Evaluate(exp)
                : base.Visit(exp);
        }

        private static Expression Evaluate(Expression e)
        {
            bool b;
            return ReduceToBool(e, out b);
        }

        private static Expression ReduceToBool(Expression e, out bool canBeReduced)
        {
            switch (e.NodeType)
            {
                case ExpressionType.Constant:
                    canBeReduced = true;
                    return e;

                case ExpressionType.Lambda:
                    var lamda = e as LambdaExpression;

                    if (lamda.Body.NodeType == ExpressionType.Conditional)

                    {
                        return ReduceToBool(lamda.Body, out canBeReduced);
                    }
                    canBeReduced = false;
                    return e;

                case ExpressionType.Quote:
                    return ReduceToBool(StripQuotes(e), out canBeReduced);

                case ExpressionType.ArrayLength:
                    canBeReduced = true;
                    return e;

                case ExpressionType.Not:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.TypeAs:

                    ReduceToBool((e as UnaryExpression).Operand, out canBeReduced);

                    if (canBeReduced)
                    {
                        LambdaExpression lambda = Expression.Lambda(e);
                        Delegate fn = lambda.Compile();
                        return Expression.Constant(fn.DynamicInvoke(null), e.Type);
                    }
                    break;

                case ExpressionType.TypeIs:

                    ReduceToBool((e as TypeBinaryExpression).Expression, out canBeReduced);

                    if (canBeReduced)
                    {
                        LambdaExpression lambda = Expression.Lambda(e);
                        Delegate fn = lambda.Compile();
                        return Expression.Constant(fn.DynamicInvoke(null), e.Type);
                    }
                    break;

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    var binary = e as BinaryExpression;
                    bool left;

                    ReduceToBool(binary.Left, out left);

                    if (left)
                    {
                        bool right;
                        ReduceToBool(binary.Right, out right);
                        if (right)
                        {
                            canBeReduced = true;
                            LambdaExpression lambda = Expression.Lambda(e);
                            Delegate fn = lambda.Compile();
                            return Expression.Constant(fn.DynamicInvoke(null), e.Type);
                        }
                    }
                    break;

                case ExpressionType.Conditional:
                    var conditional = e as ConditionalExpression;
                    bool test;

                    var constant = ReduceToBool(conditional.Test, out test) as ConstantExpression;

                    if (test && constant != null)
                    {
                        if (true.Equals(constant.Value))
                        {
                            return ReduceToBool(conditional.IfTrue, out canBeReduced);
                        }
                        if (false.Equals(constant.Value))
                        {
                            return ReduceToBool(conditional.IfFalse, out canBeReduced);
                        }
                    }
                    break;

                case ExpressionType.Call:
                    canBeReduced = true;
                    return Expression.Constant(Expression.Lambda(e).Compile().DynamicInvoke(null), e.Type);

                case ExpressionType.MemberAccess:
                    var member = e as MemberExpression;

                    bool isNullable = member.Member.DeclaringType.Name != "Nullable`1";

                    if (member.Type == typeof(bool) && ((isNullable && member.Member.Name == "HasValue") || (!isNullable)))
                    {
                        canBeReduced = true;
                        LambdaExpression lambda = Expression.Lambda(e);
                        Delegate fn = lambda.Compile();
                        return Expression.Constant(fn.DynamicInvoke(null), e.Type);
                    }
                    break;
            }

            canBeReduced = false;
            return e;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }
    }
}