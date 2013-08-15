using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Threading;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Produces a video sequence of images acquired from the specified camera index.")]
    public class CameraCapture : Source<IplImage>
    {
        IObservable<IplImage> source;

        public CameraCapture()
        {
            source = Observable
                .Using(
                    () => CvCapture.CreateCameraCapture(Index),
                    capture => ObservableCombinators.GenerateWithThread<IplImage>(observer =>
                    {
                        var image = capture.QueryFrame();
                        if (image == null)
                        {
                            observer.OnCompleted();
                        }
                        else observer.OnNext(image.Clone());
                    }))
                .PublishReconnectable()
                .RefCount();
        }

        [Description("The index of the camera from which to acquire images.")]
        public int Index { get; set; }

        public override IObservable<IplImage> Generate()
        {
            return source;
        }
    }
}
