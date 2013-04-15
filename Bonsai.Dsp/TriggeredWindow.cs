using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace Bonsai.Dsp
{
    public class TriggeredWindow : CombinatorBuilder<Tuple<CvMat, CvMat>, IObservable<CvMat>>
    {
        public int Count { get; set; }

        class DataWindow
        {
            public CvMat Window;
            public int WindowIndex;

            public static DataWindow Create(CvMat window, int windowIndex)
            {
                return new DataWindow { Window = window, WindowIndex = windowIndex };
            }
        }

        static int UpdateWindow(CvMat data, CvMat dataWindow, int dataIndex, int windowIndex)
        {
            var windowElements = Math.Min(data.Cols - dataIndex, dataWindow.Cols - windowIndex);
            if (windowElements > 0)
            {
                using (var dataSubRect = data.GetSubRect(new CvRect(dataIndex, 0, windowElements, data.Rows)))
                using (var windowSubRect = dataWindow.GetSubRect(new CvRect(windowIndex, 0, windowElements, dataWindow.Rows)))
                {
                    Core.cvCopy(dataSubRect, windowSubRect);
                }
            }

            return windowIndex + windowElements;
        }

        protected override IObservable<IObservable<CvMat>> Combine(IObservable<Tuple<CvMat, CvMat>> source)
        {
            return Observable.Create<IObservable<CvMat>>(observer =>
            {
                bool active = false;
                var dataWindows = new List<DataWindow>();
                return source.Subscribe(
                    xs =>
                    {
                        var data = xs.Item1;
                        var trigger = xs.Item2;

                        // Update pending windows
                        dataWindows.RemoveAll(window =>
                        {
                            window.WindowIndex = UpdateWindow(data, window.Window, 0, window.WindowIndex);
                            if (window.WindowIndex < window.Window.Cols) return false;

                            // Window is ready, emit
                            observer.OnNext(Observable.Return(window.Window));
                            return true;
                        });

                        // Check if new triggers have arrived
                        var nonZero = Core.cvCountNonZero(trigger);
                        if (nonZero <= 0) active = false;
                        else
                        {
                            var triggerBuffer = new byte[trigger.Cols];
                            var triggerHandle = GCHandle.Alloc(triggerBuffer, GCHandleType.Pinned);
                            using (var triggerHeader = new CvMat(1, triggerBuffer.Length, CvMatDepth.CV_8U, 1, triggerHandle.AddrOfPinnedObject()))
                            {
                                Core.cvConvert(trigger, triggerHeader);
                                triggerHandle.Free();
                            }

                            for (int i = 0; i < triggerBuffer.Length; i++)
                            {
                                var triggerHigh = triggerBuffer[i] > 0;
                                if (triggerHigh && !active)
                                {
                                    var dataWindow = new CvMat(data.Rows, Count, data.Depth, data.NumChannels);
                                    var windowIndex = UpdateWindow(data, dataWindow, i, 0);
                                    if (windowIndex < dataWindow.Cols)
                                    {
                                        // Window is missing data, add to list
                                        dataWindows.Add(DataWindow.Create(dataWindow, windowIndex));
                                    }
                                    // Window is ready, emit
                                    else observer.OnNext(Observable.Return(dataWindow));
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
