using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    public abstract class VariableArgumentExpressionBuilder : ExpressionBuilder
    {
        Range<int> argumentRange;

        protected VariableArgumentExpressionBuilder(int minArguments, int maxArguments)
        {
            SetArgumentRange(minArguments, maxArguments);
        }

        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }

        protected void SetArgumentRange(int minArguments, int maxArguments)
        {
            argumentRange = Range.Create(minArguments, maxArguments);
        }
    }
}
