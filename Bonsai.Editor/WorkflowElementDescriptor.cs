using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor
{
    [Serializable]
    [DebuggerDisplay("Name = {Name}, AssemblyName = {AssemblyName}")]
    public struct WorkflowElementDescriptor
    {
        public string Name { get; set; }

        public string Namespace { get; set; }

        public string AssemblyQualifiedName { get; set; }

        public string Description { get; set; }

        public ElementCategory[] ElementTypes { get; set; }
    }
}
