using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
