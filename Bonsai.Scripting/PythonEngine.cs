using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.IO;
using System.Reflection;

namespace Bonsai.Scripting
{
    static class PythonEngine
    {
        internal static ScriptEngine Create()
        {
            var engine = Python.CreateEngine();
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var lib = Path.Combine(basePath, "../../../IronPython.StdLib.2.7.5/content/Lib");
            var sitePackages = Path.Combine(lib, "site-packages");
            engine.SetSearchPaths(new[] { lib, sitePackages });
            return engine;
        }
    }
}
