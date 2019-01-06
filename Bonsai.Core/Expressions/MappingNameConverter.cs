using Bonsai.Dag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    abstract class MappingNameConverter<TMapping> : StringConverter
    {
        static readonly Attribute[] ExternalizableAttributes = new[] { ExternalizableAttribute.Default };

        protected abstract bool ContainsMapping(ExpressionBuilder builder, TMapping mapping);

        Node<ExpressionBuilder, ExpressionBuilderArgument> GetBuilderNode(TMapping mapping, ExpressionBuilderGraph nodeBuilderGraph)
        {
            foreach (var node in nodeBuilderGraph)
            {
                var builder = ExpressionBuilder.Unwrap(node.Value);
                if (ContainsMapping(builder, mapping)) return node;

                var workflowBuilder = builder as IWorkflowExpressionBuilder;
                if (workflowBuilder != null && workflowBuilder.Workflow != null)
                {
                    var builderNode = GetBuilderNode(mapping, workflowBuilder.Workflow);
                    if (builderNode != null) return builderNode;
                }
            }

            return null;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            var nodeBuilderGraph = (ExpressionBuilderGraph)context.GetService(typeof(ExpressionBuilderGraph));
            if (nodeBuilderGraph != null)
            {
                var mapping = (TMapping)context.Instance;
                var builderNode = GetBuilderNode(mapping, nodeBuilderGraph);
                return builderNode != null && builderNode.Successors.Count > 0;
            }

            return false;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var nodeBuilderGraph = (ExpressionBuilderGraph)context.GetService(typeof(ExpressionBuilderGraph));
            if (nodeBuilderGraph != null)
            {
                var mapping = (TMapping)context.Instance;
                var builderNode = GetBuilderNode(mapping, nodeBuilderGraph);
                if (builderNode != null)
                {
                    var properties = from successor in builderNode.Successors
                                     let element = ExpressionBuilder.GetWorkflowElement(successor.Target.Value)
                                     where element != null
                                     from descriptor in TypeDescriptor.GetProperties(element, ExternalizableAttributes)
                                                                      .Cast<PropertyDescriptor>()
                                     where descriptor.IsBrowsable && !descriptor.IsReadOnly
                                     select descriptor.Name;
                    return new StandardValuesCollection(properties.Distinct().ToArray());
                }
            }

            return base.GetStandardValues(context);
        }
    }
}
