using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai
{
    /// <summary>
    /// Represents a named workflow property.
    /// </summary>
    [Source]
    [WorkflowElementCategory(ElementCategory.Property)]
    public abstract class WorkflowProperty : INamedElement
    {
        internal WorkflowProperty()
        {
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Represents a strongly typed workflow property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    [DefaultProperty("Value")]
    public class WorkflowProperty<TValue> : WorkflowProperty
    {
        TValue value;
        event Action<TValue> ValueChanged;

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        public TValue Value
        {
            get { return value; }
            set
            {
                this.value = value;
                OnValueChanged(value);
            }
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
                .Return(value)
                .Concat(Observable.FromEvent<TValue>(
                    handler => ValueChanged += handler,
                    handler => ValueChanged -= handler));
        }
    }
}
