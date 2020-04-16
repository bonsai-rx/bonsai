using Bonsai.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Xml.Serialization;
using Timer = System.Windows.Forms.Timer;

namespace Bonsai.Vision.Design
{
    public abstract class ImageMashupVisualizer : DialogMashupVisualizer
    {
        const int TargetInterval = 16;
        IList<object> activeValues;
        IList<object> shownValues;
        Timer updateTimer;

        [XmlIgnore]
        public IplImage VisualizerImage { get; private set; }

        public abstract VisualizerCanvas VisualizerCanvas { get; }

        public override void Load(IServiceProvider provider)
        {
            updateTimer = new Timer();
            updateTimer.Interval = TargetInterval;
            updateTimer.Tick += updateTimer_Tick;
            base.Load(provider);
            updateTimer.Start();
        }

        public override void Show(object value)
        {
            var inputImage = (IplImage)value;
            if (Mashups.Count > 0)
            {
                VisualizerImage = IplImageHelper.EnsureImageFormat(VisualizerImage, inputImage.Size, inputImage.Depth, inputImage.Channels);
                CV.Copy(inputImage, VisualizerImage);
            }
            else VisualizerImage = inputImage;
        }

        protected virtual void ShowMashup(IList<object> values)
        {
            shownValues = values;
            foreach (var (value, visualizer) in values.Zip(
                Mashups.Select(xs => (DialogTypeVisualizer)xs.Visualizer).Prepend(this),
                (value, visualizer) => (value, visualizer)))
            {
                visualizer.Show(value);
            }
        }

        void updateTimer_Tick(object sender, EventArgs e)
        {
            var values = Interlocked.Exchange(ref activeValues, null);
            if (values != shownValues)
            {
                UpdateCanvas(values);
            }

            shownValues = null;
        }

        protected virtual void UpdateValues(IList<object> values)
        {
        }

        private void UpdateCanvas(IList<object> values)
        {
            var canvas = VisualizerCanvas;
            if (values != null && canvas != null)
            {
                canvas.BeginInvoke((Action<IList<object>>)ShowMashup, values);
            }
        }

        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            IObservable<IList<object>> dataSource;
            var mergedSource = source.SelectMany(xs => xs.Do(
                ys => { },
                () => VisualizerCanvas.BeginInvoke((Action)SequenceCompleted)));

            if (Mashups.Count > 0)
            {
                var mergedMashups = Mashups.Select(xs => xs.Visualizer.Visualize(xs.Source, provider).Publish().RefCount()).ToArray();
                dataSource = Observable
                    .CombineLatest(mergedMashups.Prepend(mergedSource))
                    .Window(mergedMashups.Last())
                    .SelectMany(window => window.TakeLast(1));
            }
            else dataSource = mergedSource.Select(xs => new[] { xs });

            return dataSource.Do(xs =>
            {
                UpdateValues(xs);
                if (Interlocked.Exchange(ref activeValues, xs) == null)
                {
                    UpdateCanvas(xs);
                }
            });
        }

        public override void Unload()
        {
            updateTimer.Stop();
            base.Unload();
            updateTimer.Dispose();
            updateTimer = null;
            VisualizerImage = null;
        }
    }
}
