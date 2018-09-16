using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
