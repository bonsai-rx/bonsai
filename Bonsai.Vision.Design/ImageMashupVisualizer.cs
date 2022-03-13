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
    /// <summary>
    /// Provides an abstract base class for type visualizers that overlay multiple
    /// values into a single image display.
    /// </summary>
    public abstract class ImageMashupVisualizer : DialogMashupVisualizer
    {
        const int TargetInterval = 16;
        IObserver<IList<object>> activeObserver;
        IList<object> activeValues;
        IList<object> shownValues;
        IplImage visualizerCache;
        Timer updateTimer;

        /// <summary>
        /// Gets the image buffer used to construct the displayed visualizer background.
        /// </summary>
        [XmlIgnore]
        public IplImage VisualizerImage { get; private set; }

        /// <summary>
        /// When overridden in a derived class, gets the graphics canvas used to
        /// render the final visualizer output.
        /// </summary>
        public abstract VisualizerCanvas VisualizerCanvas { get; }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            updateTimer = new Timer();
            updateTimer.Interval = TargetInterval;
            updateTimer.Tick += updateTimer_Tick;
            base.Load(provider);
            updateTimer.Start();
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var inputImage = (IplImage)value;
            if (Mashups.Count > 0)
            {
                visualizerCache = IplImageHelper.EnsureImageFormat(visualizerCache, inputImage.Size, inputImage.Depth, inputImage.Channels);
                CV.Copy(inputImage, visualizerCache);
                VisualizerImage = visualizerCache;
            }
            else VisualizerImage = inputImage;
        }

        /// <summary>
        /// Combines the specified collection of values into a single image mashup.
        /// </summary>
        /// <param name="values">
        /// The collection of values to be displayed by the mashup visualizer.
        /// </param>
        protected virtual void ShowMashup(IList<object> values)
        {
            shownValues = values;
            foreach (var pair in values.Zip(
                Mashups.Select(xs => (DialogTypeVisualizer)xs.Visualizer).Prepend(this),
                (value, visualizer) => Tuple.Create(value, visualizer)))
            {
                pair.Item2.Show(pair.Item1);
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

        /// <summary>
        /// Updates the type visualizer state in preparation for displaying the
        /// specified collection of values.
        /// </summary>
        /// <param name="values">
        /// The collection of values to be displayed by the mashup visualizer.
        /// </param>
        protected virtual void UpdateValues(IList<object> values)
        {
        }

        private void UpdateCanvas(IList<object> values)
        {
            var canvas = VisualizerCanvas;
            if (values != null && canvas != null)
            {
                canvas.BeginInvoke((Action)(() =>
                {
                    try { ShowMashup(values); }
                    catch (Exception ex)
                    {
                        var observer = activeObserver;
                        if (observer != null)
                        {
                            observer.OnError(ex);
                        }
                    }
                }));
            }
        }

        /// <inheritdoc/>
        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            IObservable<IList<object>> dataSource;
            var mergedSource = source.SelectMany(xs => xs.Do(
                ys => { },
                () => VisualizerCanvas.BeginInvoke((Action)SequenceCompleted)));

            if (Mashups.Count > 0)
            {
                var mergedMashups = Mashups.Select(xs => xs.Visualizer.Visualize(((ITypeVisualizerContext)xs).Source.Output, provider).Publish().RefCount()).ToArray();
                dataSource = Observable
                    .CombineLatest(mergedMashups.Prepend(mergedSource))
                    .Window(mergedMashups.Last())
                    .SelectMany(window => window.TakeLast(1));
            }
            else dataSource = mergedSource.Select(xs => new[] { xs });

            return Observable.Create<IList<object>>(observer =>
            {
                Interlocked.Exchange(ref activeObserver, observer);
                return dataSource.Do(xs =>
                {
                    UpdateValues(xs);
                    if (Interlocked.Exchange(ref activeValues, xs) == null)
                    {
                        UpdateCanvas(xs);
                    }
                }).SubscribeSafe(observer);
            });
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            updateTimer.Stop();
            base.Unload();
            updateTimer.Dispose();
            updateTimer = null;
            visualizerCache = null;
            VisualizerImage = null;
            activeObserver = null;
        }
    }

    static class EnumerableExtensions
    {
        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            yield return value;
            foreach (var x in source)
            {
                yield return x;
            }
        }
    }
}
