using System;
using System.Linq.Expressions;

namespace Bonsai.Expressions
{
    class DisableExpression : Expression
    {
        readonly Expression[] arguments;

        public DisableExpression(Expression[] arguments)
        {
            this.arguments = arguments;
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }

        public override Type Type
        {
            get { throw new InvalidOperationException("Unable to evaluate disabled expression. Ensure there are no conflicting inputs to disabled nodes."); }
        }

        public Expression[] Arguments
        {
            get { return arguments; }
        }
    }
}
