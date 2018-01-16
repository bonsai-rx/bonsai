using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor
{
    public interface IScriptEnvironment
    {
        AssemblyName AssemblyName { get; }

        bool DebugScripts { get; set; }

        void AddAssemblyReferences(IEnumerable<string> assemblyReferences);
    }
}
