using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that detects spike events in the input signal and
    /// extracts their waveforms.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Detects spike events in the input signal and extracts their waveforms.")]
    public class DetectSpikes : Combinator<Mat, SpikeWaveformCollection>
    {
        static readonly double[] DefaultThreshold = new[] { 0.0 };
        readonly Delay delay = new Delay();

        /// <summary>
        /// Gets or sets the delay of each spike waveform from its trigger, in samples.
        /// </summary>
        [Description("The delay of each spike waveform from its trigger, in samples.")]
        public int Delay
        {
            get { return delay.Count; }
            set { delay.Count = value; }
        }

        /// <summary>
        /// Gets or sets the length of each spike waveform, in samples.
        /// </summary>
        [Description("The length of each spike waveform, in samples.")]
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the per-channel threshold for detecting individual spikes.
        /// </summary>
        [TypeConverter(typeof(UnidimensionalArrayConverter))]
        [Editor("Bonsai.Dsp.Design.SpikeThresholdEditor, Bonsai.Dsp.Design", DesignTypes.UITypeEditor)]
        [Description("The per-channel threshold for detecting individual spikes.")]
        public double[] Threshold { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the waveform refinement method.
        /// </summary>
        [Description("Specifies the waveform refinement method.")]
        public SpikeWaveformRefinement WaveformRefinement { get; set; } = SpikeWaveformRefinement.AlignPeaks;

        /// <summary>
        /// Detects spike events in the input signal and extracts their waveforms.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Mat"/> objects representing the waveform of the
        /// signal from which to extract spike waveforms.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="SpikeWaveformCollection"/> representing the spikes
        /// detected in each buffer of the signal waveform.
        /// </returns>
        public override IObservable<SpikeWaveformCollection> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
            {
                byte[] triggerBuffer = null;
                bool[] activeChannels = null;
                int[] refractoryChannels = null;
                SampleBuffer[] activeSpikes = null;
                var ioff = 0L;
                return source.Publish(ps => ps.Zip(delay.Process(ps), (input, delayed) =>
                {
                    var spikes = new SpikeWaveformCollection(input.Size);
                    if (activeSpikes == null)
                    {
                        triggerBuffer = new byte[input.Cols];
                        activeChannels = new bool[input.Rows];
                        refractoryChannels = new int[input.Rows];
                        activeSpikes = new SampleBuffer[input.Rows];
                    }

                    var thresholdValues = Threshold ?? DefaultThreshold;
                    if (thresholdValues.Length == 0) thresholdValues = DefaultThreshold;
                    for (int i = 0; i < activeSpikes.Length; i++)
                    {
                        using (var channel = input.GetRow(i))
                        using (var delayedChannel = delayed.GetRow(i))
                        {
                            var threshold = thresholdValues.Length > 1 ? thresholdValues[i] : thresholdValues[0];
                            if (activeSpikes[i] != null)
                            {
                                var buffer = activeSpikes[i];
                                buffer = UpdateBuffer(buffer, delayedChannel, 0, delay.Count, threshold);
                                activeSpikes[i] = buffer;
                                if (buffer.Completed)
                                {
                                    spikes.Add(new SpikeWaveform
                                    {
                                        ChannelIndex = i,
                                        SampleIndex = buffer.SampleIndex,
                                        Waveform = buffer.Samples
                                    });
                                    activeSpikes[i] = null;
                                }
                                else continue;
                            }

                            using (var triggerHeader = Mat.CreateMatHeader(triggerBuffer))
                            {
                                CV.Threshold(
                                    channel,
                                    triggerHeader,
                                    threshold, 1,
                                    threshold < 0 ? ThresholdTypes.BinaryInv : ThresholdTypes.Binary);
                            }

                            for (int j = 0; j < triggerBuffer.Length; j++)
                            {
                                var triggerHigh = triggerBuffer[j] > 0;
                                if (triggerHigh && !activeChannels[i] && refractoryChannels[i] == 0 && activeSpikes[i] == null)
                                {
                                    var length = Length;
                                    refractoryChannels[i] = length;
                                    var buffer = new SampleBuffer(channel, length, j + ioff);
                                    buffer.Refined |= WaveformRefinement == SpikeWaveformRefinement.None;
                                    buffer = UpdateBuffer(buffer, delayedChannel, j, delay.Count, threshold);
                                    if (buffer.Completed)
                                    {
                                        spikes.Add(new SpikeWaveform
                                        {
                                            ChannelIndex = i,
                                            SampleIndex = buffer.SampleIndex,
                                            Waveform = buffer.Samples
                                        });
                                    }
                                    else activeSpikes[i] = buffer;
                                }

                                activeChannels[i] = triggerHigh;
                                if (refractoryChannels[i] > 0)
                                {
                                    refractoryChannels[i]--;
                                }
                            }
                        }
                    }

                    ioff += input.Cols;
                    return spikes;
                }));
            });
        }

        static SampleBuffer UpdateBuffer(SampleBuffer buffer, Mat source, int index, int delay, double threshold)
        {
            var samplesTaken = buffer.Update(source, index);
            if (buffer.Completed && !buffer.Refined)
            {
                var waveform = buffer.Samples;
                CV.MinMaxLoc(waveform, out _, out _, out Point minLoc, out Point maxLoc);

                var offset = threshold > 0 ? maxLoc.X - delay : minLoc.X - delay;
                if (offset > 0)
                {
                    var offsetBuffer = new SampleBuffer(waveform, waveform.Cols, buffer.SampleIndex + offset);
                    offsetBuffer.Refined = true;
                    offsetBuffer.Update(waveform, offset);
                    offsetBuffer.Update(source, index + samplesTaken + offset);
                    return offsetBuffer;
                }
            }

            return buffer;
        }
    }
}
