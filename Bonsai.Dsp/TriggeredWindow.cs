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
    public class TriggeredWindow : Combinator<Tuple<Mat, Mat>, Mat>
    {
        public int Count { get; set; }

        class DataWindow
        {
            public Mat Window;
            public int WindowIndex;

            public static DataWindow Create(Mat window, int windowIndex)
            {
                return new DataWindow { Window = window, WindowIndex = windowIndex };
            }
        }

        static int UpdateWindow(Mat data, Mat dataWindow, int dataIndex, int windowIndex)
        {
            var windowElements = Math.Min(data.Cols - dataIndex, dataWindow.Cols - windowIndex);
            if (windowElements > 0)
            {
                using (var dataSubRect = data.GetSubRect(new Rect(dataIndex, 0, windowElements, data.Rows)))
                using (var windowSubRect = dataWindow.GetSubRect(new Rect(windowIndex, 0, windowElements, dataWindow.Rows)))
                {
                    CV.Copy(dataSubRect, windowSubRect);
                }
            }

            return windowIndex + windowElements;
        }

        public override IObservable<Mat> Process(IObservable<Tuple<Mat, Mat>> source)
        {
            return Observable.Create<Mat>(observer =>
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
                            observer.OnNext(window.Window);
                            return true;
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
                                    var dataWindow = new Mat(data.Rows, Count, data.Depth, data.Channels);
                                    var windowIndex = UpdateWindow(data, dataWindow, i, 0);
                                    if (windowIndex < dataWindow.Cols)
                                    {
                                        // Window is missing data, add to list
                                        dataWindows.Add(DataWindow.Create(dataWindow, windowIndex));
                                    }
                                    // Window is ready, emit
                                    else observer.OnNext(dataWindow);
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
