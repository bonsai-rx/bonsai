using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Reactive;

namespace Bonsai.Vision.Design
{
    class IplImageRectanglePicker : IplImageControl
    {
        CvRect pickedRectangle;
        CvRect rectangleSample;

        public IplImageRectanglePicker()
        {
            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(pictureBox, "MouseMove").Select(e => e.EventArgs);
            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(pictureBox, "MouseDown").Select(e => e.EventArgs);
            var mouseUp = Observable.FromEventPattern<MouseEventArgs>(pictureBox, "MouseUp").Select(e => e.EventArgs);

            var mousePick = from downEvt in mouseDown
                            where pictureBox.Image != null && downEvt.Button.HasFlag(MouseButtons.Left)
                            let origin = new CvPoint(downEvt.X, downEvt.Y)
                            from moveEvt in mouseMove.TakeUntil(mouseUp)
                            select new CvRect(origin.X, origin.Y, moveEvt.X - origin.X, moveEvt.Y - origin.Y);

            mousePick.Subscribe(rect => rectangleSample = NormalizedRectangle(rect));
            mouseUp.Subscribe(evt =>
            {
                rectangleSample.X = Math.Min(rectangleSample.X, rectangleSample.X + rectangleSample.Width);
                rectangleSample.Y = Math.Min(rectangleSample.Y, rectangleSample.Y + rectangleSample.Height);
                rectangleSample.Width = Math.Abs(rectangleSample.Width);
                rectangleSample.Height = Math.Abs(rectangleSample.Height);
                pickedRectangle = rectangleSample;
                rectangleSample = new CvRect(0, 0, 0, 0);
                OnPickedRectangleChanged(EventArgs.Empty);
            });
        }

        CvRect NormalizedRectangle(CvRect rect)
        {
            return new CvRect(
                (int)(rect.X * pictureBox.Image.Width / (float)pictureBox.Width),
                (int)(rect.Y * pictureBox.Image.Height / (float)pictureBox.Height),
                (int)(rect.Width * pictureBox.Image.Width / (float)pictureBox.Width),
                (int)(rect.Height * pictureBox.Image.Width / (float)pictureBox.Width));
        }

        public CvRect PickedRectangle
        {
            get { return pickedRectangle; }
        }

        public event EventHandler PickedRectangleChanged;

        protected virtual void OnPickedRectangleChanged(EventArgs e)
        {
            var handler = PickedRectangleChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public override IplImage Image
        {
            get { return base.Image; }
            set
            {
                var image = value.Clone();
                Core.cvRectangle(
                    image,
                    new CvPoint(rectangleSample.X, rectangleSample.Y),
                    new CvPoint(rectangleSample.X + rectangleSample.Width, rectangleSample.Y + rectangleSample.Height),
                    CvScalar.Rgb(255, 0, 0),
                    -1, 8, 0);
                base.Image = image;
            }
        }
    }
}
