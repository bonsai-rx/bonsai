using System;
using OpenCV.Net;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that generates a sequence of images acquired from
    /// the specified camera.
    /// </summary>
    [DefaultProperty(nameof(Index))]
    [WorkflowElementIcon(typeof(ElementCategory), "ElementIcon.Video")]
    [Description("Generates a sequence of images acquired from the specified camera.")]
    public class CameraCapture : Source<IplImage>
    {
        readonly IObservable<IplImage> source;
        readonly object captureLock = new object();
        readonly CapturePropertyCollection captureProperties = new CapturePropertyCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraCapture"/> class.
        /// </summary>
        public CameraCapture()
        {
            source = Observable.Create<IplImage>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    lock (captureLock)
                    {
                        using (var capture = Capture.CreateCameraCapture(Index))
                        {
                            foreach (var setting in captureProperties)
                            {
                                capture.SetProperty(setting.Property, setting.Value);
                            }
                            captureProperties.Capture = capture;
                            try
                            {
                                while (!cancellationToken.IsCancellationRequested)
                                {
                                    var image = captureProperties.Capture.QueryFrame();
                                    if (image == null)
                                    {
                                        observer.OnError(new InvalidOperationException("Unable to acquire camera frame."));
                                        break;
                                    }
                                    else observer.OnNext(image.Clone());
                                }
                            }
                            finally { captureProperties.Capture = null; }
                        }
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            })
            .PublishReconnectable()
            .RefCount();
        }

        /// <summary>
        /// Gets or sets the index of the camera from which to acquire images.
        /// </summary>
        [Description("The index of the camera from which to acquire images.")]
        public int Index { get; set; }

        /// <summary>
        /// Gets the set of capture properties assigned to the camera.
        /// </summary>
        [Description("The set of capture properties assigned to the camera.")]
        public CapturePropertyCollection CaptureProperties
        {
            get { return captureProperties; }
        }

        /// <summary>
        /// Generates an observable sequence of images acquired from
        /// the camera with the specified index.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing each frame
        /// acquired from the camera.
        /// </returns>
        public override IObservable<IplImage> Generate()
        {
            return source;
        }
    }
}
