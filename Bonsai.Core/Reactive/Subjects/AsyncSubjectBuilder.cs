﻿using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Xml.Serialization;
using Bonsai.Expressions;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an expression builder that broadcasts the last value of an observable
    /// sequence to all subscribed and future observers using a shared subject.
    /// </summary>
    [XmlType("AsyncSubject", Namespace = Constants.ReactiveXmlNamespace)]
    [WorkflowElementIcon(typeof(AsyncSubjectBuilder), nameof(AsyncSubjectBuilder))]
    [Description("Broadcasts the last value of an observable sequence to all subscribed and future observers using a shared subject.")]
    public class AsyncSubjectBuilder : SubjectBuilder
    {
        /// <inheritdoc/>
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
    [XmlType("AsyncSubject", Namespace = Constants.ReactiveXmlNamespace)]
    [WorkflowElementIcon(typeof(AsyncSubjectBuilder), nameof(AsyncSubjectBuilder))]
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
