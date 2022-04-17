using Bonsai.Resources.Design;
using System;

namespace Bonsai.Shaders.Configuration.Design
{
    /// <summary>
    /// Provides a user interface editor that displays a dialog for editing a
    /// collection of buffer binding configuration objects.
    /// </summary>
    public class BufferBindingConfigurationCollectionEditor : CollectionEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BufferBindingConfigurationCollectionEditor"/>
        /// class using the specified collection type.
        /// </summary>
        /// <param name="type">
        /// The type of the collection for this editor to edit.
        /// </param>
        public BufferBindingConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        /// <inheritdoc/>
        protected override Type[] CreateNewItemTypes()
        {
            return new[]
            {
                typeof(TextureBindingConfiguration),
                typeof(ImageTextureBindingConfiguration),
                typeof(MeshBindingConfiguration)
            };
        }

        /// <inheritdoc/>
        protected override string GetDisplayText(object value)
        {
            return value.ToString();
        }
    }
}
