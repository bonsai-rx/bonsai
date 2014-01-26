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

[assembly: TypeVisualizer(typeof(MatVisualizer), Target = typeof(Mat))]
[assembly: TypeVisualizer(typeof(MatVisualizer), Target = typeof(IObservable<Mat>))]

namespace Bonsai.Dsp.Design
{
    public class MatVisualizer : DialogTypeVisualizer
    {
        const int TargetElapsedTime = (int)(1000.0 / 30);
        bool requireInvalidate;
        Timer updateTimer;
        WaveformView graph;

        public MatVisualizer()
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

        public int[] SelectedChannels { get; set; }

        public override void Load(IServiceProvider provider)
        {
            graph = new WaveformView { Dock = DockStyle.Fill };
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

            graph.OverlayChannels = OverlayChannels;
            if (SelectedChannels != null)
            {
                foreach (var channel in SelectedChannels)
                {
                    graph.SelectedChannels.Add(channel);
                }
            }

            graph.HandleDestroyed += delegate
            {
                XMin = graph.XMin;
                XMax = graph.XMax;
                YMin = graph.YMin;
                YMax = graph.YMax;
                AutoScaleX = graph.AutoScaleX;
                AutoScaleY = graph.AutoScaleY;
                OverlayChannels = graph.OverlayChannels;
                WaveformBufferLength = graph.WaveformBufferLength;
                HistoryLength = graph.HistoryLength;
                ChannelOffset = graph.ChannelOffset;
                if (graph.SelectedChannels.Count > 0)
                {
                    SelectedChannels = graph.SelectedChannels.ToArray();
                }
                else SelectedChannels = null;
            };

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
            var visualizerContext = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            if (graph != null && visualizerContext != null)
            {
                var observableType = visualizerContext.Source.ObservableType;
                if (observableType == typeof(IObservable<Mat>))
                {
                    return source.SelectMany(xs => xs.Select(ws => ws as IObservable<Mat>)
                                                     .Where(ws => ws != null)
                                                     .SelectMany(ws => ws.ObserveOn(graph)
                                                                         .Do(Show, SequenceCompleted)));
                }
                else return source.SelectMany(xs => xs.ObserveOn(graph).Do(Show, SequenceCompleted));
            }

            return source;
        }

        public override void SequenceCompleted()
        {
            base.SequenceCompleted();
        }
    }
}
