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
    /// Provides a base class for expression builders that broadcast the values of an observable
    /// sequence to multiple subscribers using a shared subject. This is an abstract class.
    /// </summary>
    [DefaultProperty("Name")]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [XmlType("Subject", Namespace = Constants.XmlNamespace)]
    public abstract class SubjectBuilder : SingleArgumentExpressionBuilder, INamedElement, IRequireBuildContext
    {
        IBuildContext buildContext;

        /// <summary>
        /// Gets or sets the name of the shared subject.
        /// </summary>
        [Category("Subject")]
        [Description("The name of the shared subject.")]
        public string Name { get; set; }

        IBuildContext IRequireBuildContext.BuildContext
        {
            get { return buildContext; }
            set { buildContext = value; }
        }

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
        protected abstract Expression BuildSubject(Expression expression);

        /// <summary>
        /// Generates an <see cref="Expression"/> node from a collection of input arguments.
        /// The result can be chained with other builders in a workflow.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            if (buildContext == null)
            {
                throw new InvalidOperationException("No valid build context was provided.");
            }

            var source = arguments.First();
            var subjectFactory = BuildSubject(source);
            var parameterType = source.Type.GetGenericArguments()[0];
            var subjectExpression = buildContext.AddVariable(Name, subjectFactory);
            return Expression.Call(typeof(SubjectBuilder), "Process", new[] { parameterType }, source, subjectExpression);
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source, ISubject<TSource> subject)
        {
            return source.Do(subject);
        }
    }
}
