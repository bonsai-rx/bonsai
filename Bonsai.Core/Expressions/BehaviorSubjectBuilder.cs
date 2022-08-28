using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that broadcasts the latest value of an observable
    /// sequence to all subscribed and future observers using a shared subject.
    /// </summary>
    [XmlType("BehaviorSubject", Namespace = Constants.XmlNamespace)]
    [WorkflowElementIcon(typeof(BehaviorSubjectBuilder), nameof(BehaviorSubjectBuilder))]
    [Description("Broadcasts the latest value of an observable sequence to all subscribed and future observers using a shared subject.")]
    public class BehaviorSubjectBuilder : SubjectBuilder
    {
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

        BehaviorSubjectBuilder<TSource>.BehaviorSubject CreateSubject<TSource>()
        {
            return new BehaviorSubjectBuilder<TSource>.BehaviorSubject();
        }
    }

    /// <summary>
    /// Represents an expression builder that broadcasts the latest value from other observable
    /// sequences to all subscribed and future observers.
    /// </summary>
    /// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
    [XmlType("BehaviorSubject", Namespace = Constants.XmlNamespace)]
    [WorkflowElementIcon(typeof(BehaviorSubjectBuilder), nameof(BehaviorSubjectBuilder))]
    [Description("Broadcasts the latest value from other observable sequences to all subscribed and future observers.")]
    public class BehaviorSubjectBuilder<T> : SubjectBuilder<T>
    {
        /// <summary>
        /// Creates a shared subject that broadcasts the latest value from other observable
        /// sequences to all subscribed and future observers.
        /// </summary>
        /// <returns>A new instance of <see cref="ISubject{T}"/>.</returns>
        protected override ISubject<T> CreateSubject()
        {
            return new BehaviorSubject();
        }

        internal class BehaviorSubject : ISubject<T>, IDisposable
        {
            readonly ReplaySubject<T> subject = new ReplaySubject<T>(1);

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
