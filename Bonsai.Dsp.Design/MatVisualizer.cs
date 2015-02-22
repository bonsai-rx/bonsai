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

namespace Bonsai.Dsp.Design
{
    public class MatVisualizer : MatVisualizer<WaveformView>
    {
    }

    public class MatVisualizer<TWaveformView> : DialogTypeVisualizer where TWaveformView : WaveformView, new()
    {
        const int TargetElapsedTime = (int)(1000.0 / 30);
        bool requireInvalidate;
        Timer updateTimer;
        TWaveformView graph;

        public MatVisualizer()
        {
            XMax = 1;
            YMax = 1;
            AutoScaleX = true;
            AutoScaleY = true;
            ChannelsPerPage = 16;
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

        public int SelectedPage { get; set; }

        public int ChannelsPerPage { get; set; }

        public bool OverlayChannels { get; set; }

        public double ChannelOffset { get; set; }

        public int HistoryLength { get; set; }

        public int WaveformBufferLength { get; set; }

        public int[] SelectedChannels { get; set; }

        protected internal TWaveformView Graph
        {
            get { return graph; }
        }

        protected void InvalidateGraph()
        {
            requireInvalidate = true;
        }

        public override void Load(IServiceProvider provider)
        {
            graph = new TWaveformView();
            graph.Dock = DockStyle.Fill;
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

            graph.SelectedPage = SelectedPage;
            graph.ChannelsPerPage = ChannelsPerPage;
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
                SelectedPage = graph.SelectedPage;
                ChannelsPerPage = graph.ChannelsPerPage;
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
            if (buffer == null) return;

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
            InvalidateGraph();
        }
    }
}
