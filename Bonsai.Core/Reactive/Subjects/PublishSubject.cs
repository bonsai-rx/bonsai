using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Xml.Serialization;
using Bonsai.Expressions;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an expression builder that broadcasts the values of an observable
    /// sequence to multiple subscribers using a shared subject.
    /// </summary>
    [WorkflowElementIcon(nameof(PublishSubject))]
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [Description("Broadcasts the values of an observable sequence to multiple subscribers using a shared subject.")]
    public class PublishSubject : SubjectBuilder
    {
        /// <inheritdoc/>
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
    [WorkflowElementIcon(nameof(PublishSubject))]
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [Description("Broadcasts the values from other observable sequences to multiple subscribers.")]
    public class PublishSubject<T> : SubjectBuilder<T>
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
