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
            return Expression.Call(builderExpression, "CreateSubject", new[] { parameterType });
        }

        AsyncSubject<TSource> CreateSubject<TSource>()
        {
            return new AsyncSubject<TSource>();
        }
    }
}
