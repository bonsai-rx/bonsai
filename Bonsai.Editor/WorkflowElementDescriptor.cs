using System;
using System.Diagnostics;

namespace Bonsai.Editor
{
    [Serializable]
    [DebuggerDisplay("Name = {Name}, Namespace = {Namespace}")]
    public struct WorkflowElementDescriptor
    {
        public string Name { get; set; }

        public string Namespace { get; set; }

        public string FullyQualifiedName { get; set; }

        public string Description { get; set; }

        public ElementCategory[] ElementTypes { get; set; }
    }
}
