using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a base class for expression builders that require a single input argument.
    /// This is an abstract class.
    /// </summary>
    public abstract class SingleArgumentExpressionBuilder : ExpressionBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 1, upperBound: 1);

        /// <summary>
        /// Gets the range of input arguments that this expression builder accepts.
        /// </summary>
        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }
    }
}
