using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TransformAttribute : Attribute
    {
        public TransformAttribute()
            : this("Process")
        {
        }

        public TransformAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; private set; }
    }
}
