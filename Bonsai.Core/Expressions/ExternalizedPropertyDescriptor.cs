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
        readonly object[] instances;
        readonly PropertyDescriptor[] properties;
        string description;
        string category;
        bool? isReadOnly;

        private ExternalizedPropertyDescriptor(ExternalizedPropertyDescriptor descr, PropertyDescriptor[] descriptors)
            : base(descr.Name, null)
        {
            instances = descr.instances;
            properties = descriptors;
            description = descr.description;
            category = descr.category;
            isReadOnly = descr.isReadOnly;
        }

        public ExternalizedPropertyDescriptor(ExternalizedMapping property, PropertyDescriptor[] descriptors, object[] components)
            : base(property.ExternalizedName, null)
        {
            instances = components;
            properties = descriptors;
            description = property.Description;
            category = property.Category;
            if (description == string.Empty) description = null;
            if (category == string.Empty) category = null;
        }

        public override string Description
        {
            get
            {
                if (description == null)
                {
                    description = properties[0].Description;
                    if (string.IsNullOrEmpty(description))
                    {
                        description = DescriptionAttribute.Default.Description;
                    }
                    else
                    {
                        for (int i = 1; i < properties.Length; i++)
                        {
                            var other = properties[i].Description;
                            if (!description.Equals(other))
                            {
                                description = DescriptionAttribute.Default.Description;
                                break;
                            }
                        }
                    }
                }

                return description;
            }
        }

        public override string Category
        {
            get
            {
                if (category == null)
                {
                    category = properties[0].Category;
                    if (string.IsNullOrEmpty(category))
                    {
                        category = CategoryAttribute.Default.Category;
                    }
                    else
                    {
                        for (int i = 1; i < properties.Length; i++)
                        {
                            var other = properties[i].Category;
                            if (!category.Equals(other))
                            {
                                category = CategoryAttribute.Default.Category;
                                break;
                            }
                        }
                    }
                }

                return category;
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
            get { return properties[0].ComponentType; }
        }

        public override TypeConverter Converter
        {
            get { return properties[0].Converter; }
        }

        protected override AttributeCollection CreateAttributeCollection()
        {
            return properties.Length > 1 ? new ExternalizedAttributeCollection(this) : properties[0].Attributes;
        }

        public override object GetEditor(Type editorBaseType)
        {
            return properties[0].GetEditor(editorBaseType);
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
            get
            {
                if (!isReadOnly.HasValue)
                {
                    isReadOnly = false;
                    for (int i = 0; i < properties.Length; i++)
                    {
                        if (properties[i].IsReadOnly)
                        {
                            isReadOnly = true;
                            break;
                        }
                    }
                }

                return isReadOnly.Value;
            }
        }

        public override Type PropertyType
        {
            get { return properties[0].PropertyType; }
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

        internal ExternalizedPropertyDescriptor Convert(Converter<PropertyDescriptor, PropertyDescriptor> converter)
        {
            var descriptors = Array.ConvertAll(properties, property =>
            {
                var externalizedDescriptor = property as ExternalizedPropertyDescriptor;
                return externalizedDescriptor != null ? externalizedDescriptor.Convert(converter) : converter(property);
            });
            return new ExternalizedPropertyDescriptor(this, descriptors);
        }

        class ExternalizedAttributeCollection : AttributeCollection
        {
            Dictionary<Type, Attribute> searchCache;
            AttributeCollection[] attributeCollections;
            readonly ExternalizedPropertyDescriptor owner;            

            public ExternalizedAttributeCollection(ExternalizedPropertyDescriptor owner)
                : base(null)
            {
                this.owner = owner;
            }

            public override Attribute this[Type attributeType]
            {
                get
                {
                    if (attributeCollections == null)
                    {
                        attributeCollections = new AttributeCollection[owner.properties.Length];
                        for (int i = 0; i < attributeCollections.Length; i++)
                        {
                            attributeCollections[i] = owner.properties[i].Attributes;
                        }
                    }

                    if (attributeCollections.Length == 0)
                    {
                        return GetDefaultAttribute(attributeType);
                    }

                    Attribute attribute;
                    if (searchCache != null && searchCache.TryGetValue(attributeType, out attribute))
                    {
                        return attribute;
                    }

                    attribute = attributeCollections[0][attributeType];
                    if (attribute == null) return null;
                    for (int i = 1; i < attributeCollections.Length; i++)
                    {
                        var other = attributeCollections[i][attributeType];
                        if (!attribute.Equals(other))
                        {
                            attribute = GetDefaultAttribute(attributeType);
                            break;
                        }
                    }

                    if (searchCache == null) searchCache = new Dictionary<Type, Attribute>();
                    searchCache.Add(attributeType, attribute);
                    return attribute;
                }
            }
        }
    }
}
