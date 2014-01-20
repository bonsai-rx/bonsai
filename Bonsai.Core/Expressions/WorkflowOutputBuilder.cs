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
    /// Represents the expression that is used as the output of an encapsulated workflow.
    /// </summary>
    [XmlType("WorkflowOutput", Namespace = Constants.XmlNamespace)]
    [Description("Represents an output sequence inside a nested workflow.")]
    public class WorkflowOutputBuilder : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Returns the output expression specified in <see cref="Output"/>.
        /// </summary>
        /// <returns>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// An <see cref="Expression"/> that will be used as the output of an
        /// encapsulated workflow.
        /// </returns>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            return arguments.Single();
        }
    }
}
