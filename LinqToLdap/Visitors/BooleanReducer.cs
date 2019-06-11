using System.Linq.Expressions;

namespace LinqToLdap.Visitors
{
    internal class BooleanReducer : ExpressionVisitor
    {
        private bool _requiresEvaluation = true;

        protected override Expression VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.OrElse)
            {
                if (b.Left.NodeType == ExpressionType.Constant && b.Left.Type == typeof(bool))
                {
                    _requiresEvaluation = true;
                    return Visit(true.Equals((b.Left as ConstantExpression).Value) ? b.Left : b.Right);
                }
                if (b.Right.NodeType == ExpressionType.Constant && b.Right.Type == typeof(bool))
                {
                    _requiresEvaluation = true;
                    return Visit(true.Equals((b.Right as ConstantExpression).Value) ? b.Right : b.Left);
                }
            }
            if (b.NodeType == ExpressionType.AndAlso)
            {
                if (b.Left.NodeType == ExpressionType.Constant &&
                    b.Right.NodeType == ExpressionType.Constant &&
                    b.Left.Type == typeof(bool) &&
                    b.Right.Type == typeof(bool))
                {
                    _requiresEvaluation = true;
                    var leftValue = (b.Left as ConstantExpression).Value;
                    var rightValue = (b.Right as ConstantExpression).Value;
                    if (leftValue == rightValue)
                    {
                        return Visit(b.Left);
                    }
                    if (false.Equals(leftValue))
                    {
                        return Visit(b.Left);
                    }
                    if (false.Equals(rightValue))
                    {
                        return Visit(b.Left);
                    }
                }
                if (b.Left.NodeType == ExpressionType.Constant && b.Left.Type == typeof(bool))
                {
                    var value = (b.Left as ConstantExpression).Value;
                    if (false.Equals(value))
                    {
                        return Visit(b.Left);
                    }
                    if (true.Equals(value))
                    {
                        return Visit(b.Right);
                    }
                }
                else if (b.Right.NodeType == ExpressionType.Constant && b.Right.Type == typeof(bool))
                {
                    var value = (b.Right as ConstantExpression).Value;
                    if (false.Equals(value))
                    {
                        return Visit(b.Right);
                    }
                    if (true.Equals(value))
                    {
                        return Visit(b.Left);
                    }
                }
            }
            return base.VisitBinary(b);
        }

        public Expression Reduce(Expression expression)
        {
            Expression reduced = expression;
            while (_requiresEvaluation)
            {
                _requiresEvaluation = false;
                reduced = Visit(reduced);
            }

            return reduced;
        }
    }
}