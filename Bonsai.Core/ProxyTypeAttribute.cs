using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai
{
    [AttributeUsage(AttributeTargets.Class)]
    sealed class ProxyTypeAttribute : Attribute
    {
        public ProxyTypeAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; set; }
    }
}
