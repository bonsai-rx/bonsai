using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor.GraphView
{
    [DebuggerDisplay("{Name}")]
    class ElementIcon
    {
        static readonly ElementIcon Source = new ElementIcon(ElementCategory.Source);
        static readonly ElementIcon Condition = new ElementIcon(ElementCategory.Condition);
        static readonly ElementIcon Transform = new ElementIcon(ElementCategory.Transform);
        static readonly ElementIcon Sink = new ElementIcon(ElementCategory.Sink);
        static readonly ElementIcon Nested = new ElementIcon(ElementCategory.Nested);
        static readonly ElementIcon Property = new ElementIcon(ElementCategory.Property);
        static readonly ElementIcon Combinator = new ElementIcon(ElementCategory.Combinator);
        static readonly ElementIcon Workflow = new ElementIcon(ElementCategory.Workflow);
        public static readonly ElementIcon Include = new ElementIcon(typeof(IncludeWorkflowBuilder));
        public static readonly ElementIcon Default = new ElementIcon(typeof(ExpressionBuilder));

        static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
        static readonly Dictionary<string, string> DefaultManifestResources = GetDefaultManifestResources();
        const string ManifestResourcePrefix = "Bonsai.Editor.Resources";
        const string EditorScriptingNamespace = "Bonsai.ElementIcon.CSharp";
        const string ExtensionsResourcePrefix = "Extensions.Extensions";
        const string BonsaiPackageName = "Bonsai";
        const string SvgExtension = ".svg";
        const char AssemblySeparator = ':';
        readonly string defaultName;
        readonly IncludeWorkflowBuilder includeElement;
        readonly Type resourceQualifier;

        private ElementIcon(string name, IncludeWorkflowBuilder include)
        {
            includeElement = include;
            defaultName = name;
        }

        private ElementIcon(ElementCategory category)
        {
            resourceQualifier = GetType();
            defaultName = string.Join(
                ExpressionHelper.MemberSeparator,
                typeof(ElementCategory).FullName,
                Enum.GetName(typeof(ElementCategory), category));
        }

        public ElementIcon(object workflowElement)
            : this(workflowElement.GetType())
        {
            includeElement = workflowElement as IncludeWorkflowBuilder;
        }

        private ElementIcon(Type workflowElementType)
        {
            var iconAttribute = workflowElementType.GetCustomAttribute<WorkflowElementIconAttribute>() ?? WorkflowElementIconAttribute.Default;
            resourceQualifier = Type.GetType(iconAttribute.TypeName ?? string.Empty, false) ?? workflowElementType;
            if (!string.IsNullOrEmpty(iconAttribute.Name)) defaultName = iconAttribute.Name;
            else defaultName = resourceQualifier.Name;
            if (resourceQualifier.Namespace != null)
            {
                defaultName = string.Join(ExpressionHelper.MemberSeparator, resourceQualifier.Namespace, defaultName);
            }
        }

        public string Name
        {
            get
            {
                if (resourceQualifier != null && includeElement != null)
                {
                    var elementName = includeElement.Path;
                    if (!string.IsNullOrEmpty(elementName)) return elementName;
                }

                return defaultName;
            }
        }

        public bool IsIncludeElement
        {
            get { return includeElement != null; }
        }

        static Dictionary<string, string> GetDefaultManifestResources()
        {
            var resourceNames = typeof(ElementIcon).Assembly.GetManifestResourceNames();
            var resourceMap = new Dictionary<string, string>(resourceNames.Length);
            foreach (var name in resourceNames)
            {
                if (Path.GetExtension(name) != SvgExtension) continue;
                var key = name.Replace(ManifestResourcePrefix, BonsaiPackageName);
                if (key.EndsWith(EditorScriptingNamespace + SvgExtension))
                {
                    resourceMap.Add(ExtensionsResourcePrefix + SvgExtension, name);
                }
                resourceMap.Add(key, name);
            }
            return resourceMap;
        }

        static ElementIcon GetNamespaceIcon(string name, string assemblyName, IncludeWorkflowBuilder include)
        {
            if (string.IsNullOrEmpty(assemblyName) || assemblyName == name) return new ElementIcon(name, include);
            else if (string.IsNullOrEmpty(name)) return new ElementIcon(assemblyName, include);
            else return new ElementIcon(assemblyName + AssemblySeparator + name, include);
        }

        public ElementIcon GetDefaultIcon()
        {
            var name = Name;
            if (name != defaultName)
            {
                string assemblyName;
                var path = Path.ChangeExtension(name, null);
                path = ResolveEmbeddedPath(path, out assemblyName);
                var separatorIndex = path.LastIndexOf(ExpressionHelper.MemberSeparator);
                if (separatorIndex < 0) return null;
                path = path.Substring(0, separatorIndex);
                return GetNamespaceIcon(path, assemblyName, includeElement);
            }

            if (resourceQualifier != null)
            {
                name = resourceQualifier.Namespace;
                var assemblyName = resourceQualifier.Assembly.GetName().Name;
                foreach (var attribute in resourceQualifier.Assembly.GetCustomAttributes<WorkflowNamespaceIconAttribute>())
                {
                    if (attribute.Namespace == resourceQualifier.Namespace || string.IsNullOrEmpty(attribute.Namespace))
                    {
                        assemblyName = string.Empty;
                        name = attribute.ResourceName;
                        if (!string.IsNullOrEmpty(attribute.Namespace))
                        {
                            break;
                        }
                    }
                }

                return GetNamespaceIcon(name, assemblyName, null);
            }
            else
            {
                var separatorIndex = name.IndexOf(AssemblySeparator);
                if (separatorIndex >= 0)
                {
                    return new ElementIcon(name.Substring(0, separatorIndex), includeElement);
                }

                return null;
            }
        }

        static string RemoveInvalidPathChars(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            return string.Concat(path.Split(InvalidPathChars));
        }

        static string ResolvePath(string path, string assemblyName)
        {
            if (!string.IsNullOrEmpty(assemblyName) && assemblyName != BonsaiPackageName)
            {
                var prefix = assemblyName + ExpressionHelper.MemberSeparator;
                var index = path.IndexOf(prefix, StringComparison.Ordinal);
                if (index == 0) path = path.Substring(prefix.Length);
            }

            var workflowPath = Path.Combine(Environment.CurrentDirectory, path);
            if (File.Exists(workflowPath)) return workflowPath;

            var appDomainBaseDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\');
            workflowPath = Path.Combine(appDomainBaseDirectory, "Extensions", path);
            if (File.Exists(workflowPath)) return workflowPath;
            return string.Empty;
        }

        string ResolveEmbeddedPath(string name, out string assemblyName)
        {
            assemblyName = string.Empty;
            var separatorIndex = name.IndexOf(AssemblySeparator);
            if (separatorIndex >= 0 && !Path.IsPathRooted(name))
            {
                var nameElements = name.Split(new[] { AssemblySeparator }, 2);
                if (!string.IsNullOrEmpty(nameElements[0]))
                {
                    if (name != defaultName)
                    {
                        // Infer namespace from assembly name for included paths
                        name = string.Join(ExpressionHelper.MemberSeparator, nameElements);
                    }
                    else name = nameElements[1];
                    assemblyName = nameElements[0];
                }
                else name = nameElements[1];
            }

            return name;
        }

        public Stream GetStream()
        {
            var name = RemoveInvalidPathChars(Name);
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var includedType = false;
            if (name != defaultName)
            {
                includedType = IsIncludeElement;
                name = Path.ChangeExtension(name, SvgExtension);
            }
            else if (Path.GetExtension(name) != SvgExtension)
            {
                name += SvgExtension;
            }

            string assemblyName;
            name = ResolveEmbeddedPath(name, out assemblyName);

            var iconPath = ResolvePath(name, includedType ? string.Empty : assemblyName);
            if (!string.IsNullOrEmpty(iconPath))
            {
                return new FileStream(iconPath, FileMode.Open, FileAccess.Read);
            }

            System.Reflection.Assembly assembly;
            if (resourceQualifier == null || !string.IsNullOrEmpty(assemblyName))
            {
                if (string.IsNullOrEmpty(assemblyName))
                {
                    assemblyName = Path.GetFileNameWithoutExtension(name);
                    name = assemblyName + ExpressionHelper.MemberSeparator + assemblyName + SvgExtension;
                }

                try { assembly = System.Reflection.Assembly.Load(assemblyName); }
                catch (SystemException) { return null; }
            }
            else assembly = resourceQualifier.Assembly;

            if (!assembly.IsDynamic)
            {
                var resourceStream = assembly.GetManifestResourceStream(name);
                if (resourceStream == null && DefaultManifestResources.TryGetValue(name, out name))
                {
                    resourceStream = typeof(ElementIcon).Assembly.GetManifestResourceStream(name);
                }
                return resourceStream;
            }
            else return null;
        }

        public static ElementIcon FromElementCategory(ElementCategory category)
        {
            switch (category)
            {
                case ElementCategory.Source: return Source;
                case ElementCategory.Condition: return Condition;
                case ElementCategory.Transform: return Transform;
                case ElementCategory.Sink: return Sink;
                case ElementCategory.Nested: return Nested;
                case ElementCategory.Property: return Property;
                case ElementCategory.Combinator: return Combinator;
                case ElementCategory.Workflow: return Workflow;
                default: throw new ArgumentException("Invalid category.");
            }
        }
    }
}
