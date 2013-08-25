using Bonsai.Design;
using Bonsai.Editor.Properties;
using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms.Design;

namespace Bonsai.Editor
{
    public class MappingTab : PropertyTab
    {
        public override PropertyDescriptorCollection GetProperties(object component)
        {
            return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
        }

        public override PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes)
        {
            return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attributes)
        {
            var properties = new List<PropertyDescriptor>();
            var selectionModel = (WorkflowSelectionModel)context.GetService(typeof(WorkflowSelectionModel));
            if (selectionModel != null)
            {
                var builder = selectionModel.SelectedNode.Value;
                var builderProperties = TypeDescriptor.GetProperties(builder);
                var builderAttributes = TypeDescriptor.GetAttributes(builder);

                var propertyMapAttribute = (PropertyMappingAttribute)builderAttributes[typeof(PropertyMappingAttribute)];
                if (propertyMapAttribute != null)
                {
                    var propertyMapProperty = builderProperties[propertyMapAttribute.PropertyName];
                    if (propertyMapProperty == null || propertyMapProperty.PropertyType != typeof(PropertyMappingCollection))
                    {
                        throw new InvalidOperationException("The property name specified in PropertyMappingAttribute does not exist or has an invalid type.");
                    }

                    var propertyMappings = (PropertyMappingCollection)propertyMapProperty.GetValue(builder);
                    var componentProperties = TypeDescriptor.GetProperties(component);
                    foreach (var descriptor in componentProperties.Cast<PropertyDescriptor>()
                                                                  .Where(descriptor => descriptor.IsBrowsable &&
                                                                                       !descriptor.IsReadOnly))
                    {
                        var propertyAttributes = new Attribute[]
                        {
                            new EditorAttribute(typeof(MemberSelectorEditor), typeof(UITypeEditor)),
                            new CategoryAttribute("Property"),
                            new DescriptionAttribute(descriptor.Description),
                            new TypeConverterAttribute(typeof(MappingConverter))
                        };
                        properties.Add(new PropertyMappingDescriptor(descriptor.Name, propertyMappings, propertyAttributes));
                    }
                }

                var sourceMappingAttribute = (SourceMappingAttribute)builderAttributes[typeof(SourceMappingAttribute)];
                if (sourceMappingAttribute != null)
                {
                    var sourceMappingProperty = builderProperties[sourceMappingAttribute.PropertyName];
                    if (sourceMappingProperty == null || sourceMappingProperty.PropertyType != typeof(string))
                    {
                        throw new InvalidOperationException("The property name specified in SourceMappingAttribute does not exist or has an invalid type.");
                    }

                    var propertyAttributes = new Attribute[]
                        {
                            new EditorAttribute(typeof(MemberSelectorEditor), typeof(UITypeEditor)),
                            new CategoryAttribute("Source"),
                            new DescriptionAttribute("The inner properties that will be selected for each element of the sequence."),
                            new TypeConverterAttribute(typeof(MappingConverter))
                        };
                    properties.Add(new SelectorPropertyDescriptor(sourceMappingProperty, builder, propertyAttributes));
                }
            }

            return new PropertyDescriptorCollection(properties.ToArray());
        }

        public override string TabName
        {
            get { return "Mappings"; }
        }

        public override Bitmap Bitmap
        {
            get
            {
                return Resources.PropertyMappingIcon;
            }
        }

        class MappingConverter : StringConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (value == null) return "(unmapped)";
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        class SelectorPropertyDescriptor : PropertyDescriptor
        {
            object builder;
            PropertyDescriptor selectorProperty;

            public SelectorPropertyDescriptor(PropertyDescriptor descriptor, object component, Attribute[] attributes)
                : base(descriptor, attributes)
            {
                selectorProperty = descriptor;
                builder = component;
            }

            public override bool CanResetValue(object component)
            {
                return ShouldSerializeValue(component);
            }

            public override Type ComponentType
            {
                get { return selectorProperty.ComponentType; }
            }

            public override object GetValue(object component)
            {
                return selectorProperty.GetValue(builder);
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override Type PropertyType
            {
                get { return typeof(string); }
            }

            public override void ResetValue(object component)
            {
                selectorProperty.SetValue(builder, null);
            }

            public override void SetValue(object component, object value)
            {
                selectorProperty.SetValue(builder, value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return selectorProperty.GetValue(builder) != null;
            }
        }

        class PropertyMappingDescriptor : PropertyDescriptor
        {
            PropertyMappingCollection owner;

            public PropertyMappingDescriptor(string name, PropertyMappingCollection propertyMappings, Attribute[] attributes)
                : base(name, attributes)
            {
                owner = propertyMappings;
            }

            public override bool CanResetValue(object component)
            {
                return ShouldSerializeValue(component);
            }

            public override Type ComponentType
            {
                get { return typeof(ExpressionBuilder); }
            }

            public override object GetValue(object component)
            {
                if (owner.Contains(Name))
                {
                    return owner[Name].Selector;
                }

                return null;
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override Type PropertyType
            {
                get { return typeof(string); }
            }

            public override void ResetValue(object component)
            {
                owner.Remove(Name);
            }

            public override void SetValue(object component, object value)
            {
                PropertyMapping mapping;
                if (!owner.Contains(Name))
                {
                    mapping = new PropertyMapping { Name = Name };
                    owner.Add(mapping);
                }
                else mapping = owner[Name];

                mapping.Selector = (string)value;
            }

            public override bool ShouldSerializeValue(object component)
            {
                return owner.Contains(Name);
            }
        }
    }
}
