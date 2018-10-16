using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class MulticastBranchExpression : Expression
    {
        Expression expression;

        public MulticastBranchExpression(ParameterExpression parameter, Expression source)
        {
            expression = parameter;
            Source = source;
        }

        public Expression Source { get; private set; }

        public new ParameterExpression Parameter
        {
            get { return expression as ParameterExpression; }
        }

        public override Type Type
        {
            get { return expression.Type; }
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }

        public override bool CanReduce
        {
            get { return true; }
        }

        public override Expression Reduce()
        {
            return expression;
        }

        public override string ToString()
        {
            return expression.ToString();
        }

        public void Cancel()
        {
            expression = Source;
        }
    }
}
