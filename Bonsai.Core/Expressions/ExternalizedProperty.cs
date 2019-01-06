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
    [Description("Represents a property that has been externalized from a workflow element.")]
    public class ExternalizedProperty : ExpressionBuilder, INamedElement, IArgumentBuilder, IExternalizedMappingBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: 1);
        readonly ExternalizedMapping mapping = new ExternalizedMapping();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalizedProperty"/> class.
        /// </summary>
        public ExternalizedProperty()
        {
        }

        /// <summary>
        /// Gets or sets the name of the externalized class member.
        /// </summary>
        [Browsable(false)]
        public string MemberName
        {
            get { return mapping.Name; }
            set { mapping.Name = value; }
        }

        /// <summary>
        /// Gets or sets the name of the externalized property.
        /// </summary>
        [Category("Design")]
        [Externalizable(false)]
        [Description("The name of the externalized property. When set, the property will appear on the pages of a nested workflow.")]
        public string Name
        {
            get { return mapping.DisplayName; }
            set { mapping.DisplayName = value; }
        }

        /// <summary>
        /// Gets or sets an optional description for the externalized property.
        /// </summary>
        [Category("Design")]
        [Externalizable(false)]
        [Description("The optional description for the externalized property.")]
        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        public string Description
        {
            get { return mapping.Description; }
            set { mapping.Description = value; }
        }

        /// <summary>
        /// Gets or sets an optional category for the externalized property.
        /// </summary>
        [Category("Design")]
        [Externalizable(false)]
        [Description("The optional category used to group the externalized property.")]
        public string Category
        {
            get { return mapping.Category; }
            set { mapping.Category = value; }
        }

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
            var source = arguments.FirstOrDefault();
            if (source == null)
            {
                return EmptyExpression.Instance;
            }
            else return source;
        }

        IEnumerable<ExternalizedMapping> IExternalizedMappingBuilder.GetExternalizedProperties()
        {
            if (!string.IsNullOrEmpty(mapping.DisplayName))
            {
                yield return mapping;
            }
        }

        bool IArgumentBuilder.BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument)
        {
            return BuildArgument(source, successor, out argument, string.Empty);
        }

        internal bool BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument, string sourceSelector)
        {
            var workflowElement = GetWorkflowElement(successor.Target.Value);
            var instance = Expression.Constant(workflowElement);
            argument = BuildPropertyMapping(source, instance, MemberName, sourceSelector);
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
    [Obsolete]
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
