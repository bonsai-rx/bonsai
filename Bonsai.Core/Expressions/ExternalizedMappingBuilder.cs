using Bonsai.Dag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Specifies a set of properties to be externalized from a workflow element.
    /// </summary>
    [DefaultProperty("ExternalizedProperties")]
    [WorkflowElementCategory(ElementCategory.Property)]
    [XmlType("ExternalizedMapping", Namespace = Constants.XmlNamespace)]
    [Description("Specifies a set of properties to be externalized from a workflow element.")]
    public class ExternalizedMappingBuilder : ZeroArgumentExpressionBuilder, INamedElement, IArgumentBuilder, IExternalizedMappingBuilder
    {
        readonly ExternalizedMappingCollection externalizedProperties = new ExternalizedMappingCollection();

        /// <summary>
        /// Gets the collection of properties to be externalized from the workflow element.
        /// </summary>
        [Externalizable(false)]
        [XmlArrayItem("Property")]
        [Description("Specifies the set of properties to be externalized.")]
        public ExternalizedMappingCollection ExternalizedProperties
        {
            get { return externalizedProperties; }
        }

        string INamedElement.Name
        {
            get
            {
                if (externalizedProperties.Count > 0)
                {
                    return string.Join(
                        ExpressionHelper.ArgumentSeparator,
                        externalizedProperties.Select(property => string.IsNullOrEmpty(property.ExternalizedName)
                            ? property.Name
                            : property.ExternalizedName));
                }

                return GetElementDisplayName(GetType());
            }
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
            return EmptyExpression.Instance;
        }

        IEnumerable<ExternalizedMapping> IExternalizedMappingBuilder.GetExternalizedProperties()
        {
            return externalizedProperties;
        }

        bool IArgumentBuilder.BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument)
        {
            argument = source;
            return false;
        }
    }
}
