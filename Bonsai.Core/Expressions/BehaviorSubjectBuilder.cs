using System;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Reactive.BehaviorSubject"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(Reactive.BehaviorSubject))]
    [XmlType("BehaviorSubject", Namespace = Constants.XmlNamespace)]
    [WorkflowElementIcon(typeof(BehaviorSubjectBuilder), nameof(BehaviorSubjectBuilder))]
    [Description("Broadcasts the latest value of an observable sequence to all subscribed and future observers using a shared subject.")]
    public class BehaviorSubjectBuilder : Reactive.BehaviorSubject
    {
        Reactive.BehaviorSubject<TSource>.Subject CreateSubject<TSource>()
        {
            return new Reactive.BehaviorSubject<TSource>.Subject();
        }
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="Reactive.BehaviorSubject"/> operator instead.
    /// </summary>
    /// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
    [Obsolete]
    [ProxyType(typeof(Reactive.BehaviorSubject<>))]
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
            return new Reactive.BehaviorSubject<T>.Subject();
        }
    }
}
