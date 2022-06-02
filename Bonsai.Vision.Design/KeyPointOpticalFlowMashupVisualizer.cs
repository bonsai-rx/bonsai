﻿using System;
using Bonsai;
using Bonsai.Vision.Design;
using Bonsai.Design;

[assembly: TypeVisualizer(typeof(KeyPointOpticalFlowMashupVisualizer), Target = typeof(MashupSource<ImageMashupVisualizer, KeyPointOpticalFlowVisualizer>))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that overlays the sparse optical flow between key points
    /// over an existing image visualizer.
    /// </summary>
    public class KeyPointOpticalFlowMashupVisualizer : DialogTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var tracking = (KeyPointOpticalFlow)value;
            KeyPointOpticalFlowVisualizer.Draw(visualizer.VisualizerImage, tracking);
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            visualizer = (ImageMashupVisualizer)provider.GetService(typeof(MashupVisualizer));
        }

        /// <inheritdoc/>
        public override void Unload()
        {
        }
    }
}
