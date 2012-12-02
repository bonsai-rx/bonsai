using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Vision.Design
{
    class IplImageQuadranglePicker : IplImageControl
    {
        bool disposed;
        IplImage canvas;
        CvPoint2D32f[] quadrangle = new CvPoint2D32f[4];

        public IplImageQuadranglePicker()
        {
            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(PictureBox, "MouseMove").Select(e => e.EventArgs);
            var mouseDrag = from evt in mouseMove
                            where Image != null && evt.Button.HasFlag(MouseButtons.Left)
                            select new CvPoint2D32f(
                                evt.X * Image.Width / (float)PictureBox.Width,
                                evt.Y * Image.Height / (float)PictureBox.Height);

            mouseDrag.Subscribe(point =>
            {
                int cornerIndex = 0;
                double min = double.MaxValue;
                for (int i = 0; i < quadrangle.Length; i++)
                {
                    var distance = Math.Pow(point.X - quadrangle[i].X, 2) + Math.Pow(point.Y - quadrangle[i].Y, 2);
                    if (distance < min)
                    {
                        min = distance;
                        cornerIndex = i;
                    }
                }

                quadrangle[cornerIndex] = point;
                OnQuadrangleChanged(EventArgs.Empty);
            });
        }

        public CvPoint2D32f[] Quadrangle
        {
            get { return quadrangle; }
        }

        public event EventHandler QuadrangleChanged;

        protected virtual void OnQuadrangleChanged(EventArgs e)
        {
            var handler = QuadrangleChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void SetImage(IplImage image)
        {
            canvas = IplImageHelper.EnsureColorCopy(canvas, image);
            Core.cvLine(canvas, new CvPoint(quadrangle[0]), new CvPoint(quadrangle[1]), CvScalar.Rgb(255, 0, 0), 3, 8, 0);
            Core.cvLine(canvas, new CvPoint(quadrangle[1]), new CvPoint(quadrangle[2]), CvScalar.Rgb(255, 0, 0), 3, 8, 0);
            Core.cvLine(canvas, new CvPoint(quadrangle[2]), new CvPoint(quadrangle[3]), CvScalar.Rgb(255, 0, 0), 3, 8, 0);
            Core.cvLine(canvas, new CvPoint(quadrangle[3]), new CvPoint(quadrangle[0]), CvScalar.Rgb(255, 0, 0), 3, 8, 0);
            base.SetImage(canvas);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (canvas != null)
                    {
                        canvas.Close();
                        canvas = null;
                    }

                    disposed = true;
                }
            }

            base.Dispose(disposing);
        }
    }
}
