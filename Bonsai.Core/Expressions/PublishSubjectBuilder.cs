using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
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
}
