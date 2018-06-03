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
    /// Represents a workflow property containing a single-precision floating-point number.
    /// </summary>
    [DisplayName("Float")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Represents a workflow property containing a single-precision floating-point number.")]
    public class FloatProperty : WorkflowProperty
    {
        float value;
        event Action<float> ValueChanged;

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        [Range(0, 1)]
        [Description("The value of the property.")]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        public float Value
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
            get { return typeof(float); }
        }

        void OnValueChanged(float value)
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
        public virtual IObservable<float> Generate()
        {
            return Observable
                .Defer(() => Observable.Return(value))
                .Concat(Observable.FromEvent<float>(
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
        public IObservable<float> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => value);
        }
    }
}
