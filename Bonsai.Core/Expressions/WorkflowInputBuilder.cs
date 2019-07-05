using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents the expression that is used as the input source of an encapsulated workflow.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Source)]
    [XmlType("WorkflowInput", Namespace = Constants.XmlNamespace)]
    [Description("Represents an input sequence inside a nested workflow.")]
    public class WorkflowInputBuilder : ZeroArgumentExpressionBuilder, INamedElement
    {
        readonly ExpressionBuilderArgument parameter = new ExpressionBuilderArgument();

        internal Expression Source { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index of the input parameter.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public int Index
        {
            get { return parameter.Index; }
            set { parameter.Index = value; }
        }

        /// <summary>
        /// Gets or sets the name of the input parameter. Arbitrary named arguments are not supported, so all
        /// names must start with the <see cref="ExpressionBuilderArgument.ArgumentNamePrefix"/> followed by the one-based
        /// argument index.
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get { return parameter.Name; }
            set { parameter.Name = value; }
        }

        /// <summary>
        /// Returns the source input expression specified in <see cref="Source"/>.
        /// </summary>
        /// <returns>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// An <see cref="Expression"/> that will be used as the source of an
        /// encapsulated workflow.
        /// </returns>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            return Source ?? EmptyExpression.Instance;
        }
    }
}
