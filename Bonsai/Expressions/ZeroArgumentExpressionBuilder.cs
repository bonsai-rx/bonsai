using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    public abstract class ZeroArgumentExpressionBuilder : ExpressionBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: 0);

        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }
    }
}
