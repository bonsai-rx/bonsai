using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace Bonsai.Expressions
{
    class WorkflowTypeDescriptor : CustomTypeDescriptor
    {
        readonly AttributeCollection attributes;
        readonly ExpressionBuilderGraph workflow;
        static readonly Attribute[] EmptyAttributes = new Attribute[0];
        static readonly Attribute[] ExternalizableAttributes = new Attribute[] { BrowsableAttribute.Yes, ExternalizableAttribute.Default };

        public WorkflowTypeDescriptor(object instance, params Attribute[] attrs)
        {
            attributes = new AttributeCollection(attrs ?? EmptyAttributes);
            if (instance is IWorkflowExpressionBuilder builder)
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
                             let workflowProperty = GetWorkflowProperty(property, targetProperties, targetComponents)
                             where workflowProperty != null
                             select workflowProperty;
            return new PropertyDescriptorCollection(properties.ToArray());
        }

        static ExternalizedPropertyDescriptor GetWorkflowProperty(ExternalizedMapping property, PropertyDescriptorCollection[] targetProperties, object[] targetComponents)
        {
            var propertyType = default(Type);
            var properties = new PropertyDescriptor[targetProperties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                var descriptor = targetProperties[i][property.Name];
                if (descriptor == null) return null;

                if (propertyType == null)
                {
                    propertyType = descriptor.PropertyType;
                }
                else if (descriptor.PropertyType != propertyType)
                {
                    if (descriptor is XmlPropertyDescriptor || propertyType == typeof(XElement))
                    {
                        return IncludeWorkflowBuilder.GetDeferredProperties(property, targetProperties, targetComponents);
                    }
                    else return null;
                }

                properties[i] = descriptor;
            }

            return new ExternalizedPropertyDescriptor(property, properties, targetComponents);
        }
    }
}
