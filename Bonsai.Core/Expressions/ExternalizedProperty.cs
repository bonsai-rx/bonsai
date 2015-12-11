using Bonsai.Dag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a property that has been externalized from a workflow element.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [WorkflowElementCategory(ElementCategory.Property)]
    [TypeDescriptionProvider(typeof(ExternalizedPropertyTypeDescriptionProvider))]
    [Description("Represents a property that has been externalized from a workflow element.")]
    public abstract class ExternalizedProperty : ExpressionBuilder, INamedElement, IArgumentBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: 1);

        internal ExternalizedProperty()
        {
        }

        /// <summary>
        /// Gets the name of the externalized class member.
        /// </summary>
        [Browsable(false)]
        public string MemberName { get; set; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        [Category("Design")]
        [Description("The name of the property. When set, the property will appear on the pages of a nested workflow.")]
        public string Name { get; set; }

        internal abstract Type ElementType { get; }

        internal abstract WorkflowProperty Property { get; }

        string INamedElement.Name
        {
            get
            {
                var name = Name;
                if (string.IsNullOrWhiteSpace(name)) return MemberName;
                else return name;
            }
        }

        /// <summary>
        /// Gets the range of input arguments that this expression builder accepts.
        /// </summary>
        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
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
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            var sourceType = Property.GetType();
            var sourceExpression = Expression.Constant(Property);
            var sourceAttributes = sourceType.GetCustomAttributes(typeof(SourceAttribute), true);
            var methodName = ((SourceAttribute)sourceAttributes.Single()).MethodName;
            var generateMethod = sourceType.GetMethods(bindingAttributes)
                                           .Single(m => m.Name == methodName && m.GetParameters().Length == 0);
            return Expression.Call(sourceExpression, generateMethod);
        }

        bool IArgumentBuilder.BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument)
        {
            var workflowElement = GetPropertyMappingElement(successor.Target.Value);
            var instance = Expression.Constant(workflowElement);
            argument = BuildPropertyMapping(source, instance, MemberName);
            return false;
        }
    }

    /// <summary>
    /// Represents a strongly typed property that has been externalized from a workflow element.
    /// This class can be used to convert class parameters of workflow elements into explicit
    /// source modules.
    /// </summary>
    /// <typeparam name="TValue">The type of the externalized property value.</typeparam>
    /// <typeparam name="TElement">
    /// The type of the workflow element to which the externalized member is bound to.
    /// </typeparam>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class ExternalizedProperty<TValue, TElement> : ExternalizedProperty
    {
        readonly WorkflowProperty<TValue> property = new WorkflowProperty<TValue>();

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        [Description("The value of the property.")]
        public TValue Value
        {
            get { return property.Value; }
            set { property.Value = value; }
        }

        internal override Type ElementType
        {
            get { return typeof(TElement); }
        }

        internal override WorkflowProperty Property
        {
            get { return property; }
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
            var source = arguments.FirstOrDefault();
            if (source == null)
            {
                return base.Build(arguments);
            }
            else
            {
                var propertySourceType = typeof(IObservable<TValue>);
                if (source.Type != propertySourceType)
                {
                    source = CoerceMethodArgument(propertySourceType, source);
                }

                return source;
            }
        }
    }
}
