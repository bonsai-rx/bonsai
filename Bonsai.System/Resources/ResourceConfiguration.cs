using System;
using System.ComponentModel;
using System.IO;

namespace Bonsai.Resources
{
    /// <summary>
    /// Provides a mechanism for loading different types of resources.
    /// </summary>
    public interface IResourceConfiguration
    {
        /// <summary>
        /// Gets the identifier of the resource.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type of the resource.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Creates the contents of the resource using the specified resource manager.
        /// </summary>
        /// <param name="resourceManager">
        /// The <see cref="ResourceManager"/> object onto which the resource will be loaded.
        /// The resource manager can be accessed to load additional resource dependencies which
        /// may be required to create the new resource.
        /// </param>
        /// <returns>
        /// A <see cref="IDisposable"/> object which can be used to access and release the
        /// resource contents.
        /// </returns>
        IDisposable CreateResource(ResourceManager resourceManager);
    }

    /// <summary>
    /// Provides the abstract base class for configuring and loading specific resources.
    /// </summary>
    /// <typeparam name="TResource">The type of the resource.</typeparam>
    public abstract class ResourceConfiguration<TResource> : IResourceConfiguration where TResource : IDisposable
    {
        /// <summary>
        /// Gets or sets the name of the resource.
        /// </summary>
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

        /// <summary>
        /// When overridden in a derived class, creates a new resource of type
        /// <typeparamref name="TResource"/>.
        /// </summary>
        /// <param name="resourceManager">
        /// The <see cref="ResourceManager"/> object onto which this resource will be loaded.
        /// The resource manager can be accessed to load additional resource dependencies which
        /// may be required to create the new resource.
        /// </param>
        /// <returns>A new instance of type <typeparamref name="TResource"/>.</returns>
        public abstract TResource CreateResource(ResourceManager resourceManager);

        /// <inheritdoc/>
        public override string ToString()
        {
            var name = Name;
            var typeName = GetType().Name;
            if (string.IsNullOrEmpty(name)) return typeName;
            else return string.Format("{0} [{1}]", name, typeName);
        }

        /// <summary>
        /// Opens a stream for reading the specified resource.
        /// </summary>
        /// <param name="path">The name of the resource to be opened for reading.</param>
        /// <returns>A <see cref="Stream"/> object for reading the resource.</returns>
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
