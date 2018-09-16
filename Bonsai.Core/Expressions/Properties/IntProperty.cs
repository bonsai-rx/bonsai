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
    /// Represents a workflow property containing a 32-bit signed integer.
    /// </summary>
    [DisplayName("Int")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Represents a workflow property containing a 32-bit signed integer.")]
    public class IntProperty : WorkflowProperty
    {
        int value;
        event Action<int> ValueChanged;

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        [Range(int.MinValue, int.MaxValue)]
        [Description("The value of the property.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        public int Value
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
            get { return typeof(int); }
        }

        void OnValueChanged(int value)
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
        public virtual IObservable<int> Generate()
        {
            return Observable
                .Defer(() => Observable.Return(value))
                .Concat(Observable.FromEvent<int>(
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
        public IObservable<int> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => value);
        }
    }
}
