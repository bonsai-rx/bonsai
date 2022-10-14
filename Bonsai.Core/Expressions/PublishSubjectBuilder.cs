using System;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Reactive.PublishSubject"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(Reactive.PublishSubject))]
    [XmlType("PublishSubject", Namespace = Constants.XmlNamespace)]
    [WorkflowElementIcon(typeof(PublishSubjectBuilder), nameof(PublishSubjectBuilder))]
    [Description("Broadcasts the values of an observable sequence to multiple subscribers using a shared subject.")]
    public class PublishSubjectBuilder : Reactive.PublishSubject
    {
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="Reactive.PublishSubject"/> operator instead.
    /// </summary>
    /// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
    [Obsolete]
    [ProxyType(typeof(Reactive.PublishSubject<>))]
    [XmlType("PublishSubject", Namespace = Constants.XmlNamespace)]
    [WorkflowElementIcon(typeof(PublishSubjectBuilder), nameof(PublishSubjectBuilder))]
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
