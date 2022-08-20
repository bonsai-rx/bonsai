using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that stores and broadcasts the last disposable
    /// value of an observable sequence to all subscribed and future observers. The value
    /// is disposed when the containing context is closed.
    /// </summary>
    [XmlType("ResourceSubject", Namespace = Constants.XmlNamespace)]
    [WorkflowElementIcon(typeof(ResourceSubjectBuilder), nameof(ResourceSubjectBuilder))]
    [Description("Stores a disposable resource and shares it with all subscribed and future observers.")]
    public class ResourceSubjectBuilder : SubjectBuilder
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

        ResourceSubjectBuilder<TSource>.ResourceSubject CreateSubject<TSource>() where TSource : class, IDisposable
        {
            return new ResourceSubjectBuilder<TSource>.ResourceSubject();
        }
    }

    /// <summary>
    /// Represents an expression builder that stores and broadcasts the last disposable
    /// value of an observable sequence to all subscribed and future observers. The value
    /// is disposed when the containing context is closed.
    /// </summary>
    /// <typeparam name="T">The type of the disposable resource stored by the subject.</typeparam>
    [XmlType("ResourceSubject", Namespace = Constants.XmlNamespace)]
    [WorkflowElementIcon(typeof(ResourceSubjectBuilder), nameof(ResourceSubjectBuilder))]
    [Description("Stores a disposable resource and shares it with all subscribed and future observers.")]
    public class ResourceSubjectBuilder<T> : SubjectBuilder<T> where T : class, IDisposable
    {
        /// <summary>
        /// Creates a shared subject that stores and broadcasts the last disposable
        /// value of an observable sequence to all subscribed and future observers. The value
        /// is disposed when the containing context is closed.
        /// </summary>
        /// <returns>A new instance of <see cref="ISubject{T}"/>.</returns>
        protected override ISubject<T> CreateSubject()
        {
            return new ResourceSubject();
        }

        internal class ResourceSubject : ISubject<T>, IDisposable
        {
            readonly AsyncSubject<T> subject = new AsyncSubject<T>();

            public void OnCompleted()
            {
                subject.OnCompleted();
            }

            public void OnError(Exception error)
            {
                subject.OnError(error);
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
                if (subject.IsCompleted)
                {
                    var disposable = subject.GetResult();
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }

                subject.Dispose();
            }
        }
    }
}
