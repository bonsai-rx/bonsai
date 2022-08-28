using System;
using System.Linq.Expressions;

namespace Bonsai.Expressions
{
    class DisableExpression : Expression
    {
        public DisableExpression(Expression[] arguments)
        {
            Arguments = arguments;
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }

        public override Type Type
        {
            get { throw new InvalidOperationException("Unable to evaluate disabled expression. Ensure there are no conflicting inputs to disabled nodes."); }
        }

        public Expression[] Arguments { get; }
    }
}
