﻿using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that computes the per-channel color histograms
    /// for each image in the sequence.
    /// </summary>
    [Description("Computes the per-channel color histograms for each image in the sequence.")]
    public class ImageHistogram : Transform<IplImage, ScalarHistogram>
    {
        static Histogram ComputeChannelHistogram(IplImage channel)
        {
            var range = channel.Depth == IplDepth.U16 ? ushort.MaxValue : byte.MaxValue;
            var histogram = new Histogram(1, new[] { 256 }, HistogramType.Array, new[] { new[] { 0f, range + 1 } });
            histogram.CalcArrHist(new[] { channel });
            histogram.Normalize(1);
            return histogram;
        }

        /// <summary>
        /// Computes the per-channel color histograms for each image in an
        /// observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images for which to compute the per-channel color
        /// histograms.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ScalarHistogram"/> objects representing
        /// the per-channel color histograms for each image.
        /// </returns>
        public override IObservable<ScalarHistogram> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var c0 = input.Channels > 1 ? new IplImage(input.Size, input.Depth, 1) : input;
                var c1 = input.Channels > 1 ? new IplImage(input.Size, input.Depth, 1) : null;
                var c2 = input.Channels > 2 ? new IplImage(input.Size, input.Depth, 1) : null;
                var c3 = input.Channels > 3 ? new IplImage(input.Size, input.Depth, 1) : null;
                if (input.Channels > 1)
                {
                    CV.Split(input, c0, c1, c2, c3);
                }

                var h0 = ComputeChannelHistogram(c0);
                var h1 = c1 != null ? ComputeChannelHistogram(c1) : null;
                var h2 = c2 != null ? ComputeChannelHistogram(c2) : null;
                var h3 = c3 != null ? ComputeChannelHistogram(c3) : null;
                return new ScalarHistogram(h0, h1, h2, h3);
            });
        }
    }
}
