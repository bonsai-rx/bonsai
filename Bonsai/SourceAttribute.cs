using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Specifies that a class provides a parameterless method that can generate observable sequences.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SourceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceAttribute"/> class with the
        /// default generator method name.
        /// </summary>
        public SourceAttribute()
            : this("Generate")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceAttribute"/> class with the
        /// specified generator method name.
        /// </summary>
        /// <param name="methodName">
        /// The name of the parameterless method that can be used to generate
        /// observable sequences.
        /// </param>
        public SourceAttribute(string methodName)
        {
            MethodName = methodName;
        }

        /// <summary>
        /// Gets the name of the parameterless method that can be used to generate
        /// observable sequences.
        /// </summary>
        public string MethodName { get; private set; }
    }
}
