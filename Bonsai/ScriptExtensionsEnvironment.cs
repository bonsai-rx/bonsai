using Bonsai.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai
{
    class ScriptExtensionsEnvironment : IScriptEnvironment, IDisposable, IServiceProvider
    {
        TempDirectory assemblyFolder;

        public ScriptExtensionsEnvironment(string path)
        {
            assemblyFolder = new TempDirectory(path);
        }

        public string AssemblyDirectory
        {
            get { return assemblyFolder.Path; }
        }

        public bool DebugScripts { get; set; }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IScriptEnvironment))
            {
                return this;
            }

            return null;
        }

        public void Dispose()
        {
            assemblyFolder.Dispose();
        }
    }
}
