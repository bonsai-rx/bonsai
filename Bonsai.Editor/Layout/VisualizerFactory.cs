using System;
using System.Collections.Generic;
using Bonsai.Expressions;

namespace Bonsai.Design
{
    class VisualizerFactory
    {
        public VisualizerFactory(InspectBuilder source, Type visualizerType, IReadOnlyList<VisualizerFactory> mashupArguments = null)
        {
            Source = source;
            VisualizerType = visualizerType;
            MashupSources = mashupArguments ?? Array.Empty<VisualizerFactory>();
        }

        public InspectBuilder Source { get; }

        public Type VisualizerType { get; }

        public IReadOnlyList<VisualizerFactory> MashupSources { get; }
    }
}
