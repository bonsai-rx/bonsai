using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Bonsai.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="SelectMany"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(SelectMany))]
    [XmlType("SelectMany", Namespace = Constants.XmlNamespace)]
    [Description("Generates one observable sequence for each input and merges the results into a single sequence.")]
    public class SelectManyBuilder : SelectMany
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectManyBuilder"/> class.
        /// </summary>
        public SelectManyBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectManyBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public SelectManyBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }
    }
}
