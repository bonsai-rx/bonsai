using System;
using System.Collections.Generic;
using System.Linq;
using Bonsai.Dag;
using Bonsai.Expressions;

namespace Bonsai.Design
{
    class MashupArgumentMap
    {
        readonly Dictionary<InspectBuilder, SortedList<int, VisualizerFactory>> mashupArguments;

        public MashupArgumentMap()
        {
            mashupArguments = new Dictionary<InspectBuilder, SortedList<int, VisualizerFactory>>();
        }

        public VisualizerDialogLauncher Add(Node<ExpressionBuilder, ExpressionBuilderArgument> node, VisualizerDialogLauncher launcher)
        {
            if (ExpressionBuilder.GetWorkflowElement(node.Value) is VisualizerMappingBuilder &&
                node.Successors.Count > 0)
            {
                var mappingEdge = node.Successors[0];
                var visualizerTarget = ExpressionBuilder.GetVisualizerElement(mappingEdge.Target.Value);
                if (!mashupArguments.TryGetValue(visualizerTarget, out SortedList<int, VisualizerFactory> mashupSources))
                {
                    mashupSources = new SortedList<int, VisualizerFactory>();
                    mashupArguments.Add(visualizerTarget, mashupSources);
                }

                mashupSources.Add(mappingEdge.Label.Index, launcher.VisualizerFactory);
            }

            return launcher;
        }

        public IReadOnlyList<VisualizerFactory> GetMashupArgumentList(InspectBuilder key)
        {
            var visualizerElement = ExpressionBuilder.GetVisualizerElement(key);
            if (mashupArguments.TryGetValue(visualizerElement, out SortedList<int, VisualizerFactory> arguments))
            {
                return arguments.Values.ToList();
            }

            return Array.Empty<VisualizerFactory>();
        }
    }
}
