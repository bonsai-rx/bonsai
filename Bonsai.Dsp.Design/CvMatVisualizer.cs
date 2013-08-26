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

[assembly: TypeVisualizer(typeof(CvMatVisualizer), Target = typeof(CvMat))]
[assembly: TypeVisualizer(typeof(CvMatVisualizer), Target = typeof(IObservable<CvMat>))]

namespace Bonsai.Dsp.Design
{
    public class CvMatVisualizer : DialogTypeVisualizer
    {
        const int DefaultBufferSize = 640;
        const int SequenceBufferSize = 100;
        static readonly TimeSpan TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 20);
        static readonly TimeSpan WindowedTargetElapsedTime = TimeSpan.FromSeconds(1.0 / 10.0);

        int sequenceIndex;
        ChartControl chart;
        RollingPointPairList[] values;

        double channelOffset;
        int blockSize = 100;
        DateTimeOffset updateTime;
        TimeSpan targetElapsedTime;

        protected ChartControl Chart
        {
            get { return chart; }
        }

        protected void ResetNumericSeries(int numSeries)
        {
            var timeSeries = chart.GraphPane.CurveList;
            values = new RollingPointPairList[numSeries];
            for (int i = 0; i < values.Length; i++)
            {
                var seriesIndex = sequenceIndex * values.Length + i;
                if (seriesIndex < timeSeries.Count)
                {
                    values[i] = (RollingPointPairList)timeSeries[seriesIndex].Points;
                    values[i].Clear();
                }
                else values[i] = new RollingPointPairList(DefaultBufferSize);
            }

            if (sequenceIndex * values.Length >= timeSeries.Count || values.Length >= timeSeries.Count)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    var series = new LineItem(string.Empty, values[i], chart.GetNextColor(), SymbolType.None);
                    series.Line.IsAntiAlias = true;
                    series.Line.IsOptimizedDraw = true;
                    series.Label.IsVisible = false;
                    timeSeries.Add(series);
                }
            }
        }

        protected void AddValue(double xValue, params double[] value)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i].Add(0, value[i]);
            }
        }

        protected void ClearValues()
        {
            values = null;
        }

        protected void DataBindValues()
        {
            chart.AxisChange();
            chart.Invalidate();
        }

        private void SetBlockSize(int size)
        {
            var xlabelText = "Samples";
            blockSize = Math.Max(1, size);
            chart.GraphPane.Title.IsVisible = true;
            chart.GraphPane.Title.Text = string.Format("Scale: 1/{0}", blockSize);
            chart.GraphPane.XAxis.Title.Text = blockSize > 1 ? string.Format(xlabelText + " (10^{0})", Math.Log10(blockSize)) : xlabelText;
        }

        public override void Load(IServiceProvider provider)
        {
            channelOffset = 0;
            chart = new ChartControl();
            chart.GraphPane.XAxis.Type = AxisType.Ordinal;
            chart.GraphPane.XAxis.MinorTic.IsAllTics = false;
            chart.GraphPane.XAxis.Title.IsVisible = true;
            chart.GraphPane.XAxis.Scale.BaseTic = 0;
            chart.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.PageUp) SetBlockSize((int)(blockSize * 10));
                if (e.KeyCode == Keys.PageDown) SetBlockSize((int)(blockSize * 0.1));
            };

            updateTime = HighResolutionScheduler.Now;
            SetBlockSize(blockSize);

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(chart);
            }
        }

        public override void Unload()
        {
            ClearValues();
            chart.Dispose();
            chart = null;
            sequenceIndex = 0;
        }

        public override void Show(object value)
        {
            var buffer = (CvMat)value;
            var now = HighResolutionScheduler.Now;

            var rows = buffer.Rows;
            var columns = buffer.Cols;
            if (values == null || values.Length != rows)
            {
                ResetNumericSeries(rows);
            }

            var samples = new double[rows, columns];
            var sampleHandle = GCHandle.Alloc(samples, GCHandleType.Pinned);
            using (var sampleHeader = new CvMat(rows, columns, CvMatDepth.CV_64F, 1, sampleHandle.AddrOfPinnedObject()))
            {
                Core.cvConvert(buffer, sampleHeader);
            }
            sampleHandle.Free();

            var maxValues = new double[rows];
            for (int j = 0; j < columns; j += blockSize)
            {
                for (int i = 0; i < maxValues.Length; i++)
                {
                    maxValues[i] = samples[i, j];
                    for (int k = 1; j + k < Math.Min(j + blockSize, columns); k++)
                    {
                        var sample = samples[i, j + k];
                        maxValues[i] = Math.Max(sample, maxValues[i]);
                    }

                    channelOffset = Math.Max(channelOffset, maxValues[i]);
                    maxValues[i] += i * channelOffset;
                }

                AddValue(blockSize, maxValues);
            }

            if ((now - updateTime) > targetElapsedTime)
            {
                DataBindValues();
                updateTime = now;
            }
        }

        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            var visualizerDialog = (TypeVisualizerDialog)provider.GetService(typeof(TypeVisualizerDialog));
            var visualizerContext = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            if (visualizerDialog != null && visualizerContext != null)
            {
                var observableType = visualizerContext.Source.ObservableType;
                if (observableType == typeof(IObservable<CvMat>))
                {
                    SetBlockSize(1);
                    targetElapsedTime = WindowedTargetElapsedTime;
                    return source.SelectMany(xs => xs.Select(ws => ws as IObservable<CvMat>)
                                                     .Where(ws => ws != null)
                                                     .SelectMany(ws => ws.ObserveOn(visualizerDialog)
                                                                         .Do(Show, SequenceCompleted)));
                }
                else
                {
                    targetElapsedTime = TargetElapsedTime;
                    return source.SelectMany(xs => xs.ObserveOn(visualizerDialog).Do(Show, SequenceCompleted));
                }
            }

            return source;
        }

        public override void SequenceCompleted()
        {
            if (values != null)
            {
                ClearValues();
                sequenceIndex = (sequenceIndex + 1) % SequenceBufferSize;
            }
            base.SequenceCompleted();
        }
    }
}
