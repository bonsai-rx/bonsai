using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    public class StateConfigurationCollectionEditor : DescriptiveCollectionEditor
    {
        public StateConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new[]
            {
                typeof(EnableState),
                typeof(DisableState),
                typeof(ViewportState),
                typeof(ScissorState),
                typeof(LineWidthState),
                typeof(PointSizeState),
                typeof(DepthMaskState),
                typeof(PolygonModeState),
                typeof(BlendFunctionState),
                typeof(DepthFunctionState),
                typeof(MemoryBarrierState)
            };
        }
    }
}
