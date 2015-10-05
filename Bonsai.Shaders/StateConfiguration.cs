using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    [XmlInclude(typeof(EnableState))]
    [XmlInclude(typeof(DisableState))]
    [XmlInclude(typeof(ViewportState))]
    [XmlInclude(typeof(LineWidthState))]
    [XmlInclude(typeof(PointSizeState))]
    [XmlInclude(typeof(BlendFunctionState))]
    [XmlInclude(typeof(DepthFunctionState))]
    public abstract class StateConfiguration
    {
        public abstract void Execute(Shader shader);
    }
}
