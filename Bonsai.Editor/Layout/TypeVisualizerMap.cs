using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bonsai.Design
{
    class TypeVisualizerMap
    {
        readonly Dictionary<string, Type> visualizerTypeMap = new Dictionary<string, Type>();
        readonly Dictionary<Type, List<Type>> visualizerMap = new Dictionary<Type, List<Type>>();

        public void Add(Type targetType, Type visualizerType)
        {
            visualizerTypeMap[visualizerType.FullName] = visualizerType;
            if (!visualizerMap.TryGetValue(targetType, out List<Type> visualizerTypes))
            {
                visualizerTypes = new List<Type>();
                visualizerMap.Add(targetType, visualizerTypes);
            }

            visualizerTypes.Add(visualizerType);
        }

        public Type GetVisualizerType(string visualizerTypeName)
        {
            visualizerTypeMap.TryGetValue(visualizerTypeName, out Type visualizerType);
            return visualizerType;
        }

        public IEnumerable<Type> GetTypeVisualizers(Type targetType)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }

            if (visualizerMap.TryGetValue(targetType, out List<Type> visualizerTypes))
            {
                return visualizerTypes;
            }

            return Enumerable.Empty<Type>();
        }

        public IEnumerable<Type> GetTypeVisualizers(InspectBuilder inspectBuilder)
        {
            var workflowElementType = ExpressionBuilder.GetWorkflowElement(inspectBuilder).GetType();
            foreach (var type in GetTypeVisualizers(workflowElementType))
            {
                yield return type;
            }

            var observableType = inspectBuilder.ObservableType;
            while (observableType != null)
            {
                foreach (var type in GetTypeVisualizers(observableType))
                {
                    yield return type;
                }

                if (!observableType.IsClass)
                {
                    foreach (var type in GetTypeVisualizers(typeof(object)))
                    {
                        yield return type;
                    }
                    break;
                }
                else observableType = observableType.BaseType;
            }
        }
    }
}
