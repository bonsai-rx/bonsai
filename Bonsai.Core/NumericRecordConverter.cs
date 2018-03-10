using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai
{
    /// <summary>
    /// Provides a type converter to convert numeric records to and from various other representations.
    /// </summary>
    public class NumericRecordConverter : TypeConverter
    {
        FieldInfo[] fieldCache;

        static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        static Type GetRecordType(Type type)
        {
            if (IsNullable(type))
            {
                type = type.GetGenericArguments()[0];
            }

            return type;
        }

        FieldInfo[] GetRecordFields(Type type)
        {
            return fieldCache ?? (fieldCache = GetRecordType(type).GetFields(BindingFlags.Instance | BindingFlags.Public));
        }

        /// <summary>
        /// Returns whether this converter can convert an object of the given type to
        /// the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="sourceType">
        /// A <see cref="Type"/> that represents the type you want to convert from.
        /// </param>
        /// <returns>
        /// <b>true</b> if this converter can perform the conversion; otherwise, <b>false</b>.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified
        /// context and culture information.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="culture">
        /// The <see cref="CultureInfo"/> to use as the current culture.
        /// </param>
        /// <param name="value">The <see cref="Object"/> to convert.</param>
        /// <returns>An <see cref="Object"/> that represents the converted value.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var valueString = value as string;
            if (valueString != null && context != null)
            {
                valueString = valueString.Trim();
                var propertyType = context.PropertyDescriptor.PropertyType;
                if (!string.IsNullOrEmpty(valueString))
                {
                    var type = GetRecordType(propertyType);
                    var instance = Activator.CreateInstance(type);
                    var properties = GetProperties(null, instance);
                    var fieldValues = valueString.Split(new[] { culture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    if (fieldValues.Length == properties.Count)
                    {
                        for (int i = 0; i < properties.Count; i++)
                        {
                            var fieldValue = Convert.ChangeType(fieldValues[i], properties[i].PropertyType, culture);
                            properties[i].SetValue(instance, fieldValue);
                        }

                        return instance;
                    }
                }
                else if (IsNullable(propertyType))
                {
                    return null;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Converts the given value object to the specified type, using the specified
        /// context and culture information.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="culture">
        /// A <see cref="CultureInfo"/>. If <b>null</b> is passed, the current culture
        /// is assumed.
        /// </param>
        /// <param name="value">The <see cref="Object"/> to convert.</param>
        /// <param name="destinationType">The <see cref="Type"/> to convert the value parameter to.</param>
        /// <returns>An <see cref="Object"/> that represents the converted value.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value != null && destinationType == typeof(string))
            {
                var properties = GetProperties(null, value).Cast<PropertyDescriptor>();
                return string.Join(
                    culture.TextInfo.ListSeparator,
                    properties.Select(property => Convert.ToString(property.GetValue(value), culture)));
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// Creates an instance of the type that this <see cref="TypeConverter"/>
        /// is associated with, using the specified context, given a set of property
        /// values for the object.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="propertyValues">An <see cref="IDictionary"/> of new property values.</param>
        /// <returns>
        /// An <see cref="Object"/> representing the given <see cref="IDictionary"/>,
        /// or <b>null</b> if the object cannot be created.
        /// </returns>
        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (context == null) return null;

            var type = GetRecordType(context.PropertyDescriptor.PropertyType);
            var instance = Activator.CreateInstance(type);
            var properties = GetProperties(null, instance);
            foreach (PropertyDescriptor property in properties)
            {
                var value = Convert.ChangeType(propertyValues[property.Name], property.PropertyType);
                property.SetValue(instance, value);
            }

            return instance;
        }

        /// <summary>
        /// Returns whether changing a value on this object requires a call to <see cref="TypeConverter.CreateInstance(IDictionary)"/>
        /// to create a new value, using the specified context.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <returns>
        /// <b>true</b> if changing a property on this object requires a call to <see cref="TypeConverter.CreateInstance(IDictionary)"/>
        /// to create a new value; otherwise, <b>false</b>.
        /// </returns>
        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Returns whether this object supports properties, using the specified context.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <returns>
        /// <b>true</b> if <see cref="TypeConverter.GetProperties(Object)"/> should be called
        /// to find the properties of this object; otherwise, <b>false</b>.
        /// </returns>
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Returns a collection of properties for the type of numeric record specified by the
        /// value parameter, using the specified context and attributes.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="value">
        /// An <see cref="Object"/> that specifies the type of numeric record for which to get properties.
        /// </param>
        /// <param name="attributes">
        /// An array of type <see cref="Attribute"/> that is used as a filter.
        /// </param>
        /// <returns>
        /// A <see cref="PropertyDescriptorCollection"/> with the properties that are exposed
        /// for this data type, or <b>null</b> if there are no properties.
        /// </returns>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            if (value == null) return null;

            var valueType = value.GetType();
            var fields = GetRecordFields(valueType);
            var fieldNames = fields.Select(field => field.Name).ToArray();
            var fieldProperties = fields
                .Select(field => new FieldPropertyDescriptor(valueType, field, context))
                .Where(field => field.Attributes.Contains(attributes))
                .ToArray();
            return new PropertyDescriptorCollection(fieldProperties).Sort(fieldNames);
        }

        class FieldPropertyDescriptor : SimplePropertyDescriptor
        {
            FieldInfo field;
            ITypeDescriptorContext recordContext;

            public FieldPropertyDescriptor(Type componentType, FieldInfo fieldInfo, ITypeDescriptorContext context)
                : base(componentType, fieldInfo.Name, fieldInfo.FieldType)
            {
                field = fieldInfo;
                recordContext = context;
            }

            public override object GetValue(object component)
            {
                return field.GetValue(component);
            }

            public override void SetValue(object component, object value)
            {
                field.SetValue(component, value);
                if (recordContext != null)
                {
                    recordContext.PropertyDescriptor.SetValue(recordContext.Instance, component);
                }
            }
        }

        /// <summary>
        /// Represents a class used for providing custom property metadata for an object.
        /// </summary>
        protected class PropertyDescriptorWrapper : SimplePropertyDescriptor
        {
            PropertyDescriptor descriptor;

            /// <summary>
            /// Initializes a new instance of the <see cref="PropertyDescriptorWrapper"/> class.
            /// </summary>
            /// <param name="name">The name of the property.</param>
            /// <param name="descr">The underlying property used for the redirection.</param>
            /// <param name="attributes">An <see cref="Array"/> with the attributes to associate with the property.</param>
            public PropertyDescriptorWrapper(string name, PropertyDescriptor descr, Attribute[] attributes)
                : base(descr.ComponentType, name, descr.PropertyType, attributes)
            {
                descriptor = descr;
            }

            /// <summary>
            /// Gets the current value of the property on a component.
            /// </summary>
            /// <param name="component">The component with the property for which to retrieve the value.</param>
            /// <returns>The value of a property for a given component.</returns>
            public override object GetValue(object component)
            {
                return descriptor.GetValue(component);
            }

            /// <summary>
            /// Sets a property of the component to a different value.
            /// </summary>
            /// <param name="component">The component with the property value that is to be set.</param>
            /// <param name="value">The new value.</param>
            public override void SetValue(object component, object value)
            {
                descriptor.SetValue(component, value);
            }
        }
    }
}
