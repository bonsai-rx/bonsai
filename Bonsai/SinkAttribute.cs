using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SinkAttribute : Attribute
    {
        public SinkAttribute()
            : this("Process")
        {
        }

        public SinkAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; private set; }
    }
}
