using System;
using System.Linq;
using Bonsai;
using Bonsai.Dsp.Design;
using Bonsai.Design;
using OpenCV.Net;
using System.Runtime.InteropServices;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Reactive;

[assembly: TypeVisualizer(typeof(MatVisualizer), Target = typeof(Mat))]

namespace Bonsai.Dsp.Design
{
    /// <summary>
    /// Provides a type visualizer for displaying a matrix as a waveform graph,
    /// using either separate or overlaying channels.
    /// </summary>
    public class MatVisualizer : MatVisualizer<WaveformView>
    {
    }

    /// <summary>
    /// Provides a base class for displaying data as a waveform graph.
    /// </summary>
    /// <typeparam name="TWaveformView">
    /// A type derived from <see cref="WaveformView"/> which will control how data is displayed.
    /// </typeparam>
    public class MatVisualizer<TWaveformView> : BufferedVisualizer where TWaveformView : WaveformView, new()
    {
        TWaveformView graph;

        /// <inheritdoc/>
        protected override int TargetInterval => 1000 / 30;

        /// <summary>
        /// Gets or sets the lower bound of the x-axis displayed in the graph.
        /// </summary>
        public double XMin { get; set; } = 0;

        /// <summary>
        /// Gets or sets the upper bound of the x-axis displayed in the graph.
        /// </summary>
        public double XMax { get; set; } = 1;

        /// <summary>
        /// Gets or sets the lower bound of the y-axis displayed in the graph.
        /// </summary>
        public double YMin { get; set; } = 0;

        /// <summary>
        /// Gets or sets the upper bound of the y-axis displayed in the graph.
        /// </summary>
        public double YMax { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether to compute the range of
        /// the x-axis automatically based on the range of the data that is
        /// included in the graph.
        /// </summary>
        public bool AutoScaleX { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to compute the range of
        /// the y-axis automatically based on the range of the data that is
        /// included in the graph.
        /// </summary>
        public bool AutoScaleY { get; set; } = true;

        /// <summary>
        /// Gets or sets the currently selected channel page. Channels in the
        /// currently selected page will be the ones displayed in the graph.
        /// </summary>
        public int SelectedPage { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of channels which should be included
        /// in a single page.
        /// </summary>
        public int ChannelsPerPage { get; set; } = 16;

        /// <summary>
        /// Gets or sets a value indicating whether to overlay the traces of all
        /// the channels in the page into a single waveform graph. If this value
        /// is <see langword="false"/>, channels will be displayed individually
        /// in separate graph panes.
        /// </summary>
        public bool OverlayChannels { get; set; } = true;

        /// <summary>
        /// Gets or sets a value which will be added to the samples of each channel,
        /// proportional to channel index, for the purposes of visualization.
        /// </summary>
        public double ChannelOffset { get; set; }

        /// <summary>
        /// Gets or sets a value specifying how many previous data buffers to store
        /// and display in the graph.
        /// </summary>
        /// <remarks>
        /// Each buffer can contain multiple samples, which means the total number of
        /// samples displayed in the graph will be <c>HistoryLength * BufferLength</c>,
        /// where <c>BufferLength</c> is the number of samples per buffer.
        /// </remarks>
        public int HistoryLength { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying how many previous traces to overlay for
        /// each channel.
        /// </summary>
        /// <remarks>
        /// This allows overlaying historical traces rather than appending them in time.
        /// </remarks>
        public int WaveformBufferLength { get; set; } = 1;

        /// <summary>
        /// Gets or sets the indices of the channels to display when the visualizer
        /// is in overlay mode.
        /// </summary>
        public int[] SelectedChannels { get; set; }

        /// <summary>
        /// Gets the graph control used to display the data.
        /// </summary>
        protected internal TWaveformView Graph
        {
            get { return graph; }
        }

        /// <summary>
        /// Invalidates the entire graph display at the next data update.
        /// This will send a paint message to the graph control.
        /// </summary>
        [Obsolete]
        protected void InvalidateGraph()
        {
        }

        /// <inheritdoc/>
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
            }
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            graph.Dispose();
            graph = null;
        }

        /// <inheritdoc/>
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
        }

        /// <inheritdoc/>
        protected override void ShowBuffer(IList<Timestamped<object>> values)
        {
            base.ShowBuffer(values);
            if (values.Count > 0)
            {
                graph.InvalidateWaveform();
            }
        }
    }
}
