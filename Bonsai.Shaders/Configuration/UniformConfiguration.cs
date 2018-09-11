using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlInclude(typeof(FloatUniform))]
    [XmlInclude(typeof(Vec2Uniform))]
    [XmlInclude(typeof(Vec3Uniform))]
    [XmlInclude(typeof(Vec4Uniform))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class UniformConfiguration
    {
        [Description("The name of the uniform variable.")]
        public string Name { get; set; }

        internal abstract void SetUniform(int location);
    }
}
