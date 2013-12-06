using Bonsai.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class WorkflowTypeDescriptionProvider : TypeDescriptionProvider
    {
        static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(WorkflowExpressionBuilder));

        public WorkflowTypeDescriptionProvider()
            : base(parentProvider)
        {
        }

        public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
        {
            return new WorkflowTypeDescriptor(instance);
        }

        class WorkflowTypeDescriptor : CustomTypeDescriptor
        {
            WorkflowExpressionBuilder builder;
            static readonly Attribute[] emptyAttributes = new Attribute[0];

            public WorkflowTypeDescriptor(object instance)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException("instance");
                }

                builder = (WorkflowExpressionBuilder)instance;
            }

            public override PropertyDescriptorCollection GetProperties()
            {
                return GetProperties(emptyAttributes);
            }

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                var properties = from node in builder.Workflow
                                 let sourceBuilder = node.Value as SourceBuilder
                                 where sourceBuilder != null
                                 let property = sourceBuilder.Generator as WorkflowProperty
                                 where property != null
                                 let name = property.Name
                                 where !string.IsNullOrEmpty(name)
                                 select new WorkflowPropertyDescriptor(name, emptyAttributes, property);
                return new PropertyDescriptorCollection(properties.ToArray());
            }
        }

        class WorkflowPropertyDescriptor : PropertyDescriptor
        {
            Type propertyType;
            Func<object> getValue;
            Action<object> setValue;

            public WorkflowPropertyDescriptor(string name, Attribute[] attrs, WorkflowProperty property)
                : base(name, attrs)
            {
                var propertyExpression = Expression.Constant(property);
                var valueProperty = Expression.Property(propertyExpression, "Value");
                var getterBody = Expression.Convert(valueProperty, typeof(object));
                getValue = Expression.Lambda<Func<object>>(getterBody).Compile();

                propertyType = valueProperty.Type;
                var setterParameter = Expression.Parameter(typeof(object));
                var setterBody = Expression.Assign(valueProperty, Expression.Convert(setterParameter, propertyType));
                setValue = Expression.Lambda<Action<object>>(setterBody, setterParameter).Compile();
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
                return getValue();
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
            }

            public override void SetValue(object component, object value)
            {
                setValue(value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return true;
            }
        }
    }
}
