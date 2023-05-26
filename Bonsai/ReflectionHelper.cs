using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bonsai
{
    static class ReflectionHelper
    {
        public static CustomAttributeData[] GetCustomAttributesData(this Type type, bool inherit)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var attributeLists = new List<IList<CustomAttributeData>>();
            while (type != null)
            {
                attributeLists.Add(CustomAttributeData.GetCustomAttributes(type));
                type = inherit ? type.BaseType : null;
            }

            var offset = 0;
            var count = attributeLists.Sum(attributes => attributes.Count);
            var result = new CustomAttributeData[count];
            for (int i = 0; i < attributeLists.Count; i++)
            {
                attributeLists[i].CopyTo(result, offset);
                offset += attributeLists[i].Count;
            }

            return result;
        }

        public static IEnumerable<CustomAttributeData> OfType<TAttribute>(this IEnumerable<CustomAttributeData> customAttributes)
        {
            var attributeTypeName = typeof(TAttribute).FullName;
            return customAttributes.Where(attribute => attribute.AttributeType.FullName == attributeTypeName);
        }

        public static bool IsDefined(this CustomAttributeData[] customAttributes, Type attributeType)
        {
            return GetCustomAttributeData(customAttributes, attributeType) != null;
        }

        public static CustomAttributeData GetCustomAttributeData(
            this CustomAttributeData[] customAttributes,
            Type attributeType)
        {
            if (customAttributes == null)
            {
                throw new ArgumentNullException(nameof(customAttributes));
            }

            return Array.Find(
                customAttributes,
                attribute => attribute.AttributeType.FullName == attributeType.FullName);
        }

        public static object GetConstructorArgument(this CustomAttributeData attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException(nameof(attribute));
            }

            return attribute.ConstructorArguments.Count > 0 ? attribute.ConstructorArguments[0].Value : null;
        }

        public static bool IsMatchSubclassOf(this Type type, Type baseType)
        {
            var typeName = baseType.AssemblyQualifiedName;
            if (type.AssemblyQualifiedName == typeName)
            {
                return false;
            }

            while (type != null)
            {
                if (type.AssemblyQualifiedName == typeName)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
