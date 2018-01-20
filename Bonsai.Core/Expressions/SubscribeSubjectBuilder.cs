using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that generates a sequence of values
    /// by subscribing to a shared subject.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Source)]
    [XmlType("SubscribeSubject", Namespace = Constants.XmlNamespace)]
    [Description("Generates a sequence of values by subscribing to a shared subject.")]
    public class SubscribeSubjectBuilder : ZeroArgumentExpressionBuilder, IRequireSubject
    {
        IBuildContext buildContext;

        /// <summary>
        /// Gets or sets the name of the shared subject.
        /// </summary>
        [TypeConverter(typeof(SubjectNameConverter))]
        [Description("The name of the shared subject.")]
        public string Name { get; set; }

        IBuildContext IRequireBuildContext.BuildContext
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

            var subjectExpression = buildContext.GetVariable(name);
            var parameterType = subjectExpression.Type.GetGenericArguments()[0];
            return Expression.Call(typeof(SubscribeSubjectBuilder), "Process", new[] { parameterType }, subjectExpression);
        }

        static IObservable<TSource> Process<TSource>(ISubject<TSource> subject)
        {
            return subject;
        }
    }
}
