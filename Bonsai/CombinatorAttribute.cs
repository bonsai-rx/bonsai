using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CombinatorAttribute : Attribute
    {
        public CombinatorAttribute()
            : this("Process")
        {
        }

        public CombinatorAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; private set; }
    }
}
