using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a workflow property.
    /// </summary>
    [Source]
    [DefaultProperty("Value")]
    [Combinator(MethodName = "Generate")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [WorkflowElementCategory(ElementCategory.Source)]
    public abstract class WorkflowProperty
    {
        internal WorkflowProperty()
        {
        }

        internal abstract Type PropertyType { get; }
    }

    /// <summary>
    /// Represents a strongly typed workflow property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class WorkflowProperty<TValue> : WorkflowProperty
    {
        TValue value;
        event Action<TValue> ValueChanged;

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        [Description("The value of the property.")]
        public TValue Value
        {
            get { return value; }
            set
            {
                this.value = value;
                OnValueChanged(value);
            }
        }

        internal override Type PropertyType
        {
            get { return typeof(TValue); }
        }

        void OnValueChanged(TValue value)
        {
            var handler = ValueChanged;
            if (handler != null)
            {
                handler(value);
            }
        }

        /// <summary>
        /// Generates an observable sequence that produces a value whenever the
        /// workflow property changes, starting with the initial property value.
        /// </summary>
        /// <returns>An observable sequence of property values.</returns>
        public virtual IObservable<TValue> Generate()
        {
            return Observable
                .Defer(() => Observable.Return(value))
                .Concat(Observable.FromEvent<TValue>(
                    handler => ValueChanged += handler,
                    handler => ValueChanged -= handler));
        }

        /// <summary>
        /// Generates an observable sequence that produces a value whenever the
        /// source sequence emits a new element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence used to generate new values.</param>
        /// <returns>An observable sequence of property values.</returns>
        public IObservable<TValue> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => value);
        }
    }
}
