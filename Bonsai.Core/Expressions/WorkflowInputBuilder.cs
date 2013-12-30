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
    [XmlType("WorkflowInput", Namespace = Constants.XmlNamespace)]
    [Description("Represents an input sequence inside a nested workflow.")]
    public class WorkflowInputBuilder : ZeroArgumentExpressionBuilder
    {
        /// <summary>
        /// Gets or sets the source of an encapsulated workflow.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public Expression Source { get; set; }

        /// <summary>
        /// Returns the source input expression specified in <see cref="Source"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="Expression"/> that will be used as the source of an
        /// encapsulated workflow.
        /// </returns>
        public override Expression Build()
        {
            return Source;
        }
    }
}
