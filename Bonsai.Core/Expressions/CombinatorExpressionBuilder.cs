using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a base class for expression builders that will generate combinator outputs that
    /// can be combined with dynamic property mappings. This is an abstract class.
    /// </summary>
    [PropertyMapping]
    public abstract class CombinatorExpressionBuilder : VariableArgumentExpressionBuilder
    {
        readonly PropertyMappingCollection propertyMappings = new PropertyMappingCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="CombinatorExpressionBuilder"/> class
        /// with the specified argument range.
        /// </summary>
        /// <param name="minArguments">The inclusive lower bound of the argument range.</param>
        /// <param name="maxArguments">The inclusive upper bound of the argument range.</param>
        protected CombinatorExpressionBuilder(int minArguments, int maxArguments)
            : base(minArguments, maxArguments)
        {
        }

        /// <summary>
        /// Gets the collection of property mappings assigned to this expression builder.
        /// Property mapping subscriptions are processed before evaluating other output generation
        /// expressions.
        /// </summary>
        [Browsable(false)]
        public PropertyMappingCollection PropertyMappings
        {
            get { return propertyMappings; }
        }

        /// <summary>
        /// Generates an <see cref="Expression"/> node that will be passed on to other
        /// builders in the workflow.
        /// </summary>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build()
        {
            var output = BuildCombinator();
            var combinatorExpression = Expression.Constant(this);
            return BuildMappingOutput(combinatorExpression, output, propertyMappings);
        }

        /// <summary>
        /// When overridden in a derived class, generates an <see cref="Expression"/> node
        /// that will be combined with any existing property mappings to produce the final
        /// output of the expression builder.
        /// </summary>
        /// <returns>
        /// An <see cref="Expression"/> tree node that represents the combinator output.
        /// </returns>
        protected abstract Expression BuildCombinator();
    }
}
