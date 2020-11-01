using Bonsai.Configuration;
using Bonsai.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bonsai
{
    class ScriptExtensionsEnvironment : IScriptEnvironment, IServiceProvider
    {
        readonly ScriptExtensions extensions;

        internal ScriptExtensionsEnvironment(ScriptExtensions owner)
        {
            extensions = owner;
        }

        public string ProjectFileName
        {
            get { return extensions.ProjectFileName; }
        }

        public AssemblyName AssemblyName
        {
            get { return extensions.AssemblyName; }
        }

        public bool DebugScripts
        {
            get { return extensions.DebugScripts; }
            set { extensions.DebugScripts = value; }
        }

        public void AddAssemblyReferences(IEnumerable<string> assemblyReferences)
        {
            extensions.AddAssemblyReferences(assemblyReferences);
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IScriptEnvironment))
            {
                return this;
            }

            return null;
        }
    }
}
