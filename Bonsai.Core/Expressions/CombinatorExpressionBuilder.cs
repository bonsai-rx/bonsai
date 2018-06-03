using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a base class for expression builders that will generate combinator outputs that
    /// can be combined with dynamic property mappings. This is an abstract class.
    /// </summary>
    public abstract class CombinatorExpressionBuilder : VariableArgumentExpressionBuilder, IPropertyMappingBuilder
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
        [Obsolete]
        [Browsable(false)]
        [XmlArrayItem("PropertyMapping")]
        public PropertyMappingCollection PropertyMappings
        {
            get { return propertyMappings; }
        }

        /// <summary>
        /// Generates an <see cref="Expression"/> node from a collection of input arguments.
        /// The result can be chained with other builders in a workflow.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            return BuildCombinator(arguments);
        }

        /// <summary>
        /// When overridden in a derived class, generates an <see cref="Expression"/> node
        /// that will be combined with any existing property mappings to produce the final
        /// output of the expression builder.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>
        /// An <see cref="Expression"/> tree node that represents the combinator output.
        /// </returns>
        protected abstract Expression BuildCombinator(IEnumerable<Expression> arguments);
    }
}
