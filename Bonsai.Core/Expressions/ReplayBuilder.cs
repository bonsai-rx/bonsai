using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Bonsai.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Replay"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(Replay))]
    [XmlType("Replay", Namespace = Constants.XmlNamespace)]
    [Description("Shares an observable sequence across the encapsulated workflow by eagerly replaying notifications.")]
    public class ReplayBuilder : Replay
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayBuilder"/> class.
        /// </summary>
        public ReplayBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public ReplayBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }
    }
}
