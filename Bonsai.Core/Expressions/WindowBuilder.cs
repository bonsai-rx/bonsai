using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Bonsai.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Window"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(Window))]
    [XmlType("Window", Namespace = Constants.XmlNamespace)]
    [Description("Projects the sequence into zero or more windows with boundaries defined by the encapsulated workflow.")]
    public class WindowBuilder : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowBuilder"/> class.
        /// </summary>
        public WindowBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public WindowBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }
    }
}
