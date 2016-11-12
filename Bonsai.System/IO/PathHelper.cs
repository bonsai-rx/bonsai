using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Bonsai.IO
{
    /// <summary>
    /// Provides a set of static methods for creating and manipulating directory
    /// and file names.
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Checks that all folders along the specified path exist and attempts to
        /// create any missing ones.
        /// </summary>
        /// <param name="path">The path to check for missing folders.</param>
        public static void EnsureDirectory(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// Appends the specified well known suffix type to a file name without
        /// modifying the extension.
        /// </summary>
        /// <param name="path">The file name on which to append the suffix.</param>
        /// <param name="suffixType">The suffix type to append.</param>
        /// <returns>
        /// The new file name where <paramref name="path"/> has been modified
        /// to end with the specified <paramref name="suffixType"/> but without
        /// modifying the file extension.
        /// </returns>
        public static string AppendSuffix(string path, PathSuffix suffixType)
        {
            switch (suffixType)
            {
                case PathSuffix.FileCount: return AppendFileCount(path);
                case PathSuffix.Timestamp: return AppendTimestamp(path, HighResolutionScheduler.Now);
                case PathSuffix.None:
                default: return path;
            }
        }

        /// <summary>
        /// Appends the specified suffix to a file name without modifying
        /// its original extension.
        /// </summary>
        /// <param name="path">The file name on which to append the suffix.</param>
        /// <param name="suffix">The suffix to append.</param>
        /// <returns>
        /// The new file name where <paramref name="path"/> has been modified
        /// to end with <paramref name="suffix"/> but without modifying the
        /// file extension.
        /// </returns>
        public static string AppendSuffix(string path, string suffix)
        {
            if (string.IsNullOrEmpty(path)) return suffix;
            var directory = Path.GetDirectoryName(path);
            var basePath = Path.GetFileNameWithoutExtension(path);
            var extension = Path.GetExtension(path);
            var suffixPath = basePath + suffix;
            if (!string.IsNullOrEmpty(extension))
            {
                suffixPath = Path.ChangeExtension(suffixPath, extension);
            }
            return Path.Combine(directory, suffixPath);
        }

        /// <summary>
        /// Appends a timestamp suffix to a file name without modifying
        /// its original extension.
        /// </summary>
        /// <param name="path">The file name on which to append the suffix.</param>
        /// <param name="timestamp">The timestamp to use for generating the suffix.</param>
        /// <returns>
        /// The new file name where <paramref name="path"/> has been modified
        /// to end with the round-trip representation of the specified
        /// <paramref name="timestamp"/> but without modifying the file extension.
        /// </returns>
        public static string AppendTimestamp(string path, DateTimeOffset timestamp)
        {
            return AppendSuffix(path, timestamp.ToString("o").Replace(':', '_'));
        }

        /// <summary>
        /// Appends a file count suffix to a file name without modifying
        /// its original extension.
        /// </summary>
        /// <param name="path">The file name on which to append the suffix.</param>
        /// <returns>
        /// The new file name where <paramref name="path"/> has been modified
        /// to end with the number of files starting with the specified file name
        /// in the containing folder but without modifying the file extension.
        /// </returns>
        public static string AppendFileCount(string path)
        {
            var fileCount = 0;
            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory)) directory = ".";

            if (Directory.Exists(directory))
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                var extension = Path.GetExtension(path);
                fileCount = Directory.GetFiles(directory, fileName + "*" + extension).Length;
            }

            return AppendSuffix(path, fileCount.ToString(CultureInfo.InvariantCulture));
        }
    }
}
