using Bonsai.Dag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class PropertyMappingNameConverter : StringConverter
    {
        static Node<ExpressionBuilder, ExpressionBuilderArgument> GetBuilderNode(
            ITypeDescriptorContext context,
            ExpressionBuilderGraph nodeBuilderGraph)
        {
            var mapping = (PropertyMapping)context.Instance;
            return (from node in nodeBuilderGraph
                    let builder = ExpressionBuilder.Unwrap(node.Value) as PropertyMappingBuilder
                    where builder != null && builder.PropertyMappings.Contains(mapping)
                    select node).SingleOrDefault();
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            var nodeBuilderGraph = (ExpressionBuilderGraph)context.GetService(typeof(ExpressionBuilderGraph));
            if (nodeBuilderGraph != null)
            {
                var builderNode = GetBuilderNode(context, nodeBuilderGraph);
                return builderNode != null && builderNode.Successors.Count > 0;
            }

            return false;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var nodeBuilderGraph = (ExpressionBuilderGraph)context.GetService(typeof(ExpressionBuilderGraph));
            if (nodeBuilderGraph != null)
            {
                var builderNode = GetBuilderNode(context, nodeBuilderGraph);
                if (builderNode != null)
                {
                    var properties = from successor in builderNode.Successors
                                     let element = ExpressionBuilder.GetWorkflowElement(successor.Target.Value)
                                     where element != null
                                     from descriptor in TypeDescriptor.GetProperties(element).Cast<PropertyDescriptor>()
                                     where descriptor.IsBrowsable && !descriptor.IsReadOnly
                                     select descriptor.Name;
                    return new StandardValuesCollection(properties.Distinct().ToArray());
                }
            }

            return base.GetStandardValues(context);
        }
    }
}
