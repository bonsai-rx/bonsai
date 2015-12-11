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
            var properties = from node in workflow
                             let property = ExpressionBuilder.Unwrap(node.Value) as ExternalizedProperty
                             where property != null
                             let name = property.Name
                             where !string.IsNullOrEmpty(name)
                             select new WorkflowPropertyDescriptor(name, GetWorkflowPropertyAttributes(property), property);
            return new PropertyDescriptorCollection(properties.ToArray());
        }

        static Attribute[] GetWorkflowPropertyAttributes(ExternalizedProperty property)
        {
            var valueProperty = TypeDescriptor.GetProperties(property)["Value"];
            if (valueProperty != null)
            {
                var attributes = new Attribute[valueProperty.Attributes.Count];
                valueProperty.Attributes.CopyTo(attributes, 0);
                return attributes;
            }

            return emptyAttributes;
        }
    }
}
