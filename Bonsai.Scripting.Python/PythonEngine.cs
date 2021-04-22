using Microsoft.Scripting.Hosting;
using System.IO;
using System.Reflection;

namespace Bonsai.Scripting.Python
{
    static class PythonEngine
    {
        internal static ScriptEngine Create()
        {
            var engine = global::IronPython.Hosting.Python.CreateEngine();
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var lib = Path.Combine(basePath, "../../../IronPython.StdLib.2.7.11/content/Lib");
            var sitePackages = Path.Combine(lib, "site-packages");
            engine.SetSearchPaths(new[] { lib, sitePackages });
            return engine;
        }
    }
}
