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
        ExpressionBuilderGraph workflow;
        static readonly Attribute[] emptyAttributes = new Attribute[0];
        static readonly PropertyDescriptor[] emptyProperties = new PropertyDescriptor[0];

        public WorkflowTypeDescriptor(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            var builder = instance as WorkflowExpressionBuilder;
            if (builder != null)
            {
                workflow = builder.Workflow;
            }
            else workflow = (ExpressionBuilderGraph)instance;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(emptyAttributes);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
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

        class ExternalizedPropertyDescriptor : PropertyDescriptor
        {
            readonly Type propertyType;
            readonly object[] instances;
            readonly PropertyDescriptor[] properties;

            public ExternalizedPropertyDescriptor(string name, Attribute[] attributes, PropertyDescriptor[] descriptors, object[] components)
                : base(name, attributes)
            {
                instances = components;
                properties = descriptors;
                propertyType = descriptors.Length > 0 ? descriptors[0].PropertyType : null;
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get { return typeof(WorkflowExpressionBuilder); }
            }

            public override object GetValue(object component)
            {
                var result = default(object);
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i] == null) continue;
                    var value = properties[i].GetValue(instances[i]);
                    if (result == null) result = value;
                    else if (!result.Equals(value)) return null;
                }

                return result;
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override Type PropertyType
            {
                get { return propertyType; }
            }

            public override void ResetValue(object component)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i] == null) continue;
                    properties[i].ResetValue(instances[i]);
                }
            }

            public override void SetValue(object component, object value)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i] == null) continue;
                    properties[i].SetValue(instances[i], value);
                }
            }

            public override bool ShouldSerializeValue(object component)
            {
                return true;
            }
        }
    }
}
