using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;

namespace Bonsai.Expressions
{
    class MulticastScope
    {
        readonly MulticastBranchBuilder multicastBuilder;
        readonly List<ExpressionBuilder> references = new List<ExpressionBuilder>();

        public MulticastScope(MulticastBranchBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            multicastBuilder = builder;
        }

        public MulticastBranchBuilder MulticastBuilder
        {
            get { return multicastBuilder; }
        }

        public List<ExpressionBuilder> References
        {
            get { return references; }
        }

        public Expression Close(Expression result)
        {
            var resultType = result.Type.GetGenericArguments()[0];
            var selector = Expression.Lambda(result, multicastBuilder.BranchExpression.Parameter);
            var output = multicastBuilder.BuildMulticast(multicastBuilder.Source, selector);
            multicastBuilder.Source = null;
            multicastBuilder.BranchExpression = null;
            return output;
        }
    }
}
