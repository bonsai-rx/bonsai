using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Expressions
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PropertyMappingAttribute : Attribute
    {
        public const string DefaultPropertyName = "PropertyMappings";

        public PropertyMappingAttribute()
            : this(DefaultPropertyName)
        {
        }

        public PropertyMappingAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; private set; }
    }
}
