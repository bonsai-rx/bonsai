using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that generates an expression tree by applying
    /// an encapsulated workflow selector to the elements of an observable sequence of windows.
    /// </summary>
    [Obsolete]
    [XmlType("WindowWorkflow", Namespace = Constants.XmlNamespace)]
    [Description("Processes each input window using the encapsulated workflow.")]
    public class WindowWorkflowBuilder : CreateObservableBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowWorkflowBuilder"/> class.
        /// </summary>
        public WindowWorkflowBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowWorkflowBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public WindowWorkflowBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        /// <summary>
        /// Gets the range of input arguments that this expression builder accepts.
        /// </summary>
        public override Range<int> ArgumentRange
        {
            get
            {
                var parameterCount = Workflow.GetNestedParameters().Count();
                return Range.Create(Math.Max(1, parameterCount), Math.Max(1, parameterCount));
            }
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
            var source = arguments.FirstOrDefault();
            if (source == null)
            {
                throw new InvalidOperationException("There must be at least one input to WindowWorkflow.");
            }

            return base.Build(arguments);
        }
    }
}
