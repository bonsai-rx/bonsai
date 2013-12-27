using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Specifies that a class provides a method that can combine one or more observable
    /// sequences into a new observable sequence.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CombinatorAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CombinatorAttribute"/> class with the
        /// default combinator method name.
        /// </summary>
        public CombinatorAttribute()
            : this("Process")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CombinatorAttribute"/> class with the
        /// specified combinator method name.
        /// </summary>
        /// <param name="methodName">
        /// The name of the method that can combine one or more observable sequences into a new
        /// observable sequence.
        /// </param>
        public CombinatorAttribute(string methodName)
        {
            MethodName = methodName;
        }

        /// <summary>
        /// Gets the name of the method that can combine one or more observable sequences into a
        /// new observable sequence.
        /// </summary>
        public string MethodName { get; private set; }
    }
}
