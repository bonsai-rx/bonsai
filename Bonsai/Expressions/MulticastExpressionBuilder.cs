using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    public abstract class MulticastExpressionBuilder : SingleArgumentExpressionBuilder
    {
        internal MulticastExpressionBuilder()
        {
        }

        internal Expression Source { get; set; }

        internal ParameterExpression MulticastParameter { get; set; }

        public override Expression Build()
        {
            Source = Arguments.Values.Single();
            return MulticastParameter = Expression.Parameter(Source.Type);
        }

        internal abstract Expression BuildMulticast(Expression source, LambdaExpression selector);
    }
}
