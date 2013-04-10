using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Dsp.Design;
using Bonsai.Design;
using OpenCV.Net;
using System.Runtime.InteropServices;

[assembly: TypeVisualizer(typeof(CvMatVisualizer), Target = typeof(CvMat))]

namespace Bonsai.Dsp.Design
{
    public class CvMatVisualizer : DialogTypeVisualizer
    {
        const int DefaultBufferSize = 640;
        static readonly TimeSpan TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 20);

        int bufferSize;
        CvMatControl chart;
        List<object> valuesX;
        List<object>[] valuesY;
        List<object> sampleIntervals;

        double channelOffset;
        int blockSize = 100;
        DateTimeOffset updateTime;

        public CvMatVisualizer()
        {
            this.bufferSize = DefaultBufferSize;
            valuesX = new List<object>();
            sampleIntervals = new List<object>();
        }

        protected CvMatControl Chart
        {
            get { return chart; }
        }

        protected void ResetNumericSeries(int numSeries)
        {
            valuesY = new List<object>[numSeries];
            for (int i = 0; i < valuesY.Length; i++)
            {
                valuesY[i] = new List<object>();
            }

            for (int i = 1; i < valuesY.Length; i++)
            {
                var series = chart.TimeSeries.Add(chart.TimeSeries[0].Name + i);
                series.ChartType = chart.TimeSeries[0].ChartType;
                series.XValueType = chart.TimeSeries[0].XValueType;
                series.ChartArea = chart.TimeSeries[0].ChartArea;
            }
        }

        protected void AddValue(object xValue, params double[] value)
        {
            var excess = sampleIntervals.Count - bufferSize;
            if (excess > 0)
            {
                sampleIntervals.RemoveRange(0, excess);
                Array.ForEach(valuesY, y => y.RemoveRange(0, excess));
            }

            sampleIntervals.Add(xValue);
            for (int i = 0; i < valuesY.Length; i++)
            {
                valuesY[i].Add(value[i]);
            }
        }

        protected void ClearValues()
        {
            valuesX.Clear();
            sampleIntervals.Clear();
            valuesY = null;
        }

        protected void DataBindValues()
        {
            var sum = 0.0;
            valuesX.Clear();
            foreach (var time in sampleIntervals)
            {
                sum += Convert.ToDouble(time);
                valuesX.Add(sum);
            }

            for (int i = 0; i < valuesY.Length; i++)
            {
                chart.TimeSeries[i].Points.DataBindXY(valuesX, valuesY[i]);
            }
        }

        private void SetBlockSize(int size)
        {
            blockSize = Math.Max(1, size);
            chart.Chart.Titles["Scale"].Text = string.Format("Scale: 1/{0}", blockSize);
        }

        public override void Load(IServiceProvider provider)
        {
            channelOffset = 0;
            chart = new CvMatControl();
            chart.Chart.MouseDoubleClick += (sender, e) => SetBlockSize((int)(blockSize * ((e.Button == System.Windows.Forms.MouseButtons.Left) ? 10 : 0.1)));
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
        }

        public override void Show(object value)
        {
            var buffer = (CvMat)value;
            var now = HighResolutionScheduler.Now;

            var rows = buffer.Rows;
            var columns = buffer.Cols;
            if (valuesY == null || valuesY.Length != rows)
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

            if ((now - updateTime) > TargetElapsedTime)
            {
                DataBindValues();
                updateTime = now;
            }
        }
    }
}
