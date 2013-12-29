using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor
{
    class TypeVisualizerMap
    {
        Dictionary<Type, List<Type>> visualizerMap = new Dictionary<Type, List<Type>>();

        public void Add(Type targetType, Type visualizerType)
        {
            List<Type> visualizerTypes;
            if (!visualizerMap.TryGetValue(targetType, out visualizerTypes))
            {
                visualizerTypes = new List<Type>();
                visualizerMap.Add(targetType, visualizerTypes);
            }

            visualizerTypes.Add(visualizerType);
        }

        public IEnumerable<Type> GetTypeVisualizers(Type targetType)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            List<Type> visualizerTypes;
            if (visualizerMap.TryGetValue(targetType, out visualizerTypes))
            {
                return visualizerTypes;
            }

            return Enumerable.Empty<Type>();
        }
    }
}
