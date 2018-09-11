using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class Vec3Uniform : UniformConfiguration
    {
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The value used to initialize the uniform variable.")]
        public Vector3 Value { get; set; }

        internal override void SetUniform(int location)
        {
            GL.Uniform3(location, Value);
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", string.IsNullOrEmpty(Name) ? "Vec3" : Name, Value);
        }
    }
}
