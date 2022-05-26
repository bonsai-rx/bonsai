using System;
using Bonsai.Design;

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a user interface editor that displays a dialog for editing a
    /// collection of image compression parameters.
    /// </summary>
    public class CompressionParameterCollectionEditor : DescriptiveCollectionEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompressionParameterCollectionEditor"/>
        /// class using the specified collection type.
        /// </summary>
        /// <param name="type">
        /// The type of the collection for this editor to edit.
        /// </param>
        public CompressionParameterCollectionEditor(Type type)
            : base(type)
        {
        }

        /// <inheritdoc/>
        protected override Type[] CreateNewItemTypes()
        {
            return new[]
            {
                typeof(JpegQuality),
                typeof(PngCompressionLevel),
                typeof(PngCompressionStrategy),
                typeof(PngBiLevelCompression),
                typeof(PxmBinaryFormat)
            };
        }
    }
}
