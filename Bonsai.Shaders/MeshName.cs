using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    public class MeshName
    {
        [XmlText]
        [TypeConverter(typeof(MeshNameConverter))]
        [Description("The name of the mesh geometry to aggregate.")]
        public string Name { get; set; }

        [XmlAttribute]
        [DefaultValue(0)]
        [Description("Optionally specifies the number of instances populated by each buffer item in case of instanced rendering.")]
        public int Divisor { get; set; }
    }
}
