using System;
using System.Linq.Expressions;

namespace DTCBillingSystem.Infrastructure.Data
{
    internal class PredicateConverter : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public PredicateConverter(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression is ParameterExpression parameter && parameter == _oldParameter)
            {
                return Expression.Property(_newParameter, node.Member.Name);
            }
            return base.VisitMember(node);
        }
    }
} 