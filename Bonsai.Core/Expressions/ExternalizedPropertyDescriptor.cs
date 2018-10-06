using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class ExternalizedPropertyDescriptor : PropertyDescriptor
    {
        readonly Type componentType;
        readonly Type propertyType;
        readonly bool isReadOnly;
        readonly object[] instances;
        readonly PropertyDescriptor[] properties;
        readonly string description;
        readonly string category;

        public ExternalizedPropertyDescriptor(ExternalizedProperty property, Attribute[] attributes, PropertyDescriptor[] descriptors, object[] components)
            : base(property.Name, attributes)
        {
            instances = components;
            properties = descriptors;
            componentType = descriptors.Length > 0 ? descriptors[0].ComponentType : null;
            propertyType = descriptors.Length > 0 ? descriptors[0].PropertyType : null;
            isReadOnly = descriptors.Length > 0 ? descriptors[0].IsReadOnly : true;
            description = property.Description;
            category = property.Category;
        }

        public override string Description
        {
            get
            {
                if (!string.IsNullOrEmpty(description)) return description;
                else return base.Description;
            }
        }

        public override string Category
        {
            get
            {
                if (!string.IsNullOrEmpty(category)) return category;
                else return base.Category;
            }
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
            get { return componentType; }
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
            get { return isReadOnly; }
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
