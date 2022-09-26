using System;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Reactive.ResourceSubjectBuilder"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(Reactive.ResourceSubjectBuilder))]
    [XmlType("ResourceSubject", Namespace = Constants.XmlNamespace)]
    [WorkflowElementIcon(typeof(ResourceSubjectBuilder), nameof(ResourceSubjectBuilder))]
    [Description("Stores a disposable resource and shares it with all subscribed and future observers.")]
    public class ResourceSubjectBuilder : Reactive.ResourceSubjectBuilder
    {
        Reactive.ResourceSubjectBuilder<TSource>.ResourceSubject CreateSubject<TSource>() where TSource : class, IDisposable
        {
            return new Reactive.ResourceSubjectBuilder<TSource>.ResourceSubject();
        }
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="Reactive.ResourceSubjectBuilder"/> operator instead.
    /// </summary>
    /// <typeparam name="T">The type of the disposable resource stored by the subject.</typeparam>
    [Obsolete]
    [ProxyType(typeof(Reactive.ResourceSubjectBuilder<>))]
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
            return new Reactive.ResourceSubjectBuilder<T>.ResourceSubject();
        }
    }
}
