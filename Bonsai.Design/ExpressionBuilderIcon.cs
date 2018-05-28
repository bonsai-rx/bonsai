using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design
{
    class ExpressionBuilderIcon : WorkflowIcon
    {
        const string SvgExtension = ".svg";
        const string GroupPrefix = "gp://";
        readonly string defaultName;
        readonly INamedElement namedElement;
        readonly Type resourceQualifier;
        string nameCache;

        public ExpressionBuilderIcon(ElementCategory category)
        {
            resourceQualifier = GetType();
            defaultName = string.Join(
                ExpressionHelper.MemberSeparator,
                resourceQualifier.Namespace,
                typeof(ElementCategory).Name,
                Enum.GetName(typeof(ElementCategory), category));
        }

        public ExpressionBuilderIcon(ExpressionBuilder builder)
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
            else
            {
                namedElement = workflowElement as INamedElement;
                defaultName = ExpressionBuilder.GetElementDisplayName(workflowElementType);
            }

            if (resourceQualifier.Namespace != null)
            {
                defaultName = string.Join(ExpressionHelper.MemberSeparator, resourceQualifier.Namespace, defaultName);
            }

            if (namedElement != null &&
               (workflowElement is IncludeWorkflowBuilder ||
                workflowElement is GroupWorkflowBuilder))
            {
                nameCache = defaultName;
                defaultName = GroupPrefix + defaultName;
            }
        }

        public override string Name
        {
            get
            {
                if (namedElement != null)
                {
                    var elementName = namedElement.Name;
                    var prefixOffset = defaultName.IndexOf(GroupPrefix, StringComparison.Ordinal);
                    if (prefixOffset == 0)
                    {
                        if (!string.IsNullOrEmpty(elementName)) return elementName;
                        else return nameCache;
                    }

                    if (!string.IsNullOrEmpty(elementName))
                    {
                        if (nameCache == null ||
                            string.CompareOrdinal(
                                nameCache, defaultName.Length + 1,
                                elementName, 0,
                                Math.Max(elementName.Length, nameCache.Length - defaultName.Length - 1)) != 0)
                        {
                            nameCache = string.Join(ExpressionHelper.MemberSeparator, defaultName, elementName);
                        }

                        return nameCache;
                    }
                    else nameCache = null;
                }

                return defaultName;
            }
        }

        static string ResolvePath(string path)
        {
            const string PathEnvironmentVariable = "PATH";
            var workflowPath = Path.Combine(Environment.CurrentDirectory, path);
            if (File.Exists(workflowPath)) return workflowPath;

            var pathLocations = Environment.GetEnvironmentVariable(PathEnvironmentVariable).Split(Path.PathSeparator);
            for (int i = 0; i < pathLocations.Length; i++)
            {
                workflowPath = Path.Combine(pathLocations[i], path);
                if (File.Exists(workflowPath)) return workflowPath;
            }

            return string.Empty;
        }

        public override Stream GetStream()
        {
            var name = Name;
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (Path.GetExtension(name) != SvgExtension)
            {
                name += SvgExtension;
            }

            var iconPath = ResolvePath(name);
            if (!string.IsNullOrEmpty(iconPath))
            {
                return new FileStream(iconPath, FileMode.Open, FileAccess.Read);
            }
            else if (!resourceQualifier.Assembly.IsDynamic)
            {
                return resourceQualifier.Assembly.GetManifestResourceStream(name);
            }
            else return null;
        }
    }
}
