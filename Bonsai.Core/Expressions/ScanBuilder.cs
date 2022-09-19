using System;
using System.Xml.Serialization;
using System.ComponentModel;
using Bonsai.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Scan"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(Scan))]
    [XmlType("Scan", Namespace = Constants.XmlNamespace)]
    [Description("Accumulates the values of an observable sequence using the encapsulated workflow.")]
    public class ScanBuilder : Scan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanBuilder"/> class.
        /// </summary>
        public ScanBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public ScanBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }
    }
}
