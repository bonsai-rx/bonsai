using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Indicates that a class provides a combinator method that can be used to filter out
    /// elements from an observable sequence. This attribute must be used in combination
    /// with a <see cref="CombinatorAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConditionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionAttribute"/> class.
        /// </summary>
        public ConditionAttribute()
        {
        }
    }
}
