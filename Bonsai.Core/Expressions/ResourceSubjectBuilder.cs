using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that stores and broadcasts the last disposable
    /// value of an observable sequence to all subscribed and future observers. The value
    /// is disposed when the containing context is closed.
    /// </summary>
    [XmlType("ResourceSubject", Namespace = Constants.XmlNamespace)]
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
            return Expression.Call(builderExpression, "CreateSubject", new[] { parameterType });
        }

        ResourceSubject<TSource> CreateSubject<TSource>() where TSource : class, IDisposable
        {
            return new ResourceSubject<TSource>();
        }

        class ResourceSubject<T> : ISubject<T>, IDisposable where T : class, IDisposable
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
