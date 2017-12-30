using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class WorkflowTypeDescriptor : CustomTypeDescriptor
    {
        AttributeCollection attributes;
        ExpressionBuilderGraph workflow;
        static readonly Attribute[] emptyAttributes = new Attribute[0];
        static readonly PropertyDescriptor[] emptyProperties = new PropertyDescriptor[0];

        public WorkflowTypeDescriptor(object instance, params Attribute[] attrs)
        {
            attributes = new AttributeCollection(attrs ?? emptyAttributes);
            var builder = instance as IWorkflowExpressionBuilder;
            if (builder != null)
            {
                workflow = builder.Workflow;
            }
            else workflow = (ExpressionBuilderGraph)instance;
        }

        public override AttributeCollection GetAttributes()
        {
            return attributes;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(emptyAttributes);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            if (workflow == null) return base.GetProperties(attributes);
            var properties = (from node in workflow
                              let property = ExpressionBuilder.Unwrap(node.Value) as ExternalizedProperty
                              where property != null
                              let name = property.Name
                              where !string.IsNullOrEmpty(name)
                              let targetComponents = node.Successors.Select(edge => ExpressionBuilder.GetWorkflowElement(edge.Target.Value)).ToArray()
                              let aggregateProperties = GetAggregateProperties(property, targetComponents)
                              let aggregateAttributes = GetAggregateAttributes(aggregateProperties)
                              select new ExternalizedPropertyDescriptor(property.Name, aggregateAttributes, aggregateProperties, targetComponents))
                              .Where(descriptor => descriptor.PropertyType != null);
            return new PropertyDescriptorCollection(properties.ToArray());
        }

        static PropertyDescriptor[] GetAggregateProperties(ExternalizedProperty property, object[] components)
        {
            var propertyType = default(Type);
            var properties = new PropertyDescriptor[components.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                var descriptor = TypeDescriptor.GetProperties(components[i])[property.MemberName];
                if (propertyType == null)
                {
                    propertyType = descriptor.PropertyType;
                }
                else if (descriptor == null || descriptor.PropertyType != propertyType)
                {
                    return emptyProperties;
                }

                properties[i] = descriptor;
            }

            return properties;
        }

        static Attribute[] GetAggregateAttributes(PropertyDescriptor[] properties)
        {
            var result = emptyAttributes;
            for (int i = 0; i < properties.Length; i++)
            {
                var attributes = properties[i].Attributes;
                if (result.Length != attributes.Count)
                {
                    result = new Attribute[attributes.Count];
                }
                attributes.CopyTo(result, 0);
            }

            return result;
        }
    }
}
