using Bonsai.Dag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    abstract class MappingNameConverter<TMapping> : StringConverter where TMapping : class
    {
        static readonly Attribute[] ExternalizableAttributes = new Attribute[]
        {
            ExternalizableAttribute.Default,
            DesignTimeVisibleAttribute.Yes
        };

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
                var mapping = context.Instance as TMapping;
                if (mapping != null)
                {
                    var builderNode = GetBuilderNode(mapping, nodeBuilderGraph);
                    return builderNode != null && builderNode.Successors.Count > 0;
                }
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
                                     select from descriptor in TypeDescriptor.GetProperties(element, ExternalizableAttributes)
                                                                             .Cast<PropertyDescriptor>()
                                            where descriptor.IsBrowsable && !descriptor.IsReadOnly
                                            select descriptor;
                    HashSet<PropertyDescriptor> propertySet = null;
                    foreach (var group in properties)
                    {
                        if (propertySet == null)
                        {
                            propertySet = new HashSet<PropertyDescriptor>(group, PropertyDescriptorComparer.Instance);
                        }
                        else propertySet.IntersectWith(group);
                    }
                    return new StandardValuesCollection(propertySet.Select(property => property.Name).ToArray());
                }
            }

            return base.GetStandardValues(context);
        }

        class PropertyDescriptorComparer : IEqualityComparer<PropertyDescriptor>
        {
            public static readonly PropertyDescriptorComparer Instance = new PropertyDescriptorComparer();

            public bool Equals(PropertyDescriptor x, PropertyDescriptor y)
            {
                if (x == null) return y == null;
                else return y != null && x.Name == y.Name && x.PropertyType == y.PropertyType;
            }

            public int GetHashCode(PropertyDescriptor obj)
            {
                var hash = 313;
                hash = hash * 523 + EqualityComparer<string>.Default.GetHashCode(obj.Name);
                hash = hash * 523 + EqualityComparer<Type>.Default.GetHashCode(obj.PropertyType);
                return hash;
            }
        }
    }
}
