using System;
using System.IO;

namespace Bonsai.Editor
{
    static class Project
    {
        internal const string BonsaiExtension = ".bonsai";
        internal const string LayoutExtension = ".layout";

        public static string GetCurrentBaseDirectory()
        {
            return GetCurrentBaseDirectory(out _);
        }

        public static string GetCurrentBaseDirectory(out bool currentDirectoryRestricted)
        {
            var currentDirectory = Path.GetFullPath(Environment.CurrentDirectory).TrimEnd('\\');
            var appDomainBaseDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\');
            currentDirectoryRestricted = currentDirectory == appDomainBaseDirectory;
            if (!EditorSettings.IsRunningOnMono)
            {
                var systemPath = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.System)).TrimEnd('\\');
                var systemX86Path = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)).TrimEnd('\\');
                currentDirectoryRestricted |= currentDirectory == systemPath || currentDirectory == systemX86Path;
            }

            return !currentDirectoryRestricted
                ? currentDirectory
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public static string GetWorkflowBaseDirectory(string fileName)
        {
            return Path.GetDirectoryName(fileName);
        }

        public static string GetWorkflowSettingsDirectory(string fileName)
        {
            return Path.Combine(BonsaiExtension, Path.GetFileNameWithoutExtension(fileName));
        }

        public static string GetLayoutConfigPath(string fileName)
        {
            return Path.Combine(GetWorkflowSettingsDirectory(fileName), LayoutExtension);
        }

        [Obsolete]
        internal static string GetLegacyLayoutConfigPath(string fileName)
        {
            return Path.ChangeExtension(fileName, Path.GetExtension(fileName) + LayoutExtension);
        }
    }
}
