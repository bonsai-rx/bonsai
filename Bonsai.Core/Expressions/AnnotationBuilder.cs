using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Serialization;
using Bonsai.Dag;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a written explanation or critical comment added to the workflow.
    /// </summary>
    [DefaultProperty(nameof(Text))]
    [WorkflowElementCategory(ElementCategory.Property)]
    [XmlType("Annotation", Namespace = Constants.XmlNamespace)]
    [Description("Represents a written explanation or critical comment added to the workflow.")]
    public class AnnotationBuilder : ExpressionBuilder, INamedElement, IArgumentBuilder
    {
        static readonly XmlDocument cdataFactory = new XmlDocument();
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: 0);

        /// <summary>
        /// Gets or sets the name of the annotation node in the workflow.
        /// </summary>
        [Category(nameof(CategoryAttribute.Design))]
        [Description("The name of the annotation node in the workflow.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the text associated with this annotation.
        /// </summary>
        [XmlIgnore]
        [Category(nameof(CategoryAttribute.Design))]
        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        [Description("The text associated with this annotation.")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a CDATA section representing the annotation for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(Text))]
        public XmlCDataSection TextCData
        {
            get { return cdataFactory.CreateCDataSection(Text); }
            set { Text = value?.Data; }
        }

        /// <inheritdoc/>
        public override Range<int> ArgumentRange => argumentRange;

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            return EmptyExpression.Instance;
        }

        bool IArgumentBuilder.BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument)
        {
            argument = source;
            return false;
        }
    }
}
