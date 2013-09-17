using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Expressions
{
    public abstract class CombinatorExpressionBuilder : ExpressionBuilder
    {
        readonly Range<int> argumentRange;

        protected CombinatorExpressionBuilder()
            : this(1, 1)
        {
        }

        protected CombinatorExpressionBuilder(int lowerBound, int upperBound)
        {
            argumentRange = Range.Create(lowerBound, upperBound);
        }

        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }
    }
}
