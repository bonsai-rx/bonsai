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
    /// Represents an expression builder that stores and broadcasts the last disposable
    /// value of an observable sequence to all subscribed and future observers. The value
    /// is disposed when the containing context is closed.
    /// </summary>
    [WorkflowElementIcon(nameof(ResourceSubject))]
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [Description("Stores a disposable resource and shares it with all subscribed and future observers.")]
    public class ResourceSubject : SubjectBuilder
    {
        /// <inheritdoc/>
        protected override Expression BuildSubject(Expression expression)
        {
            var builderExpression = Expression.Constant(this);
            var parameterType = expression.Type.GetGenericArguments()[0];
            return Expression.Call(builderExpression, nameof(CreateSubject), new[] { parameterType });
        }

        ResourceSubject<TSource>.Subject CreateSubject<TSource>() where TSource : class, IDisposable
        {
            return new ResourceSubject<TSource>.Subject();
        }
    }

    /// <summary>
    /// Represents an expression builder that stores and broadcasts the last disposable
    /// value of an observable sequence to all subscribed and future observers. The value
    /// is disposed when the containing context is closed.
    /// </summary>
    /// <typeparam name="T">The type of the disposable resource stored by the subject.</typeparam>
    [WorkflowElementIcon(nameof(ResourceSubject))]
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [Description("Stores a disposable resource and shares it with all subscribed and future observers.")]
    public class ResourceSubject<T> : SubjectBuilder<T> where T : class, IDisposable
    {
        /// <summary>
        /// Creates a shared subject that stores and broadcasts the last disposable
        /// value of an observable sequence to all subscribed and future observers. The value
        /// is disposed when the containing context is closed.
        /// </summary>
        /// <returns>A new instance of <see cref="ISubject{T}"/>.</returns>
        protected override ISubject<T> CreateSubject()
        {
            return new Subject();
        }

        internal class Subject : ISubject<T>, IDisposable
        {
            readonly Rx.AsyncSubject<T> subject = new Rx.AsyncSubject<T>();

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
                    try
                    {
                        var disposable = subject.GetResult();
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                    catch { /* source terminated exceptionally */ }
                }

                subject.Dispose();
            }
        }
    }
}
