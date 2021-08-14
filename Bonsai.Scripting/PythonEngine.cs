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
            var libPath = Directory.GetDirectories(Path.Combine(basePath, "../../../"), "IronPython.StdLib.*");
            if (libPath.Length == 1)
            {
                var lib = Path.Combine(libPath[0], $"content/Lib");
                var sitePackages = Path.Combine(lib, "site-packages");
                engine.SetSearchPaths(new[] { lib, sitePackages });
            }
            return engine;
        }
    }
}
