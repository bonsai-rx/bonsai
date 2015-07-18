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
    [WorkflowElementCategory(ElementCategory.Sink)]
    [XmlType("PublishSubject", Namespace = Constants.XmlNamespace)]
    [Description("Broadcasts the values of an observable sequence to multiple subscribers using a shared subject.")]
    public class PublishSubjectBuilder : SingleArgumentExpressionBuilder, IRequireBuildContext
    {
        BuildContext buildContext;

        /// <summary>
        /// Gets or sets the name of the shared subject.
        /// </summary>
        [Description("The name of the shared subject.")]
        public string Name { get; set; }

        BuildContext IRequireBuildContext.BuildContext
        {
            get { return buildContext; }
            set { buildContext = value; }
        }

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

            var name = Name;
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException("A valid variable name must be specified.");
            }

            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var subjectFactory = Expression.New(typeof(Subject<>).MakeGenericType(parameterType));
            var subjectExpression = buildContext.AddVariable(name, subjectFactory);
            return Expression.Call(typeof(PublishSubjectBuilder), "Process", new[] { parameterType }, source, subjectExpression);
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source, Subject<TSource> subject)
        {
            return source.Do(subject);
        }
    }
}
