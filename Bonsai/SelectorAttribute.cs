using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SelectorAttribute : Attribute
    {
        public SelectorAttribute()
            : this("Process")
        {
        }

        public SelectorAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; private set; }
    }
}
