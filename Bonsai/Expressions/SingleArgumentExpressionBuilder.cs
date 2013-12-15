using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    public abstract class SingleArgumentExpressionBuilder : ExpressionBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 1, upperBound: 1);

        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }
    }
}
