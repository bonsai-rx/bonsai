using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that broadcasts the values of an observable
    /// sequence to multiple subscribers using a shared subject.
    /// </summary>
    [XmlType("PublishSubject", Namespace = Constants.XmlNamespace)]
    [Description("Broadcasts the values of an observable sequence to multiple subscribers using a shared subject.")]
    public class PublishSubjectBuilder : SubjectBuilder
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
            var parameterType = expression.Type.GetGenericArguments()[0];
            return Expression.New(typeof(Subject<>).MakeGenericType(parameterType));
        }
    }

    /// <summary>
    /// Represents an expression builder that broadcasts the values from other observable
    /// sequences to multiple subscribers.
    /// </summary>
    /// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
    [XmlType("PublishSubject", Namespace = Constants.XmlNamespace)]
    [Description("Broadcasts the values from other observable sequences to multiple subscribers.")]
    public class PublishSubjectBuilder<T> : SubjectBuilder<T>
    {
        /// <summary>
        /// Creates a shared subject that broadcasts the values from other observable
        /// sequences to multiple subscribers.
        /// </summary>
        /// <returns>A new instance of <see cref="ISubject{T}"/>.</returns>
        protected override ISubject<T> CreateSubject()
        {
            return new Subject<T>();
        }
    }
}
