using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    class NumericAggregateConverter : TypeConverter
    {
        FieldInfo[] fieldCache;

        static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        static Type GetUnderlyingType(ITypeDescriptorContext context)
        {
            var type = context.PropertyDescriptor.PropertyType;
            if (IsNullable(type))
            {
                type = type.GetGenericArguments()[0];
            }

            return type;
        }

        FieldInfo[] GetFields(ITypeDescriptorContext context)
        {
            var type = GetUnderlyingType(context);
            return GetFields(type);
        }

        FieldInfo[] GetFields(Type type)
        {
            return fieldCache ?? (fieldCache = type.GetFields(BindingFlags.Instance | BindingFlags.Public));
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var valueString = value as string;
            if (valueString != null)
            {
                valueString = valueString.Trim();
                if (!string.IsNullOrEmpty(valueString))
                {
                    var type = GetUnderlyingType(context);
                    var fields = GetFields(type);
                    var fieldValues = valueString.Split(new[] { culture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    if (fieldValues.Length == fields.Length)
                    {
                        var instance = Activator.CreateInstance(type);
                        for (int i = 0; i < fields.Length; i++)
                        {
                            var fieldValue = Convert.ChangeType(fieldValues[i], fields[i].FieldType, culture);
                            fields[i].SetValue(instance, fieldValue);
                        }

                        return instance;
                    }
                }
                else if (IsNullable(context.PropertyDescriptor.PropertyType))
                {
                    return null;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value != null && destinationType == typeof(string))
            {
                var fields = GetFields(context);
                return string.Join(culture.TextInfo.ListSeparator, fields.Select(field => Convert.ToString(field.GetValue(value), culture)));
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            var type = GetUnderlyingType(context);
            var fields = GetFields(type);
            var instance = Activator.CreateInstance(type);
            foreach (var field in fields)
            {
                var value = Convert.ChangeType(propertyValues[field.Name], field.FieldType);
                field.SetValue(instance, value);
            }

            return instance;
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            if (value == null) return null;

            var valueType = value.GetType();
            var fields = GetFields(context);
            var fieldNames = fields.Select(field => field.Name).ToArray();
            var fieldProperties = fields.Select(field => new FieldPropertyDescriptor(valueType, field, context)).ToArray();
            return new PropertyDescriptorCollection(fieldProperties).Sort(fieldNames);
        }

        class FieldPropertyDescriptor : SimplePropertyDescriptor
        {
            FieldInfo field;
            ITypeDescriptorContext structContext;

            public FieldPropertyDescriptor(Type componentType, FieldInfo fieldInfo, ITypeDescriptorContext context)
                : base(componentType, fieldInfo.Name, fieldInfo.FieldType)
            {
                field = fieldInfo;
                structContext = context;
            }

            public override object GetValue(object component)
            {
                return field.GetValue(component);
            }

            public override void SetValue(object component, object value)
            {
                field.SetValue(component, value);
                structContext.PropertyDescriptor.SetValue(structContext.Instance, component);
            }
        }
    }
}
