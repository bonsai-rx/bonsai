using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Bonsai.Scripting.IronPython
{
    static class PythonEngine
    {
        const string ExtensionsPath = "Extensions";
        const string RepositoryPath = "Packages";

        internal static ScriptEngine Create()
        {
            var searchPaths = new List<string>();
            var editorPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var editorExtensionsPath = Path.Combine(editorPath, ExtensionsPath);
            var editorRepositoryPath = Path.Combine(editorPath, RepositoryPath);
            var customExtensionsPath = Path.Combine(Environment.CurrentDirectory, ExtensionsPath);
            searchPaths.Add(customExtensionsPath);
            searchPaths.Add(editorExtensionsPath);

            var libPath = Directory.GetDirectories(editorRepositoryPath, "IronPython.StdLib.*");
            if (libPath.Length == 1)
            {
                var lib = Path.Combine(libPath[0], "content", "Lib");
                var sitePackages = Path.Combine(lib, "site-packages");
                searchPaths.Add(lib);
                searchPaths.Add(sitePackages);
            }

            var engine = global::IronPython.Hosting.Python.CreateEngine();
            engine.SetSearchPaths(searchPaths);
            return engine;
        }
    }
}
