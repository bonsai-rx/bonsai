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
    /// Represents an expression builder that pushes a sequence of values
    /// into a shared subject.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Sink)]
    [XmlType("MulticastSubject", Namespace = Constants.XmlNamespace)]
    [Description("Pushes a sequence of values into a shared subject.")]
    public class MulticastSubjectBuilder : SingleArgumentExpressionBuilder, INamedElement, IRequireBuildContext
    {
        BuildContext buildContext;

        /// <summary>
        /// Gets or sets the name of the shared subject.
        /// </summary>
        [Category("Design")]
        [Externalizable(false)]
        [TypeConverter(typeof(SubjectNameConverter))]
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
            var subjectExpression = buildContext.GetVariable(name);
            var parameterType = subjectExpression.Type.GetGenericArguments()[0];
            return Expression.Call(typeof(MulticastSubjectBuilder), "Process", new[] { parameterType }, source, subjectExpression);
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source, ISubject<TSource> subject)
        {
            return source.Do(subject);
        }
    }
}
