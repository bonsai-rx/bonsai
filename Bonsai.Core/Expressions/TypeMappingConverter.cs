using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    class TypeMappingConverter : TypeConverter
    {
        static bool HasExtensionTypes(ITypeDescriptorContext context)
        {
            if (context == null) return false;
            var builderType = context.Instance?.GetType() ?? context.PropertyDescriptor.ComponentType;
            if (Attribute.IsDefined(builderType, typeof(XmlIncludeAttribute))) return true;

            var propertyInfo = builderType.GetProperty(context.PropertyDescriptor.Name);
            return propertyInfo != null &&
                (Attribute.IsDefined(propertyInfo, typeof(XmlElementAttribute)) ||
                 Attribute.IsDefined(propertyInfo.PropertyType, typeof(XmlIncludeAttribute)));
        }

        static IEnumerable<Type> GetInstanceTypes(ITypeDescriptorContext context)
        {
            var builderType = context.Instance?.GetType() ?? context.PropertyDescriptor.ComponentType;
            var includeAttributes = (XmlIncludeAttribute[])builderType.GetCustomAttributes(typeof(XmlIncludeAttribute), inherit: true);
            if (includeAttributes.Length > 0)
            {
                return includeAttributes.Select(attribute => attribute.Type);
            }

            var propertyInfo = builderType.GetProperty(context.PropertyDescriptor.Name);
            if (propertyInfo == null) return Enumerable.Empty<Type>();

            var elementAttributes = (XmlElementAttribute[])propertyInfo.GetCustomAttributes(typeof(XmlElementAttribute), inherit: true);
            if (elementAttributes.Length > 0)
            {
                return elementAttributes.Select(attribute => attribute.Type);
            }

            return propertyInfo.PropertyType.GetCustomAttributes<XmlIncludeAttribute>().Select(attribute => attribute.Type);
        }

        static string GetDisplayName(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(TypeMapping<>))
            {
                return GetDisplayName(type.GetGenericArguments()[0]);
            }

            var displayNameAttribute = (DisplayNameAttribute)type.GetCustomAttribute(typeof(DisplayNameAttribute));
            if (!string.IsNullOrEmpty(displayNameAttribute?.DisplayName))
            {
                return displayNameAttribute.DisplayName;
            }

            return type.Name;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) && HasExtensionTypes(context);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string typeName)
            {
                var targetType = GetInstanceTypes(context).FirstOrDefault(
                    type => string.Equals(GetDisplayName(type), typeName, StringComparison.OrdinalIgnoreCase));
                return Activator.CreateInstance(targetType);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is TypeMapping mapping && mapping.TargetType != null && destinationType == typeof(string))
            {
                if (HasExtensionTypes(context))
                {
                    return GetDisplayName(mapping.TargetType);
                }
                else
                {
                    using var provider = new CSharpCodeProvider();
                    var typeRef = new CodeTypeReference(mapping.TargetType);
                    return provider.GetTypeOutput(typeRef);
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return HasExtensionTypes(context);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var includeTypes = GetInstanceTypes(context).Select(Activator.CreateInstance).ToArray();
            return new StandardValuesCollection(includeTypes);
        }
    }
}
