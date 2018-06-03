using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a workflow property containing a time interval.
    /// </summary>
    [DisplayName("TimeSpan")]
    [DefaultProperty("Value")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Represents a workflow property containing a time interval.")]
    public class TimeSpanProperty : WorkflowProperty
    {
        TimeSpan value;
        event Action<TimeSpan> ValueChanged;

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        [XmlIgnore]
        [Description("The value of the property.")]
        public TimeSpan Value
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
            get { return typeof(TimeSpan); }
        }

        /// <summary>
        /// Gets or sets an XML representation of the property value for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement("Value")]
        public string ValueXml
        {
            get { return XmlConvert.ToString(Value); }
            set { Value = XmlConvert.ToTimeSpan(value); }
        }

        void OnValueChanged(TimeSpan value)
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
        public virtual IObservable<TimeSpan> Generate()
        {
            return Observable
                .Defer(() => Observable.Return(value))
                .Concat(Observable.FromEvent<TimeSpan>(
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
        public IObservable<TimeSpan> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => value);
        }
    }
}
