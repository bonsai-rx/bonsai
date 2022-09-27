using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Xml.Serialization;
using Bonsai.Expressions;
using Rx = System.Reactive.Subjects;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an expression builder that broadcasts the latest value of an observable
    /// sequence to all subscribed and future observers using a shared subject.
    /// </summary>
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [WorkflowElementIcon(typeof(BehaviorSubject), nameof(BehaviorSubject))]
    [Description("Broadcasts the latest value of an observable sequence to all subscribed and future observers using a shared subject.")]
    public class BehaviorSubject : SubjectBuilder
    {
        /// <inheritdoc/>
        protected override Expression BuildSubject(Expression expression)
        {
            var builderExpression = Expression.Constant(this);
            var parameterType = expression.Type.GetGenericArguments()[0];
            return Expression.Call(builderExpression, nameof(CreateSubject), new[] { parameterType });
        }

        BehaviorSubject<TSource>.Subject CreateSubject<TSource>()
        {
            return new BehaviorSubject<TSource>.Subject();
        }
    }

    /// <summary>
    /// Represents an expression builder that broadcasts the latest value from other observable
    /// sequences to all subscribed and future observers.
    /// </summary>
    /// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [WorkflowElementIcon(typeof(BehaviorSubject), nameof(BehaviorSubject))]
    [Description("Broadcasts the latest value from other observable sequences to all subscribed and future observers.")]
    public class BehaviorSubject<T> : SubjectBuilder<T>
    {
        /// <summary>
        /// Creates a shared subject that broadcasts the latest value from other observable
        /// sequences to all subscribed and future observers.
        /// </summary>
        /// <returns>A new instance of <see cref="ISubject{T}"/>.</returns>
        protected override ISubject<T> CreateSubject()
        {
            return new Subject();
        }

        internal class Subject : ISubject<T>, IDisposable
        {
            readonly Rx.ReplaySubject<T> subject = new Rx.ReplaySubject<T>(1);

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(T value)
            {
                subject.OnNext(value);
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                return subject.Subscribe(observer);
            }

            public void Dispose()
            {
                subject.Dispose();
            }
        }
    }
}
