using Bonsai.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Bonsai.Dsp.Design
{
    public class SpikeThresholdEditor : DataSourceTypeEditor
    {
        const string AutoStart = "Auto Start";
        const string AutoStop = "Auto Stop";
        const string DeviationLabel = "SD";

        public SpikeThresholdEditor()
            : base(DataSource.Input, typeof(Mat))
        {
        }

        public override bool IsDropDownResizable
        {
            get { return true; }
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                var subscription = default(IDisposable);
                var propertyDescriptor = context.PropertyDescriptor;
                var source = GetDataSource(context, provider);
                var chart = new ChartPanel(source, provider);
                chart.Dock = DockStyle.Fill;

                var autoSetButton = new Button { Text = AutoStart };
                autoSetButton.Dock = DockStyle.Fill;
                var autoSetButtonClicked = Observable.FromEventPattern<EventHandler, EventArgs>(
                    handler => autoSetButton.Click += handler,
                    handler => autoSetButton.Click -= handler);

                var autoSetDeviationLabel = new Label();
                autoSetDeviationLabel.Dock = DockStyle.Fill;
                autoSetDeviationLabel.Text = DeviationLabel;
                autoSetDeviationLabel.TextAlign = ContentAlignment.MiddleCenter;

                var autoSetDeviationUpDown = new NumericUpDown();
                var upDownMargin = autoSetDeviationUpDown.Margin;
                autoSetDeviationUpDown.Margin = new Padding(upDownMargin.Left, upDownMargin.Top + 1, upDownMargin.Right, upDownMargin.Bottom);
                autoSetDeviationUpDown.Anchor = AnchorStyles.None;

                var autoSetPanel = new TableLayoutPanel();
                autoSetPanel.Dock = DockStyle.Top;
                autoSetPanel.Height = autoSetButton.Height + autoSetPanel.Margin.Top;
                autoSetPanel.GrowStyle = TableLayoutPanelGrowStyle.AddColumns;
                autoSetPanel.Controls.Add(autoSetDeviationLabel, 0, 0);
                autoSetPanel.Controls.Add(autoSetDeviationUpDown, 1, 0);
                autoSetPanel.Controls.Add(autoSetButton, 2, 0);
                autoSetPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, autoSetDeviationLabel.PreferredWidth + autoSetPanel.Margin.Left + autoSetPanel.Margin.Right));
                autoSetPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, autoSetDeviationUpDown.PreferredSize.Width));
                autoSetPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                autoSetDeviationUpDown.Maximum = 99;
                autoSetDeviationUpDown.Minimum = -99;

                var editorPanel = new TableLayoutPanel();
                editorPanel.ClientSize = new System.Drawing.Size(320, 320);
                editorPanel.RowCount = 2;
                editorPanel.ColumnCount = 1;
                editorPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 99));
                editorPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                editorPanel.Controls.Add(chart);
                editorPanel.Controls.Add(autoSetPanel);

                var activeVisualizer = provider.GetService(typeof(DialogTypeVisualizer)) as SpikeWaveformCollectionVisualizer;
                var spikeVisualizer = new MatVisualizer<WaveformThresholdPicker>();
                if (activeVisualizer != null)
                {
                    spikeVisualizer.AutoScaleX = activeVisualizer.AutoScaleX;
                    spikeVisualizer.AutoScaleY = activeVisualizer.AutoScaleY;
                    spikeVisualizer.ChannelOffset = activeVisualizer.ChannelOffset;
                    spikeVisualizer.ChannelsPerPage = activeVisualizer.ChannelsPerPage;
                    spikeVisualizer.HistoryLength = activeVisualizer.HistoryLength;
                    spikeVisualizer.OverlayChannels = activeVisualizer.OverlayChannels;
                    spikeVisualizer.SelectedChannels = activeVisualizer.SelectedChannels;
                    spikeVisualizer.SelectedPage = activeVisualizer.SelectedPage;
                    spikeVisualizer.WaveformBufferLength = activeVisualizer.WaveformBufferLength;
                    spikeVisualizer.XMax = activeVisualizer.XMax;
                    spikeVisualizer.XMin = activeVisualizer.XMin;
                    spikeVisualizer.YMax = activeVisualizer.YMax;
                    spikeVisualizer.YMin = activeVisualizer.YMin;
                }
                else
                {
                    spikeVisualizer.OverlayChannels = false;
                    spikeVisualizer.WaveformBufferLength = 10;
                }
                spikeVisualizer.Load(chart);
                var thresholdPicker = spikeVisualizer.Graph;
                thresholdPicker.Threshold = (double[])value;
                thresholdPicker.ThresholdChanged += (sender, e) => propertyDescriptor.SetValue(context.Instance, value = thresholdPicker.Threshold);

                var visualizerObservable = spikeVisualizer.Visualize(source.Output, chart);
                var autoSetObservable = AutoThreshold(source.Output, autoSetButtonClicked, () => (double)autoSetDeviationUpDown.Value)
                    .Do(ts => propertyDescriptor.SetValue(context.Instance, value = thresholdPicker.Threshold = ts));
                chart.HandleCreated += delegate { subscription = new CompositeDisposable(visualizerObservable.Subscribe(), autoSetObservable.Subscribe()); };
                editorPanel.Leave += delegate { editorService.CloseDropDown(); subscription.Dispose(); };
                autoSetButton.Click += delegate { autoSetButton.Text = autoSetButton.Text == AutoStart ? AutoStop : AutoStart; };
                try
                {
                    editorService.DropDownControl(editorPanel);
                }
                finally
                {
                    chart.Dispose();
                    autoSetButton.Dispose();
                    editorPanel.Dispose();
                    spikeVisualizer.Unload();
                }

                return value;
            }

            return base.EditValue(context, provider, value);
        }

        static IObservable<double[]> AutoThreshold<TOther>(IObservable<IObservable<object>> source, IObservable<TOther> bufferBoundaries, Func<double> scale)
        {
            var concat = new Concat { Axis = 1 };
            var firstBoundary = Observable.Return<TOther>(default(TOther));
            bufferBoundaries = firstBoundary.Concat(bufferBoundaries);
            return bufferBoundaries.Publish(ps => ps.Window(2).Skip(1).SelectMany(start => Observable.Defer(() =>
            {
                var n = 0;
                var mean = default(double[]);
                var variance = default(double[]);
                var buffer = default(double[]);
                return source.SelectMany(xs => xs.Cast<Mat>()).TakeUntil(ps).Select(xs =>
                {
                    if (mean == null)
                    {
                        mean = new double[xs.Rows];
                        variance = new double[xs.Rows];
                        buffer = new double[xs.Rows * xs.Cols];
                    }

                    // Convert data into temporary buffer
                    var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    try
                    {
                        using (var bufferHeader = new Mat(xs.Rows, xs.Cols, Depth.F64, 1, bufferHandle.AddrOfPinnedObject()))
                        {
                            CV.Convert(xs, bufferHeader);
                        }
                    }
                    finally { bufferHandle.Free(); }

                    // Knuth's online variance algorithm
                    // http://en.wikipedia.org/wiki/Algorithms_for_calculating_variance
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        var row = i / xs.Cols;
                        var col = i % xs.Cols;
                        var previousMean = mean[row];
                        var delta = buffer[i] - previousMean;
                        var newMean = previousMean + delta / (n + i + 1);
                        variance[row] = variance[row] + delta * (buffer[i] - newMean);
                        mean[row] = newMean;
                    }

                    n += xs.Cols;
                    return xs;
                }).TakeLast(1).Select(xs =>
                {
                    var result = new double[xs.Rows];
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = mean[i] + scale() * Math.Sqrt(variance[i] / (n - 1));
                    }
                    return result;
                });
            })));
        }
    }
}
