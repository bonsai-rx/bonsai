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
        CvCapture capture;

        [Description("The index of the camera from which to acquire images.")]
        public int Index { get; set; }

        public override IDisposable Load()
        {
            capture = CvCapture.CreateCameraCapture(Index);
            return base.Load();
        }

        protected override void Unload()
        {
            capture.Close();
            base.Unload();
        }

        protected override IObservable<IplImage> Generate()
        {
            return ObservableCombinators.GenerateWithThread<IplImage>(observer =>
            {
                var image = capture.QueryFrame();
                if (image == null)
                {
                    observer.OnCompleted();
                }
                else observer.OnNext(image.Clone());
            });
        }
    }
}
