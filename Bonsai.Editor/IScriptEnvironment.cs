using System.Collections.Generic;
using System.Reflection;

namespace Bonsai.Editor
{
    public interface IScriptEnvironment
    {
        string ProjectFileName { get; }

        AssemblyName AssemblyName { get; }

        bool DebugScripts { get; set; }

        void AddAssemblyReferences(IEnumerable<string> assemblyReferences);
    }
}
