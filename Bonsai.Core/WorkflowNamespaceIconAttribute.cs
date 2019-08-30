using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai
{
    /// <summary>
    /// Specifies the icon that should represent a given namespace in the assembly
    /// this attribute is bound to when drawing the workflow.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class WorkflowNamespaceIconAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowNamespaceIconAttribute"/> class with
        /// the name of the default icon resource used to represent namespaces in the assembly this
        /// attribute is bound to.
        /// </summary>
        /// <param name="name">
        /// The name of the default icon resource used to represent namespaces in the assembly
        /// this attribute is bound to.
        /// </param>
        public WorkflowNamespaceIconAttribute(string name)
            : this(string.Empty, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowNamespaceIconAttribute"/> class with
        /// a specific namespace that will be matched against workflow element types in the assembly this
        /// attribute is bound to, and the name of the icon resource used to represent the namespace
        /// in case of a successful match.
        /// </summary>
        /// <param name="ns">
        /// The specific namespace that will be matched against workflow element types in the assembly this
        /// attribute is bound to. If this value is <b>null</b> or empty, the match will be successful
        /// against any namespace.
        /// </param>
        /// <param name="name">
        /// The name of the icon resource used to represent the namespace in the assembly
        /// this attribute is bound to.
        /// </param>
        public WorkflowNamespaceIconAttribute(string ns, string name)
        {
            Namespace = ns;
            ResourceName = name;
        }

        /// <summary>
        /// Gets the optional namespace that will be matched against a workflow element type.
        /// If this value is <b>null</b> or empty, the match will be successful against any
        /// namespace in the assembly this attribute is bound to.
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Gets the name of the icon resource used to represent a namespace in the assembly
        /// this attribute is bound to.
        /// </summary>
        public string ResourceName { get; private set; }
    }
}
