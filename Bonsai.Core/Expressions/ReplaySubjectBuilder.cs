﻿using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Reactive.ReplaySubject"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(Reactive.ReplaySubject))]
    [XmlType("ReplaySubject", Namespace = Constants.XmlNamespace)]
    [WorkflowElementIcon(typeof(ReplaySubjectBuilder), nameof(ReplaySubjectBuilder))]
    [Description("Replays the values of an observable sequence to all subscribed and future observers using a shared subject.")]
    public class ReplaySubjectBuilder : SubjectBuilder
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
        /// When overridden in a derived class, returns the expression
        /// that creates the shared subject.
        /// </summary>
        /// <param name="expression">
        /// The expression representing the observable input sequence.
        /// </param>
        /// <returns>
        /// The <see cref="Expression"/> that creates the shared subject.
        /// </returns>
        protected override Expression BuildSubject(Expression expression)
        {
            var builderExpression = Expression.Constant(this);
            var parameterType = expression.Type.GetGenericArguments()[0];
            return Expression.Call(builderExpression, nameof(CreateSubject), new[] { parameterType });
        }

        ReplaySubject<TSource> CreateSubject<TSource>()
        {
            var bufferSize = BufferSize;
            var window = Window;
            if (bufferSize.HasValue)
            {
                if (window.HasValue) return new ReplaySubject<TSource>(bufferSize.Value, window.Value);
                else return new ReplaySubject<TSource>(bufferSize.Value);
            }
            else if (window.HasValue)
            {
                return new ReplaySubject<TSource>(window.Value);
            }
            else return new ReplaySubject<TSource>();
        }
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="Reactive.ReplaySubject"/> operator instead.
    /// </summary>
    /// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
    [Obsolete]
    [ProxyType(typeof(Reactive.ReplaySubject<>))]
    [XmlType("ReplaySubject", Namespace = Constants.XmlNamespace)]
    [WorkflowElementIcon(typeof(ReplaySubjectBuilder), nameof(ReplaySubjectBuilder))]
    [Description("Replays the values of other observable sequences to all subscribed and future observers.")]
    public class ReplaySubjectBuilder<T> : SubjectBuilder<T>
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
                if (window.HasValue) return new ReplaySubject<T>(bufferSize.Value, window.Value);
                else return new ReplaySubject<T>(bufferSize.Value);
            }
            else if (window.HasValue)
            {
                return new ReplaySubject<T>(window.Value);
            }
            else return new ReplaySubject<T>();
        }
    }
}
