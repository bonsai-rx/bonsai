using Bonsai.Resources.Design;
using System;

namespace Bonsai.Shaders.Configuration.Design
{
    public class StateConfigurationCollectionEditor : CollectionEditor
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
                typeof(ClearColorState),
                typeof(LineWidthState),
                typeof(PointSizeState),
                typeof(DepthMaskState),
                typeof(PolygonModeState),
                typeof(BlendFunctionState),
                typeof(DepthFunctionState),
                typeof(MemoryBarrierState),
                typeof(HintState)
            };
        }
    }
}
