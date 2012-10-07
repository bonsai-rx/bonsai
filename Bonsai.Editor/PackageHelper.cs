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

        static IEnumerable<string> GetPackageFiles(string path)
        {
            path = Path.GetFullPath(path);
            if (!string.Equals(path.TrimEnd('\\'),
                               Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\'),
                               StringComparison.InvariantCultureIgnoreCase))
            {
                var bonsaiAssemblyName = typeof(LoadableElement).Assembly.GetName();
                var bonsaiDesignAssemblyName = typeof(DialogTypeVisualizer).Assembly.GetName();
                return Directory
                    .GetFiles(path, "*.dll")
                    .Where(fileName => AssemblyName.GetAssemblyName(fileName).FullName != bonsaiAssemblyName.FullName &&
                                       AssemblyName.GetAssemblyName(fileName).FullName != bonsaiDesignAssemblyName.FullName);
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
