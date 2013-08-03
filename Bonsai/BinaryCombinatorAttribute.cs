using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BinaryCombinatorAttribute : Attribute
    {
        public BinaryCombinatorAttribute()
            : this("Process")
        {
        }

        public BinaryCombinatorAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; private set; }
    }
}
