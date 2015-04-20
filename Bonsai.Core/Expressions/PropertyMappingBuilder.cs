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
    [DefaultProperty("PropertyMappings")]
    [WorkflowElementCategory(ElementCategory.Property)]
    [XmlType("PropertyMapping", Namespace = Constants.XmlNamespace)]
    public class PropertyMappingBuilder : SingleArgumentExpressionBuilder, IArgumentBuilder
    {
        readonly PropertyMappingCollection propertyMappings = new PropertyMappingCollection();

        [XmlArrayItem("Property")]
        public PropertyMappingCollection PropertyMappings
        {
            get { return propertyMappings; }
        }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            return arguments.First();
        }

        bool IArgumentBuilder.BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument)
        {
            return BuildArgument(source, successor, out argument);
        }

        internal virtual bool BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument)
        {
            argument = source;
            var workflowElement = GetWorkflowElement(successor.Target.Value);
            var instance = Expression.Constant(workflowElement);
            foreach (var mapping in propertyMappings)
            {
                argument = BuildPropertyMapping(argument, instance, mapping.Name, mapping.Selector);
            }

            return false;
        }
    }
}
