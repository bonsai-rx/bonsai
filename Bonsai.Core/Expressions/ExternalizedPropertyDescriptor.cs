﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
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
            for (int i = 0; i < properties.Length; i++)
            {
                if (!properties[i].CanResetValue(component))
                {
                    return false;
                }
            }

            return true;
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
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].ShouldSerializeValue(component))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
