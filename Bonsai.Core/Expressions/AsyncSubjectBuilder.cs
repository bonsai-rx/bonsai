using System;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Reactive.AsyncSubject"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(Reactive.AsyncSubject))]
    [WorkflowElementIcon(nameof(AsyncSubjectBuilder))]
    [XmlType("AsyncSubject", Namespace = Constants.XmlNamespace)]
    [Description("Broadcasts the last value of an observable sequence to all subscribed and future observers using a shared subject.")]
    public class AsyncSubjectBuilder : Reactive.AsyncSubject
    {
        AsyncSubject<TSource> CreateSubject<TSource>()
        {
            return new AsyncSubject<TSource>();
        }
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="Reactive.AsyncSubject"/> operator instead.
    /// </summary>
    /// <typeparam name="T">The type of the result stored by the subject.</typeparam>
    [Obsolete]
    [ProxyType(typeof(Reactive.AsyncSubject<>))]
    [WorkflowElementIcon(nameof(AsyncSubjectBuilder))]
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
