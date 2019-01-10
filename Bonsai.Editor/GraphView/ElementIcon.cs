using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design
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

        static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
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
                resourceQualifier.Namespace,
                typeof(ElementCategory).Name,
                Enum.GetName(typeof(ElementCategory), category));
        }

        public ElementIcon(ExpressionBuilder builder)
            : this(builder, false)
        {
        }

        private ElementIcon(ExpressionBuilder builder, bool forceDefault)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
            var workflowElementType = workflowElement.GetType();
            var attributes = TypeDescriptor.GetAttributes(workflowElement);
            var iconAttribute = (WorkflowIconAttribute)attributes[typeof(WorkflowIconAttribute)];
            resourceQualifier = Type.GetType(iconAttribute.TypeName ?? string.Empty, false) ?? workflowElementType;
            if (!string.IsNullOrEmpty(iconAttribute.Name)) defaultName = iconAttribute.Name;
            else defaultName = workflowElementType.Name;

            includeElement = forceDefault ? null : workflowElement as IncludeWorkflowBuilder;
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

        public ElementIcon GetDefaultIcon()
        {
            var name = Name;
            if (name != defaultName)
            {
                string assemblyName;
                var path = Path.ChangeExtension(name, null);
                path = ResolveEmbeddedPath(path, out assemblyName);
                var separatorIndex = path.LastIndexOf(ExpressionHelper.MemberSeparator);
                if (separatorIndex < 0) return new ElementIcon(includeElement, forceDefault: true);

                path = path.Substring(0, separatorIndex);
                name = !string.IsNullOrEmpty(assemblyName) ? assemblyName + AssemblySeparator + path : path;
                return new ElementIcon(name, includeElement);
            }

            if (resourceQualifier != null)
            {
                string assemblyName;
                var iconAttribute = (WorkflowIconAttribute)(
                    Attribute.GetCustomAttribute(resourceQualifier.Assembly, typeof(WorkflowIconAttribute))
                    ?? WorkflowIconAttribute.Default);
                if (!string.IsNullOrEmpty(iconAttribute.Name)) assemblyName = iconAttribute.Name;
                else assemblyName = resourceQualifier.Assembly.GetName().Name;
                return new ElementIcon(assemblyName + AssemblySeparator + resourceQualifier.Namespace, null);
            }
            else
            {
                var separatorIndex = name.IndexOf(AssemblySeparator);
                if (separatorIndex >= 0)
                {
                    return new ElementIcon(name.Substring(0, separatorIndex), includeElement);
                }
                else if (includeElement != null)
                {
                    return new ElementIcon(includeElement, forceDefault: true);
                }

                return null;
            }
        }

        static string RemoveInvalidPathChars(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            return string.Concat(path.Split(InvalidPathChars));
        }

        static string ResolvePath(string path)
        {
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

            if (name != defaultName)
            {
                name = Path.ChangeExtension(name, SvgExtension);
            }
            else if (Path.GetExtension(name) != SvgExtension)
            {
                name += SvgExtension;
            }

            string assemblyName;
            name = ResolveEmbeddedPath(name, out assemblyName);

            var iconPath = ResolvePath(name);
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
                return assembly.GetManifestResourceStream(name);
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
