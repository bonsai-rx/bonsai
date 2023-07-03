using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Xml;
using System.Xml.Serialization;
using Bonsai.Expressions;
using Rx = System.Reactive.Subjects;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an expression builder that replays the values of an observable
    /// sequence to all subscribed and future observers using a shared subject.
    /// </summary>
    [WorkflowElementIcon(nameof(ReplaySubject))]
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [Description("Replays the values of an observable sequence to all subscribed and future observers using a shared subject.")]
    public class ReplaySubject : SubjectBuilder
    {
        /// <summary>
        /// Gets or sets the maximum element count of the replay buffer.
        /// </summary>
        [Description("The maximum element count of the replay buffer.")]
        public int? BufferSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum time length of the replay buffer.
        /// </summary>
        [XmlIgnore]
        [Description("The maximum time length of the replay buffer.")]
        public TimeSpan? Window { get; set; }

        /// <summary>
        /// Gets or sets the XML serializable representation of the replay window interval.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(Window))]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string WindowXml
        {
            get
            {
                var window = Window;
                if (window.HasValue) return XmlConvert.ToString(window.Value);
                else return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(value)) Window = XmlConvert.ToTimeSpan(value);
                else Window = null;
            }
        }

        /// <inheritdoc/>
        protected override Expression BuildSubject(Expression expression)
        {
            var builderExpression = Expression.Constant(this);
            var parameterType = expression.Type.GetGenericArguments()[0];
            return Expression.Call(builderExpression, nameof(CreateSubject), new[] { parameterType });
        }

        Rx.ReplaySubject<TSource> CreateSubject<TSource>()
        {
            var bufferSize = BufferSize;
            var window = Window;
            if (bufferSize.HasValue)
            {
                if (window.HasValue) return new Rx.ReplaySubject<TSource>(bufferSize.Value, window.Value);
                else return new Rx.ReplaySubject<TSource>(bufferSize.Value);
            }
            else if (window.HasValue)
            {
                return new Rx.ReplaySubject<TSource>(window.Value);
            }
            else return new Rx.ReplaySubject<TSource>();
        }
    }

    /// <summary>
    /// Represents an expression builder that replays the values of other observable
    /// sequences to all subscribed and future observers.
    /// </summary>
    /// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
    [WorkflowElementIcon(nameof(ReplaySubject))]
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [Description("Replays the values of other observable sequences to all subscribed and future observers.")]
    public class ReplaySubject<T> : SubjectBuilder<T>
    {
        /// <summary>
        /// Gets or sets the maximum element count of the replay buffer.
        /// </summary>
        [Description("The maximum element count of the replay buffer.")]
        public int? BufferSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum time length of the replay buffer.
        /// </summary>
        [XmlIgnore]
        [Description("The maximum time length of the replay buffer.")]
        public TimeSpan? Window { get; set; }

        /// <summary>
        /// Gets or sets the XML serializable representation of the replay window interval.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(Window))]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string WindowXml
        {
            get
            {
                var window = Window;
                if (window.HasValue) return XmlConvert.ToString(window.Value);
                else return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(value)) Window = XmlConvert.ToTimeSpan(value);
                else Window = null;
            }
        }

        /// <summary>
        /// Creates a shared subject that replays the values of other observable
        /// sequences to all subscribed and future observers.
        /// </summary>
        /// <returns>A new instance of <see cref="ISubject{T}"/>.</returns>
        protected override ISubject<T> CreateSubject()
        {
            var bufferSize = BufferSize;
            var window = Window;
            if (bufferSize.HasValue)
            {
                if (window.HasValue) return new Rx.ReplaySubject<T>(bufferSize.Value, window.Value);
                else return new Rx.ReplaySubject<T>(bufferSize.Value);
            }
            else if (window.HasValue)
            {
                return new Rx.ReplaySubject<T>(window.Value);
            }
            else return new Rx.ReplaySubject<T>();
        }
    }
}
