﻿using System;
using System.Diagnostics;

namespace Bonsai.Editor
{
    [Serializable]
    [DebuggerDisplay("Name = {Name}, Namespace = {Namespace}")]
    public struct WorkflowElementDescriptor
    {
        public string Name { get; set; }

        public string Namespace { get; set; }

        [Obsolete]
        public string AssemblyQualifiedName
        {
            get { return FullyQualifiedName; }
            set { FullyQualifiedName = value; }
        }

        public string FullyQualifiedName { get; set; }

        public string Description { get; set; }

        public ElementCategory[] ElementTypes { get; set; }
    }
}
