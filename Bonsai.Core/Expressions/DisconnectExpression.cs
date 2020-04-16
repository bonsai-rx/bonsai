using System.Linq.Expressions;

namespace Bonsai.Expressions
{
    class DisconnectExpression : Expression
    {
        internal static readonly DisconnectExpression Instance = new DisconnectExpression();

        private DisconnectExpression()
        {
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }
    }
}
