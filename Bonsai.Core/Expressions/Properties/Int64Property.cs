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
    /// Represents a workflow property containing a 64-bit signed integer.
    /// </summary>
    [DisplayName("Int64")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Represents a workflow property containing a 64-bit signed integer.")]
    public class Int64Property : WorkflowProperty
    {
        long value;
        event Action<long> ValueChanged;

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        [Range(long.MinValue, long.MaxValue)]
        [Description("The value of the property.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        public long Value
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
            get { return typeof(long); }
        }

        void OnValueChanged(long value)
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
        public virtual IObservable<long> Generate()
        {
            return Observable
                .Defer(() => Observable.Return(value))
                .Concat(Observable.FromEvent<long>(
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
        public IObservable<long> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => value);
        }
    }
}
