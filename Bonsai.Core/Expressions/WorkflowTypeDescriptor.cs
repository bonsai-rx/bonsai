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
        static readonly Attribute[] EmptyAttributes = new Attribute[0];
        static readonly PropertyDescriptor[] EmptyProperties = new PropertyDescriptor[0];
        static readonly Attribute[] ExternalizableAttributes = new Attribute[] { BrowsableAttribute.Yes, ExternalizableAttribute.Default };

        public WorkflowTypeDescriptor(object instance, params Attribute[] attrs)
        {
            attributes = new AttributeCollection(attrs ?? EmptyAttributes);
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
            return GetProperties(EmptyAttributes);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            if (workflow == null) return base.GetProperties(attributes);
            var properties = from node in workflow
                             let externalizedBuilder = ExpressionBuilder.Unwrap(node.Value) as IExternalizedMappingBuilder
                             where externalizedBuilder != null
                             let targetComponents = node.Successors.Select(edge => ExpressionBuilder.GetWorkflowElement(edge.Target.Value)).ToArray()
                             let targetProperties = Array.ConvertAll(targetComponents, component => TypeDescriptor.GetProperties(component, ExternalizableAttributes))
                             from property in externalizedBuilder.GetExternalizedProperties()
                             where !string.IsNullOrEmpty(property.Name)
                             let aggregateProperties = GetAggregateProperties(property, targetProperties)
                             where aggregateProperties.Length > 0
                             select new ExternalizedPropertyDescriptor(property, aggregateProperties, targetComponents);
            return new PropertyDescriptorCollection(properties.ToArray());
        }

        static PropertyDescriptor[] GetAggregateProperties(ExternalizedMapping property, PropertyDescriptorCollection[] components)
        {
            var propertyType = default(Type);
            var properties = new PropertyDescriptor[components.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                var descriptor = components[i][property.Name];
                if (descriptor == null) return EmptyProperties;

                if (propertyType == null)
                {
                    propertyType = descriptor.PropertyType;
                }
                else if (descriptor.PropertyType != propertyType)
                {
                    return EmptyProperties;
                }

                properties[i] = descriptor;
            }

            return properties;
        }
    }
}
