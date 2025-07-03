using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bonsai.Design
{
    static class TypeVisitor
    {
        static IEnumerable<PropertyInfo> GetProperties(Type type, BindingFlags bindingAttr)
        {
            var properties = type.GetProperties(bindingAttr).Except(type.GetDefaultMembers().OfType<PropertyInfo>());
            if (type.IsInterface)
            {
                properties = properties.Concat(type
                    .GetInterfaces()
                    .SelectMany(i => i.GetProperties(bindingAttr)));
            }
            return properties;
        }

        internal static void VisitMember(this Type type, Action<MemberInfo, Type> visitor)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                                               .OrderBy(member => member.MetadataToken))
            {
                visitor(field, field.FieldType);
            }

            foreach (var property in GetProperties(type, BindingFlags.Instance | BindingFlags.Public)
                                                  .OrderBy(member => member.MetadataToken))
            {
                visitor(property, property.PropertyType);
            }

            if (type is IExtendedVisitableType extendedType)
            {
                foreach (var member in extendedType.GetExtendedMembers())
                {
                    if (member is IExtendedVisitableMemberInfo extendedMemberInfo)
                    {
                        visitor(member, extendedMemberInfo.ExtendedVisitableMemberType);
                    }
                }
            }
        }
    }
}
