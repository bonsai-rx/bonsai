using System;

namespace Bonsai
{
    [AttributeUsage(AttributeTargets.Class)]
    sealed class ProxyTypeAttribute : Attribute
    {
        public ProxyTypeAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; set; }
    }
}
