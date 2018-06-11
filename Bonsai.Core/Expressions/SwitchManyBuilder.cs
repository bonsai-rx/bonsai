using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that combines higher-order observable sequences
    /// generated from the encapsulated workflow by producing values only from the most
    /// recent observable.
    /// </summary>
    [XmlType("SwitchMany", Namespace = Constants.XmlNamespace)]
    [Description("Generates one observable sequence for each input and combines the results by producing values only from the most recent sequence.")]
    public class SwitchManyBuilder : WorkflowCombinatorBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchManyBuilder"/> class.
        /// </summary>
        public SwitchManyBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchManyBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public SwitchManyBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        internal override IObservable<TResult> Process<TSource, TResult>(IObservable<TSource> source, Func<TSource, IObservable<TResult>> selector)
        {
            return source.Select(selector).Switch();
        }
    }
}
