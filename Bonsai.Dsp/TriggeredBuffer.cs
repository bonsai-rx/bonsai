﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace Bonsai.Dsp
{
    public class TriggeredBuffer : Combinator<Tuple<Mat, Mat>, Mat>
    {
        public int Count { get; set; }

        public override IObservable<Mat> Process(IObservable<Tuple<Mat, Mat>> source)
        {
            return Observable.Create<Mat>(observer =>
            {
                bool active = false;
                var activeBuffers = new List<SampleBuffer>();
                return source.Subscribe(
                    xs =>
                    {
                        var data = xs.Item1;
                        var trigger = xs.Item2;

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
                                    var buffer = new SampleBuffer(data, Count);
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
                    },
                    error => observer.OnError(error),
                    () => observer.OnCompleted());
            });
        }
    }
}
