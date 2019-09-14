using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Resources
{
    public interface IResourceConfiguration
    {
        string Name { get; }

        Type Type { get; }

        IDisposable CreateResource(ResourceManager resourceManager);
    }

    public abstract class ResourceConfiguration<TResource> : IResourceConfiguration where TResource : IDisposable
    {
        [Description("The name of the resource.")]
        public string Name { get; set; }

        Type IResourceConfiguration.Type
        {
            get { return typeof(TResource); }
        }

        IDisposable IResourceConfiguration.CreateResource(ResourceManager resourceManager)
        {
            return CreateResource(resourceManager);
        }

        public abstract TResource CreateResource(ResourceManager resourceManager);

        public override string ToString()
        {
            var name = Name;
            var typeName = GetType().Name;
            if (string.IsNullOrEmpty(name)) return typeName;
            else return string.Format("{0} [{1}]", name, typeName);
        }

        protected Stream OpenResource(string path)
        {
            const char AssemblySeparator = ':';
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Missing resource path while loading " + ToString() + ".", "path");
            }

            var separatorIndex = path.IndexOf(AssemblySeparator);
            if (separatorIndex >= 0 && !Path.IsPathRooted(path))
            {
                var nameElements = path.Split(new[] { AssemblySeparator }, 2);
                if (string.IsNullOrEmpty(nameElements[0]))
                {
                    throw new ArgumentException(
                        "The embedded resource path \"" + path +
                        "\" in " + ToString() + " must be qualified with a valid assembly name.",
                        "path");
                }

                var assembly = System.Reflection.Assembly.Load(nameElements[0]);
                var resourceName = string.Join(ExpressionHelper.MemberSeparator, nameElements);
                var resourceStream = assembly.GetManifestResourceStream(resourceName);
                if (resourceStream == null)
                {
                    throw new ArgumentException(
                        "The specified embedded resource \"" + nameElements[1] +
                        "\" was not found in assembly \"" + nameElements[0] +
                        "\" while loading " + ToString() + ".",
                        "path");
                }

                return resourceStream;
            }
            else
            {
                if (!File.Exists(path))
                {
                    throw new ArgumentNullException("path", "The specified path \"" + path + "\" was not found while loading " + ToString() + ".");
                }

                return File.OpenRead(path);
            }
        }
    }
}
