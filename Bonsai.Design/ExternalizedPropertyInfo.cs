using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bonsai.Expressions;

namespace Bonsai.Design
{
    internal class ExternalizedPropertyInfo : MemberInfo, IExtendedVisitableMemberInfo
    {
        readonly ExternalizedPropertyDescriptor property;
        internal ExternalizedPropertyInfo(ExternalizedPropertyDescriptor property)
        {
            this.property = property;
        }
        public override string Name => property.Name;

        public Type ExtendedVisitableMemberType => property.PropertyType;

        public override Type DeclaringType => property.ComponentType;

        public override Type ReflectedType => property.ComponentType;

        public override MemberTypes MemberType => MemberTypes.Custom;

        public override object[] GetCustomAttributes(bool inherit)
        {
            return property.Attributes.Cast<Attribute>().ToArray();
        }
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return property.Attributes.Cast<Attribute>().Where(attributeType.IsInstanceOfType).ToArray();
        }
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return property.Attributes.Cast<Attribute>().Any(attributeType.IsInstanceOfType);
        }

        public override int MetadataToken => 0;



    }
}
