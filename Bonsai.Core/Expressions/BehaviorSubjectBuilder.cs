using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that broadcasts the latest value of an observable
    /// sequence to all subscribed and future observers using a shared subject.
    /// </summary>
    [XmlType("BehaviorSubject", Namespace = Constants.XmlNamespace)]
    [TypeDescriptionProvider(typeof(BehaviorSubjectTypeDescriptionProvider))]
    [Description("Broadcasts the latest value of an observable sequence to all subscribed and future observers using a shared subject.")]
    public class BehaviorSubjectBuilder : SubjectBuilder
    {
        /// <summary>
        /// Gets or sets the initial value sent to observers when no other value
        /// has been received by the subject yet.
        /// </summary>
        [Browsable(false)]
        public WorkflowProperty Value { get; set; }

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
            var value = Value;
            var builderExpression = Expression.Constant(this);
            var parameterType = expression.Type.GetGenericArguments()[0];
            if (value == null || value.PropertyType != parameterType)
            {
                var propertyType = GetWorkflowPropertyType(parameterType);
                Value = value = (WorkflowProperty)Activator.CreateInstance(propertyType);
            }

            return Expression.Call(builderExpression, "CreateSubject", new[] { parameterType }, Expression.Constant(value));
        }

        BehaviorSubject<TSource> CreateSubject<TSource>(WorkflowProperty<TSource> value)
        {
            return new BehaviorSubject<TSource>(value.Value);
        }
    }
}
