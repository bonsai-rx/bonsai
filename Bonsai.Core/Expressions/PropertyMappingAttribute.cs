using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Specifies that a class provides a property that defines the collection of property
    /// mappings that will be used to dynamically assign values in a workflow element.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PropertyMappingAttribute : Attribute
    {
        const string DefaultPropertyName = "PropertyMappings";

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMappingAttribute"/> class.
        /// </summary>
        public PropertyMappingAttribute()
            : this(DefaultPropertyName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMappingAttribute"/> class with
        /// the specified name for the property defining the collection of property mappings.
        /// </summary>
        public PropertyMappingAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// Gets or sets the name of the property defining the collection of property mappings.
        /// </summary>
        public string PropertyName { get; private set; }
    }
}
