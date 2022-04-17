using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents the format of a user-defined input value to a vertex shader.
    /// </summary>
    public class VertexAttributeMapping
    {
        /// <summary>
        /// Gets or sets a value specifying the number of components in the vertex
        /// attribute.
        /// </summary>
        [Description("Specifies the number of components in the vertex attribute.")]
        public int Size { get; set; } = 4;

        /// <summary>
        /// Gets or sets a value specifying whether fixed-point data values should
        /// be normalized or converted directly before they are accessed.
        /// </summary>
        [Description("Specifies whether fixed-point data values should be normalized or converted directly before they are accessed.")]
        public bool Normalized { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the data type of each component in
        /// the vertex attribute.
        /// </summary>
        [Description("Specifies the data type of each component in the vertex attribute.")]
        public VertexAttribPointerType Type { get; set; } = VertexAttribPointerType.Float;

        /// <inheritdoc/>
        public override string ToString()
        {
            var size = Size;
            return string.Format("VertexAttribute({0}{1})", Type, size > 1 ? (object)size : null);
        }
    }
}
