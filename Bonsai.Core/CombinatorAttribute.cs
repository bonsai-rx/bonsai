using Bonsai.Expressions;
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
        /// Initializes a new instance of the <see cref="CombinatorAttribute"/> class using the
        /// default expression builder class.
        /// </summary>
        public CombinatorAttribute()
            : this(typeof(CombinatorBuilder))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CombinatorAttribute"/> class using the
        /// specified expression builder class.
        /// </summary>
        /// <param name="builderType">
        /// The <see cref="Type"/> of the expression builder class used to build expressions
        /// from this combinator.
        /// </param>
        public CombinatorAttribute(Type builderType)
            : this(builderType != null ? builderType.AssemblyQualifiedName : null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CombinatorAttribute"/> class using the
        /// specified expression builder class.
        /// </summary>
        /// <param name="builderTypeName">
        /// The fully qualified name of the expression builder class used to build expressions
        /// from this combinator.
        /// </param>
        public CombinatorAttribute(string builderTypeName)
        {
            MethodName = "Process";
            ExpressionBuilderTypeName = builderTypeName;
        }

        /// <summary>
        /// Gets or sets the name of the method that can combine one or more observable sequences into a
        /// new observable sequence.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Gets the fully qualified name of the expression builder class used to build expressions
        /// from this combinator.
        /// </summary>
        public string ExpressionBuilderTypeName { get; private set; }
    }
}
