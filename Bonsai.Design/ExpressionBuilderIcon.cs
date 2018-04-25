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
        readonly string defaultName;
        readonly INamedElement namedElement;
        readonly Type iconQualifier;

        public ExpressionBuilderIcon(object workflowElement)
        {
            if (workflowElement == null)
            {
                throw new ArgumentNullException("workflowElement");
            }

            var componentType = workflowElement.GetType();
            var attributes = TypeDescriptor.GetAttributes(workflowElement);
            var iconAttribute = (WorkflowIconAttribute)attributes[typeof(WorkflowIconAttribute)];
            iconQualifier = Type.GetType(iconAttribute.TypeName ?? string.Empty, false) ?? componentType;
            if (!string.IsNullOrEmpty(iconAttribute.Name)) defaultName = iconAttribute.Name;
            else
            {
                namedElement = workflowElement as INamedElement;
                defaultName = ExpressionBuilder.GetElementDisplayName(componentType);
            }
        }

        public override string Name
        {
            get
            {
                var name = defaultName;
                if (namedElement != null)
                {
                    var elementName = namedElement.Name;
                    if (!string.IsNullOrEmpty(elementName)) name = elementName;
                }

                return string.Join(".", iconQualifier.Namespace, name);
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
            else return iconQualifier.Assembly.GetManifestResourceStream(name);
        }
    }
}
