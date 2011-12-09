using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VideoAnalyzer
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FilterDependencyAttribute : Attribute
    {
        public FilterDependencyAttribute()
        {
        }

        public FilterDependencyAttribute(Type filterType)
            : this(filterType != null ? filterType.AssemblyQualifiedName : null)
        {
        }

        public FilterDependencyAttribute(string filterTypeName)
        {
            FilterTypeName = filterTypeName;
        }

        public string FilterTypeName { get; private set; }
    }
}
