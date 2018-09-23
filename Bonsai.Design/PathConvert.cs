using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design
{
    public static class PathConvert
    {
        public static string GetProjectPath(string path)
        {
            var rootPath = Environment.CurrentDirectory
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            var rootUri = new Uri(rootPath);
            var pathUri = new Uri(path);
            if (rootUri.IsBaseOf(pathUri))
            {
                var relativeUri = rootUri.MakeRelativeUri(pathUri);
                return Uri.UnescapeDataString(relativeUri.ToString().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
            }
            else return path;
        }
    }
}
