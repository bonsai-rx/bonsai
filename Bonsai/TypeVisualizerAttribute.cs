using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class TypeVisualizerAttribute : Attribute
    {
        public TypeVisualizerAttribute(Type visualizer)
            : this(visualizer != null ? visualizer.AssemblyQualifiedName : null)
        {
        }

        public TypeVisualizerAttribute(string visualizerTypeName)
        {
            VisualizerTypeName = visualizerTypeName;
        }

        public string VisualizerTypeName { get; private set; }

        public string TargetTypeName { get; set; }

        public Type Target
        {
            get { return Type.GetType(TargetTypeName); }
            set { TargetTypeName = value != null ? value.AssemblyQualifiedName : null; }
        }
    }
}
