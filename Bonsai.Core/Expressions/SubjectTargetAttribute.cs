using System;

namespace Bonsai.Expressions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    sealed class SubjectTargetAttribute : Attribute
    {
        public SubjectTargetAttribute(Type targetType)
            : this(targetType?.AssemblyQualifiedName)
        {
        }

        public SubjectTargetAttribute(string targetTypeName)
        {
            TargetTypeName = targetTypeName;
        }

        public string TargetTypeName { get; private set; }

        public Type TargetType
        {
            get { return Type.GetType(TargetTypeName); }
        }
    }
}
