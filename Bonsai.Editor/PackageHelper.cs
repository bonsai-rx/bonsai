using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Bonsai.Design;

namespace Bonsai.Editor
{
    class PackageHelper
    {
        const string DefaultProbingPath = "Packages";

        static AssemblyName GetAssemblyName(string fileName)
        {
            try { return AssemblyName.GetAssemblyName(fileName); }
            catch (BadImageFormatException)
            {
                return null;
            }
        }

        static IEnumerable<string> GetPackageFiles(string path)
        {
            path = Path.GetFullPath(path);
            if (!string.Equals(path.TrimEnd('\\'),
                               Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\'),
                               StringComparison.InvariantCultureIgnoreCase))
            {
                var bonsaiAssemblyName = typeof(LoadableElement).Assembly.GetName();
                var bonsaiDesignAssemblyName = typeof(DialogTypeVisualizer).Assembly.GetName();
                return from fileName in Directory.GetFiles(path, "*.dll")
                       let assemblyName = GetAssemblyName(fileName)
                       where assemblyName != null &&
                             assemblyName.FullName != bonsaiAssemblyName.FullName &&
                             assemblyName.FullName != bonsaiDesignAssemblyName.FullName
                       select fileName;
            }

            return Enumerable.Empty<string>();
        }

        public static IEnumerable<string> GetPackageFiles()
        {
            IEnumerable<string> packageFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            packageFiles = packageFiles.Concat(GetPackageFiles(DefaultProbingPath));
            packageFiles = packageFiles.Concat(GetPackageFiles(Environment.CurrentDirectory));
            return packageFiles;
        }
    }
}
