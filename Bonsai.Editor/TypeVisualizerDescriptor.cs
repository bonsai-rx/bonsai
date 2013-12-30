using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor
{
    [Serializable]
    public class TypeVisualizerDescriptor
    {
        public string VisualizerTypeName;
        public string TargetTypeName;

        public TypeVisualizerDescriptor(TypeVisualizerAttribute typeVisualizer)
        {
            TargetTypeName = typeVisualizer.TargetTypeName;
            VisualizerTypeName = typeVisualizer.VisualizerTypeName;
        }
    }
}
