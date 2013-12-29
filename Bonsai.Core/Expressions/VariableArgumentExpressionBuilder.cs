using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a base class for expression builders that can have a variable number of
    /// input arguments. This is an abstract class.
    /// </summary>
    public abstract class VariableArgumentExpressionBuilder : ExpressionBuilder
    {
        Range<int> argumentRange;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableArgumentExpressionBuilder"/> class
        /// with the specified argument range.
        /// </summary>
        /// <param name="minArguments">The inclusive lower bound of the argument range.</param>
        /// <param name="maxArguments">The inclusive upper bound of the argument range.</param>
        protected VariableArgumentExpressionBuilder(int minArguments, int maxArguments)
        {
            SetArgumentRange(minArguments, maxArguments);
        }

        /// <summary>
        /// Gets the range of input arguments that this expression builder accepts.
        /// </summary>
        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }

        /// <summary>
        /// Updates the argument range of the expression builder.
        /// </summary>
        /// <param name="minArguments">The inclusive lower bound of the argument range.</param>
        /// <param name="maxArguments">The inclusive upper bound of the argument range.</param>
        protected void SetArgumentRange(int minArguments, int maxArguments)
        {
            argumentRange = Range.Create(minArguments, maxArguments);
        }
    }
}
