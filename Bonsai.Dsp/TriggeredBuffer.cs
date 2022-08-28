using System;
using System.Collections.Generic;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that extracts a number of samples from the input signal
    /// whenever a trigger rises.
    /// </summary>
    [Description("Extracts a number of samples from the input signal whenever a trigger rises.")]
    public class TriggeredBuffer : Combinator<Tuple<Mat, Mat>, Mat>
    {
        /// <summary>
        /// Gets or sets the number of samples in each triggered buffer.
        /// </summary>
        [Description("The number of samples in each triggered buffer.")]
        public int Count { get; set; }

        /// <summary>
        /// Extracts a number of samples from the input signal whenever a trigger rises.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs of 2D matrices, where the first matrix contains the
        /// signal to extract samples from, and the second matrix contains the
        /// binary trigger signal, where zero values represent the trigger is in a
        /// LOW state, and positive values represent the trigger is in a HIGH state.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects storing the extracted samples
        /// whenever the trigger line changes from LOW to HIGH.
        /// </returns>
        public override IObservable<Mat> Process(IObservable<Tuple<Mat, Mat>> source)
        {
            return Observable.Create<Mat>(observer =>
            {
                bool active = false;
                var activeBuffers = new List<SampleBuffer>();
                return source.Subscribe(input =>
                {
                    try
                    {
                        var data = input.Item1;
                        var trigger = input.Item2;

                        // Update pending windows
                        activeBuffers.RemoveAll(buffer =>
                        {
                            buffer.Update(data, 0);
                            if (buffer.Completed)
                            {
                                // Window is ready, emit
                                observer.OnNext(buffer.Samples);
                                return true;
                            }

                            return false;
                        });

                        // Check if new triggers have arrived
                        var nonZero = CV.CountNonZero(trigger);
                        if (nonZero <= 0) active = false;
                        else
                        {
                            var triggerBuffer = new byte[trigger.Cols];
                            var triggerHandle = GCHandle.Alloc(triggerBuffer, GCHandleType.Pinned);
                            using (var triggerHeader = new Mat(1, triggerBuffer.Length, Depth.U8, 1, triggerHandle.AddrOfPinnedObject()))
                            {
                                CV.Convert(trigger, triggerHeader);
                                triggerHandle.Free();
                            }

                            for (int i = 0; i < triggerBuffer.Length; i++)
                            {
                                var triggerHigh = triggerBuffer[i] > 0;
                                if (triggerHigh && !active)
                                {
                                    var buffer = new SampleBuffer(data, Count, i);
                                    buffer.Update(data, i);
                                    if (buffer.Completed)
                                    {
                                        // Window is ready, emit
                                        observer.OnNext(buffer.Samples);
                                    }
                                    // Window is missing data, add to list
                                    else activeBuffers.Add(buffer);
                                }

                                active = triggerHigh;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                error => observer.OnError(error),
                () => observer.OnCompleted());
            });
        }
    }
}
