using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Determines the type of visualizer used to display the target of the attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class TypeVisualizerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeVisualizerAttribute"/> class with the
        /// specified visualizer type.
        /// </summary>
        /// <param name="visualizer">
        /// The <see cref="Type"/> of a visualizer that can be used to display the type
        /// this attribute is bound to.
        /// </param>
        public TypeVisualizerAttribute(Type visualizer)
            : this(visualizer != null ? visualizer.AssemblyQualifiedName : null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeVisualizerAttribute"/> class with the
        /// specified visualizer type name.
        /// </summary>
        /// <param name="visualizerTypeName">
        /// A <see cref="String"/> specifying the assembly qualified name of a visualizer that can
        /// be used to display the type this attribute is bound to.
        /// </param>
        public TypeVisualizerAttribute(string visualizerTypeName)
        {
            VisualizerTypeName = visualizerTypeName;
        }

        /// <summary>
        /// Gets the assembly qualified name of the visualizer class.
        /// </summary>
        public string VisualizerTypeName { get; private set; }

        /// <summary>
        /// Gets or sets the assembly qualified name of the type that is the target of the attribute.
        /// </summary>
        public string TargetTypeName { get; set; }

        /// <summary>
        /// Gets or sets the type that is the target of the attribute.
        /// </summary>
        public Type Target
        {
            get { return Type.GetType(TargetTypeName); }
            set { TargetTypeName = value != null ? value.AssemblyQualifiedName : null; }
        }
    }
}
