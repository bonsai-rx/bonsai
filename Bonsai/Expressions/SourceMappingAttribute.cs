using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Expressions
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SourceMappingAttribute : Attribute
    {
        public const string DefaultPropertyName = "MemberSelector";

        public SourceMappingAttribute()
            : this(DefaultPropertyName)
        {
        }

        public SourceMappingAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; private set; }
    }
}
