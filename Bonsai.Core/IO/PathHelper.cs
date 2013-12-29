using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Bonsai.IO
{
    public static class PathHelper
    {
        public static void EnsureDirectory(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

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

        public static string AppendSuffix(string path, string suffix)
        {
            var directory = Path.GetDirectoryName(path);
            var basePath = Path.GetFileNameWithoutExtension(path);
            var suffixPath = basePath + suffix;
            if (!string.IsNullOrEmpty(basePath))
            {
                return Path.Combine(directory, Path.ChangeExtension(suffixPath, Path.GetExtension(path)));
            }

            return suffixPath;
        }

        public static string AppendTimestamp(string path, DateTimeOffset timestamp)
        {
            return AppendSuffix(path, timestamp.ToString("o").Replace(':', '_'));
        }

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
