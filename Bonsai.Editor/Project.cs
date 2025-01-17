using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Bonsai.Design;

namespace Bonsai.Editor
{
    static class Project
    {
        const string DefaultWorkflowNamespace = "Unspecified";
        internal const string BonsaiExtension = ".bonsai";
        internal const string LayoutFileName = "layout";
        internal const string ExtensionsDirectory = "Extensions";
        internal const string DefinitionsDirectory = "Definitions";

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
            return Path.Combine(GetWorkflowSettingsDirectory(fileName), LayoutFileName);
        }

        [Obsolete]
        internal static string GetLegacyLayoutConfigPath(string fileName)
        {
            return Path.ChangeExtension(fileName, Path.GetExtension(fileName) + "." + LayoutFileName);
        }

        internal static string GetDefinitionsTempPath()
        {
            return Path.Combine(Path.GetTempPath(), DefinitionsDirectory + "." + GuidHelper.GetProcessGuid().ToString());
        }

        public static IEnumerable<WorkflowElementDescriptor> EnumerateExtensionWorkflows(string basePath)
        {
            IEnumerable<string> workflowFiles;
            try { workflowFiles = Directory.EnumerateFiles(basePath, "*" + BonsaiExtension, SearchOption.AllDirectories); }
            catch (UnauthorizedAccessException) { yield break; }
            catch (DirectoryNotFoundException) { yield break; }

            foreach (var fileName in workflowFiles)
            {
                var description = string.Empty;
                try
                {
                    using var reader = XmlReader.Create(fileName, new XmlReaderSettings { IgnoreWhitespace = true });
                    reader.ReadStartElement(typeof(WorkflowBuilder).Name);
                    if (reader.Name == nameof(WorkflowBuilder.Description))
                    {
                        reader.ReadStartElement();
                        description = reader.Value;
                    }
                }
                catch (SystemException) { continue; }

                var relativePath = PathConvert.GetProjectPath(fileName);
                var fileNamespace = Path.GetDirectoryName(relativePath)
                                        .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                                        .Replace(Path.DirectorySeparatorChar, ExpressionHelper.MemberSeparator.First());
                if (string.IsNullOrEmpty(fileNamespace)) fileNamespace = DefaultWorkflowNamespace;

                yield return new WorkflowElementDescriptor
                {
                    Name = Path.GetFileNameWithoutExtension(relativePath),
                    Namespace = fileNamespace,
                    FullyQualifiedName = relativePath,
                    Description = description,
                    ElementTypes = new[] { ~ElementCategory.Workflow }
                };
            }
        }
    }
}
