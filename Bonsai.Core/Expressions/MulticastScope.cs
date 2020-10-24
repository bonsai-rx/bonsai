using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Bonsai.Expressions
{
    class MulticastScope
    {
        public MulticastScope(MulticastBranchBuilder builder)
        {
            MulticastBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public MulticastBranchBuilder MulticastBuilder { get; }

        public List<ExpressionBuilder> References { get; } = new List<ExpressionBuilder>();

        public Expression Close(Expression result)
        {
            var selector = Expression.Lambda(result, MulticastBuilder.BranchExpression.Parameter);
            var output = MulticastBuilder.BuildMulticast(MulticastBuilder.Source, selector);
            MulticastBuilder.Source = null;
            MulticastBuilder.BranchExpression = null;
            return output;
        }
    }
}
