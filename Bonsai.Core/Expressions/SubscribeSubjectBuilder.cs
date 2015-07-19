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
    public class SubscribeSubjectBuilder : ZeroArgumentExpressionBuilder, INamedElement, IRequireBuildContext
    {
        BuildContext buildContext;

        /// <summary>
        /// Gets or sets the name of the shared subject.
        /// </summary>
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

            var subjectExpression = buildContext.GetVariable(name);
            var parameterType = subjectExpression.Type.GetGenericArguments()[0];
            return Expression.Call(typeof(SubscribeSubjectBuilder), "Process", new[] { parameterType }, subjectExpression);
        }

        static IObservable<TSource> Process<TSource>(Subject<TSource> subject)
        {
            return subject;
        }

        class SubjectNameConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            bool GetCallContext(ExpressionBuilderGraph source, ExpressionBuilderGraph target, Stack<ExpressionBuilderGraph> context)
            {
                context.Push(source);
                if (source == target)
                {
                    return true;
                }

                foreach (var node in source)
                {
                    var workflowBuilder = ExpressionBuilder.Unwrap(node.Value) as WorkflowExpressionBuilder;
                    if (workflowBuilder != null)
                    {
                        if (GetCallContext(workflowBuilder.Workflow, target, context))
                        {
                            return true;
                        }
                    }
                }

                context.Pop();
                return false;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                if (context != null)
                {
                    var workflow = (ExpressionBuilderGraph)context.GetService(typeof(ExpressionBuilderGraph));
                    var workflowBuilder = (WorkflowBuilder)context.GetService(typeof(WorkflowBuilder));
                    if (workflow != null && workflowBuilder != null)
                    {
                        var callContext = new Stack<ExpressionBuilderGraph>();
                        if (GetCallContext(workflowBuilder.Workflow, workflow, callContext))
                        {
                            var names = (from level in callContext
                                         from node in level
                                         let publishBuilder = ExpressionBuilder.Unwrap(node.Value) as PublishSubjectBuilder
                                         where publishBuilder != null
                                         select publishBuilder.Name)
                                         .Distinct()
                                         .ToList();
                            return new StandardValuesCollection(names);
                        }
                    }
                }

                return base.GetStandardValues(context);
            }
        }
    }
}
