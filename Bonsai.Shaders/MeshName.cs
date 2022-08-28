using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents the properties of a mesh geometry which is part of an aggregate
    /// rendering operation.
    /// </summary>
    public class MeshName
    {
        /// <summary>
        /// Gets or sets the name of the mesh geometry to aggregate.
        /// </summary>
        [XmlText]
        [TypeConverter(typeof(MeshNameConverter))]
        [Description("The name of the mesh geometry to aggregate.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the number of instances that each attribute
        /// in the mesh geometry buffer represents during instanced rendering.
        /// </summary>
        /// <remarks>
        /// If divisor is zero, the attribute advances once per vertex. If divisor
        /// is non-zero, the attribute advances once per divisor instances of the
        /// sets of vertices being rendered.
        /// </remarks>
        [XmlAttribute]
        [DefaultValue(0)]
        [Description("Specifies the number of instances that each attribute in the mesh geometry represents during instanced rendering.")]
        public int Divisor { get; set; }
    }
}
