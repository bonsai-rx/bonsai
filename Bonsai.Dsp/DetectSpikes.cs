using Bonsai.Dsp;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    public class DetectSpikes : Combinator<Mat, SpikeWaveformCollection>
    {
        readonly Delay delay = new Delay();

        public int Delay
        {
            get { return delay.Count; }
            set { delay.Count = value; }
        }

        public int Length { get; set; }

        public int Threshold { get; set; }

        public override IObservable<SpikeWaveformCollection> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
            {
                byte[] triggerBuffer = null;
                bool[] activeChannels = null;
                SampleBuffer[] activeSpikes = null;
                return source.Publish(ps => ps.Zip(delay.Process(ps), (input, delayed) =>
                {
                    var spikes = new SpikeWaveformCollection(input.Size);
                    if (activeSpikes == null)
                    {
                        triggerBuffer = new byte[input.Cols];
                        activeChannels = new bool[input.Rows];
                        activeSpikes = new SampleBuffer[input.Rows];
                    }

                    for (int i = 0; i < activeSpikes.Length; i++)
                    {
                        using (var channel = input.GetRow(i))
                        using (var delayedChannel = delayed.GetRow(i))
                        {
                            if (activeSpikes[i] != null)
                            {
                                var buffer = activeSpikes[i];
                                buffer.Update(delayedChannel, 0);
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

                            var threshold = Threshold;
                            using (var triggerHeader = Mat.CreateMatHeader(triggerBuffer))
                            {
                                CV.Threshold(channel, triggerHeader, threshold, 1, threshold < 0 ? ThresholdTypes.BinaryInv : ThresholdTypes.Binary);
                            }

                            for (int j = 0; j < triggerBuffer.Length; j++)
                            {
                                var triggerHigh = triggerBuffer[j] > 0;
                                if (triggerHigh && !activeChannels[i])
                                {
                                    var buffer = new SampleBuffer(channel, Length, j);
                                    buffer.Update(delayedChannel, j);
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
                            }
                        }
                    }

                    return spikes;
                }));
            });
        }
    }
}
