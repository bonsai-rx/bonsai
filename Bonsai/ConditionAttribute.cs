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
        {
        }
    }
}
