using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SourceAttribute : Attribute
    {
        public SourceAttribute()
            : this("Generate")
        {
        }

        public SourceAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; private set; }
    }
}
