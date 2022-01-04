using System;
using System.IO;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides static methods for converting project paths.
    /// </summary>
    public static class PathConvert
    {
        /// <summary>
        /// Converts an absolute path into a relative path, if the absolute path
        /// is relative to the project path.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>
        /// The project relative path, if the path is absolute but located in
        /// any subdirectory of the project path.
        /// </returns>
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
