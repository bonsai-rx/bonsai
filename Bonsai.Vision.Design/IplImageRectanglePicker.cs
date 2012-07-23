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
        CvRect rectangle;

        public IplImageRectanglePicker()
        {
            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(PictureBox, "MouseMove").Select(e => e.EventArgs);
            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(PictureBox, "MouseDown").Select(e => e.EventArgs);
            var mouseUp = Observable.FromEventPattern<MouseEventArgs>(PictureBox, "MouseUp").Select(e => e.EventArgs);

            var mousePick = (from downEvt in mouseDown
                             where Image != null && downEvt.Button.HasFlag(MouseButtons.Left)
                             let origin = new CvPoint(downEvt.X, downEvt.Y)
                             select from moveEvt in mouseMove.TakeUntil(mouseUp)
                                    select new CvRect(origin.X, origin.Y, moveEvt.X - origin.X, moveEvt.Y - origin.Y)).Switch();

            mousePick.Subscribe(rect => rectangle = NormalizedRectangle(rect));
            mouseUp.Subscribe(evt =>
            {
                rectangle.X = Math.Min(rectangle.X, rectangle.X + rectangle.Width);
                rectangle.Y = Math.Min(rectangle.Y, rectangle.Y + rectangle.Height);
                rectangle.Width = Math.Abs(rectangle.Width);
                rectangle.Height = Math.Abs(rectangle.Height);
                rectangle = ClipRectangle(rectangle);
                OnRectangleChanged(EventArgs.Empty);
            });
        }

        CvRect ClipRectangle(CvRect rect)
        {
            var clipX = rect.X < 0 ? -rect.X : 0;
            var clipY = rect.Y < 0 ? -rect.Y : 0;
            clipX += Math.Max(0, rect.X + rect.Width - Image.Width);
            clipY += Math.Max(0, rect.Y + rect.Height - Image.Height);

            rect.X = Math.Max(0, rect.X);
            rect.Y = Math.Max(0, rect.Y);
            rect.Width = rect.Width - clipX;
            rect.Height = rect.Height - clipY;
            return rect;
        }

        CvRect NormalizedRectangle(CvRect rect)
        {
            return new CvRect(
                (int)(rect.X * Image.Width / (float)PictureBox.Width),
                (int)(rect.Y * Image.Height / (float)PictureBox.Height),
                (int)(rect.Width * Image.Width / (float)PictureBox.Width),
                (int)(rect.Height * Image.Width / (float)PictureBox.Width));
        }

        public CvRect Rectangle
        {
            get { return rectangle; }
            set { rectangle = value; }
        }

        public event EventHandler RectangleChanged;

        protected virtual void OnRectangleChanged(EventArgs e)
        {
            var handler = RectangleChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void SetImage(IplImage image)
        {
            using (image = image.ColorClone())
            {
                Core.cvRectangle(
                    image,
                    new CvPoint(rectangle.X, rectangle.Y),
                    new CvPoint(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height),
                    CvScalar.Rgb(255, 0, 0), 3, 8, 0);
                base.SetImage(image);
            }
        }
    }
}
