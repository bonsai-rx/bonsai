using Bonsai.Resources.Design;
using System;

namespace Bonsai.Shaders.Configuration.Design
{
    /// <summary>
    /// Provides a user interface editor that displays a dialog for editing a
    /// collection of framebuffer attachment configuration objects.
    /// </summary>
    public class FramebufferAttachmentConfigurationCollectionEditor : CollectionEditor
    {
        const string BaseText = "Attach";

        /// <summary>
        /// Initializes a new instance of the <see cref="FramebufferAttachmentConfigurationCollectionEditor"/>
        /// class using the specified collection type.
        /// </summary>
        /// <param name="type">
        /// The type of the collection for this editor to edit.
        /// </param>
        public FramebufferAttachmentConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        /// <inheritdoc/>
        protected override string GetDisplayText(object value)
        {
            var configuration = (FramebufferAttachmentConfiguration)value;
            var attachment = configuration.Attachment;
            var textureName = configuration.TextureName;
            if (string.IsNullOrEmpty(textureName))
            {
                return $"{BaseText}({attachment})";
            }
            else return $"{BaseText}({attachment} : {textureName})";
        }
    }
}
