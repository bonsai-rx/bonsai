using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConditionAttribute : Attribute
    {
        public ConditionAttribute()
            : this("Process")
        {
        }

        public ConditionAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; private set; }
    }
}
