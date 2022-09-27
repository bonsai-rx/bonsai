using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Reflection;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that generates a sequence of values
    /// by subscribing to a shared subject.
    /// </summary>
    [DefaultProperty(nameof(Name))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Generates a sequence of values by subscribing to a shared subject.")]
    public class SubscribeSubject : ZeroArgumentExpressionBuilder, IRequireSubject
    {
        IBuildContext buildContext;

        /// <summary>
        /// Gets or sets the name of the shared subject.
        /// </summary>
        [Category("Subject")]
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
            if (string.IsNullOrEmpty(name)) return EmptyExpression.Instance;
            var subjectExpression = buildContext.GetVariable(name);

            var processMethod = GetType().GetMethod(nameof(Process), BindingFlags.Static | BindingFlags.NonPublic);
            if (processMethod.IsGenericMethod)
            {
                var typeArgument = subjectExpression.Type.GetGenericArguments()[0];
                processMethod = processMethod.MakeGenericMethod(typeArgument);
            }
            else
            {
                var parameterType = processMethod.GetParameters()[0].ParameterType;
                if (!parameterType.IsAssignableFrom(subjectExpression.Type))
                {
                    var targetType = parameterType.GetGenericArguments()[0];
                    throw new InvalidOperationException("The type of the subscribed subject must be assignable to " + targetType);
                }
            }

            return Expression.Call(processMethod, subjectExpression);
        }

        static IObservable<TSource> Process<TSource>(ISubject<TSource> subject)
        {
            return subject;
        }
    }

    /// <summary>
    /// Represents an expression builder that generates a sequence of values
    /// by subscribing to a shared subject of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [WorkflowElementIcon(typeof(SubscribeSubject), nameof(SubscribeSubject))]
    public class SubscribeSubject<T> : SubscribeSubject
    {
        /// <summary>
        /// Gets or sets the name of the shared subject.
        /// </summary>
        [Category("Subject")]
        [TypeConverter(typeof(SubjectNameConverter))]
        [Description("The name of the shared subject.")]
        public new string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        static IObservable<T> Process(IObservable<T> subject)
        {
            return subject;
        }
    }

    [Obsolete]
    [ProxyType(typeof(SubscribeSubject))]
    public class SubscribeSubjectBuilder : SubscribeSubject
    {
    }
}
