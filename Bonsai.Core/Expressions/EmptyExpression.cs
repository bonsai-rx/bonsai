using System;
using System.Linq.Expressions;

namespace Bonsai.Expressions
{
    class EmptyExpression : Expression
    {
        internal static readonly EmptyExpression Instance = new EmptyExpression();

        private EmptyExpression()
        {
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }

        public override Type Type
        {
            get { return typeof(void); }
        }
    }
}
