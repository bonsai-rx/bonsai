using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai
{
    /// <summary>
    /// Specifies the icon that should represent the element this attribute is bound to
    /// when drawing the workflow.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class WorkflowElementIconAttribute : Attribute
    {
        /// <summary>
        /// Specifies the default value for the <see cref="WorkflowElementIconAttribute"/>. This field is read-only.
        /// </summary>
        public static readonly WorkflowElementIconAttribute Default = new WorkflowElementIconAttribute(string.Empty);

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowElementIconAttribute"/> class with
        /// the name of the icon resource that should represent the workflow element.
        /// </summary>
        /// <param name="name">
        /// The name of the icon resource that should represent the workflow element
        /// this attribute is bound to.
        /// </param>
        public WorkflowElementIconAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowElementIconAttribute"/> class with
        /// the name of the icon resource that should represent the workflow element, scoped
        /// by the namespace of the specified type.
        /// </summary>
        /// <param name="type">The type that will be used to scope the name of the icon resource.</param>
        /// <param name="name">
        /// The name of the icon resource that should represent the workflow element
        /// this attribute is bound to.
        /// </param>
        public WorkflowElementIconAttribute(Type type, string name)
            : this(type != null ? type.AssemblyQualifiedName : null, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowElementIconAttribute"/> class with
        /// the name of the icon resource that should represent the workflow element, scoped
        /// by the namespace of the specified type.
        /// </summary>
        /// <param name="typeName">
        /// The fully qualified name of the type that will be used to scope the name
        /// of the icon resource.
        /// </param>
        /// <param name="name">
        /// The name of the icon resource that should represent the workflow element
        /// this attribute is bound to.
        /// </param>
        public WorkflowElementIconAttribute(string typeName, string name)
            : this(name)
        {
            TypeName = typeName;
        }

        /// <summary>
        /// Gets the name of the icon resource that should represent the workflow element
        /// this attribute is bound to.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the optional qualified type name that will be used to scope the name of the icon resource.
        /// </summary>
        public string TypeName { get; private set; }
    }
}
