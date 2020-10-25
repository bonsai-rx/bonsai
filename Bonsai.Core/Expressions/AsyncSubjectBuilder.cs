using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that broadcasts the last value of an observable
    /// sequence to all subscribed and future observers using a shared subject.
    /// </summary>
    [XmlType("AsyncSubject", Namespace = Constants.XmlNamespace)]
    [Description("Broadcasts the last value of an observable sequence to all subscribed and future observers using a shared subject.")]
    public class AsyncSubjectBuilder : SubjectBuilder
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

        AsyncSubject<TSource> CreateSubject<TSource>()
        {
            return new AsyncSubject<TSource>();
        }
    }

    /// <summary>
    /// Represents an expression builder that broadcasts the result of the first observable
    /// sequence to complete to all subscribed and future observers.
    /// </summary>
    /// <typeparam name="T">The type of the result stored by the subject.</typeparam>
    [XmlType("AsyncSubject", Namespace = Constants.XmlNamespace)]
    [Description("Broadcasts the result of the first observable sequence to complete to all subscribed and future observers.")]
    public class AsyncSubjectBuilder<T> : SubjectBuilder<T>
    {
        /// <summary>
        /// Creates a shared subject that broadcasts the result of the first observable
        /// sequence to complete to all subscribed and future observers.
        /// </summary>
        /// <returns>A new instance of <see cref="ISubject{T}"/>.</returns>
        protected override ISubject<T> CreateSubject()
        {
            return new AsyncSubject<T>();
        }
    }
}
