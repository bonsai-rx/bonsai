using System;

namespace Bonsai
{
    /// <summary>
    /// Specifies a type used to replace the class this attribute is bound to.
    /// This attribute is meant for internal use only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ProxyTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyTypeAttribute"/> class
        /// with the specified destination type.
        /// </summary>
        /// <param name="destination">
        /// The <see cref="Type"/> that should be used to replace the class this
        /// attribute is bound to.
        /// </param>
        public ProxyTypeAttribute(Type destination)
        {
            Destination = destination;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> that should be used to replace the class this
        /// attribute is bound to.
        /// </summary>
        public Type Destination { get; private set; }
    }
}
