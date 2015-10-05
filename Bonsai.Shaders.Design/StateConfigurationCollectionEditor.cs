using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Design
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
                typeof(LineWidthState),
                typeof(PointSizeState),
                typeof(BlendFunctionState),
                typeof(DepthFunctionState)
            };
        }
    }
}
