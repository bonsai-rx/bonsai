using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Dsp.Design;
using Bonsai.Design;
using OpenCV.Net;
using System.Runtime.InteropServices;
using System.Reactive.Linq;
using ZedGraph;
using System.Windows.Forms;
using Bonsai.Design.Visualizers;

[assembly: TypeVisualizer(typeof(CvMatVisualizer), Target = typeof(Mat))]
[assembly: TypeVisualizer(typeof(CvMatVisualizer), Target = typeof(IObservable<Mat>))]

namespace Bonsai.Dsp.Design
{
    public class CvMatVisualizer : DialogTypeVisualizer
    {
        const int TargetElapsedTime = (int)(1000.0 / 60);
        bool requireInvalidate;
        Timer updateTimer;
        WaveformGraph graph;

        public CvMatVisualizer()
        {
            XMax = 1;
            YMax = 1;
            AutoScaleX = true;
            AutoScaleY = true;
            OverlayChannels = true;
            WaveformBufferLength = 1;
            HistoryLength = 1;
        }

        public double XMin { get; set; }

        public double XMax { get; set; }

        public double YMin { get; set; }

        public double YMax { get; set; }

        public bool AutoScaleX { get; set; }

        public bool AutoScaleY { get; set; }

        public bool OverlayChannels { get; set; }

        public double ChannelOffset { get; set; }

        public int HistoryLength { get; set; }

        public int WaveformBufferLength { get; set; }

        public override void Load(IServiceProvider provider)
        {
            graph = new WaveformGraph { Dock = DockStyle.Fill };
            graph.WaveformBufferLength = WaveformBufferLength;
            graph.HistoryLength = HistoryLength;
            graph.ChannelOffset = ChannelOffset;
            graph.AutoScaleX = AutoScaleX;
            if (!AutoScaleX)
            {
                graph.XMin = XMin;
                graph.XMax = XMax;
            }

            graph.AutoScaleY = AutoScaleY;
            if (!AutoScaleY)
            {
                graph.YMin = YMin;
                graph.YMax = YMax;
            }

            EventHandler updateScale = (sender, e) =>
            {
                AutoScaleX = graph.AutoScaleX;
                if (!AutoScaleX)
                {
                    XMin = graph.XMin;
                    XMax = graph.XMax;
                }

                AutoScaleY = graph.AutoScaleY;
                if (!AutoScaleY)
                {
                    YMin = graph.YMin;
                    YMax = graph.YMax;
                }
            };
            graph.OverlayChannels = OverlayChannels;
            graph.AutoScaleXChanged += updateScale;
            graph.AutoScaleYChanged += updateScale;
            graph.AxisChanged += updateScale;

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(graph);
                updateTimer = new System.Windows.Forms.Timer();
                updateTimer.Interval = TargetElapsedTime;
                updateTimer.Tick += (sender, e) =>
                {
                    if (requireInvalidate)
                    {
                        graph.InvalidateWaveform();
                        requireInvalidate = false;
                    }
                };
                updateTimer.Start();
            }
        }

        public override void Unload()
        {
            OverlayChannels = graph.OverlayChannels;
            WaveformBufferLength = graph.WaveformBufferLength;
            HistoryLength = graph.HistoryLength;
            ChannelOffset = graph.ChannelOffset;
            updateTimer.Stop();
            updateTimer.Dispose();
            graph.Dispose();
            updateTimer = null;
            graph = null;
        }

        public override void Show(object value)
        {
            var buffer = (Mat)value;
            var rows = buffer.Rows;
            var columns = buffer.Cols;
            var samples = new double[rows * columns];
            var sampleHandle = GCHandle.Alloc(samples, GCHandleType.Pinned);
            using (var sampleHeader = new Mat(rows, columns, Depth.F64, 1, sampleHandle.AddrOfPinnedObject()))
            {
                CV.Convert(buffer, sampleHeader);
            }
            sampleHandle.Free();

            graph.UpdateWaveform(samples, rows, columns);
            requireInvalidate = true;
        }

        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            var visualizerDialog = (TypeVisualizerDialog)provider.GetService(typeof(TypeVisualizerDialog));
            var visualizerContext = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            if (visualizerDialog != null && visualizerContext != null)
            {
                var observableType = visualizerContext.Source.ObservableType;
                if (observableType == typeof(IObservable<Mat>))
                {
                    return source.SelectMany(xs => xs.Select(ws => ws as IObservable<Mat>)
                                                     .Where(ws => ws != null)
                                                     .SelectMany(ws => ws.ObserveOn(visualizerDialog)
                                                                         .Do(Show, SequenceCompleted)));
                }
                else return source.SelectMany(xs => xs.ObserveOn(visualizerDialog).Do(Show, SequenceCompleted));
            }

            return source;
        }

        public override void SequenceCompleted()
        {
            base.SequenceCompleted();
        }
    }
}
