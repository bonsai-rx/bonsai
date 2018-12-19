using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that disables the generation of
    /// expression tree nodes from its decorated builder.
    /// </summary>
    [XmlType("Disable", Namespace = Constants.XmlNamespace)]
    public class DisableBuilder : ExpressionBuilder, INamedElement
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: int.MaxValue);

        /// <summary>
        /// Initializes a new instance of the <see cref="DisableBuilder"/> class.
        /// </summary>
        public DisableBuilder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisableBuilder"/> class with the
        /// specified expression builder.
        /// </summary>
        /// <param name="builder">The expression builder instance to be disabled.</param>
        public DisableBuilder(ExpressionBuilder builder)
            : base(builder, decorator: true)
        {
            Builder = builder;
        }

        /// <summary>
        /// Gets or sets the expression builder to be disabled by this decorator.
        /// </summary>
        public ExpressionBuilder Builder { get; set; }

        /// <summary>
        /// Gets the range of input arguments that the decorated expression builder accepts.
        /// </summary>
        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }

        /// <summary>
        /// Gets the display name of the decorated expression builder.
        /// </summary>
        public string Name
        {
            get { return GetElementDisplayName(Builder); }
        }

        static void EnsureTypes(ExpressionBuilder builder)
        {
            var workflowElement = GetWorkflowElement(builder);
            var unknownType = workflowElement as UnknownTypeBuilder;
            if (unknownType != null) unknownType.Build();
            else
            {
                var workflowBuilder = workflowElement as IWorkflowExpressionBuilder;
                if (workflowBuilder != null && workflowBuilder.Workflow != null)
                {
                    foreach (var node in workflowBuilder.Workflow)
                    {
                        try { EnsureTypes(node.Value); }
                        catch (Exception e)
                        {
                            throw new WorkflowBuildException(e.Message, node.Value, e);
                        }
                    }
                }
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
            var builder = Builder;
            if (IsBuildDependency(builder as IArgumentBuilder))
            {
                return DisconnectExpression.Instance;
            }

            EnsureTypes(builder);
            var distinctArguments = arguments.Distinct().ToArray();
            switch (distinctArguments.Length)
            {
                case 0: return EmptyExpression.Instance;
                case 1: return distinctArguments[0];
                default: return new DisableExpression(distinctArguments);
            }
        }
    }
}
