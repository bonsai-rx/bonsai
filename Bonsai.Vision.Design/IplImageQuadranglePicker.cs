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
        CvPoint2D32f[] quadrangle = new CvPoint2D32f[4];

        public IplImageQuadranglePicker()
        {
            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(pictureBox, "MouseMove").Select(e => e.EventArgs);
            var mouseDrag = from evt in mouseMove
                            where pictureBox.Image != null && evt.Button.HasFlag(MouseButtons.Left)
                            select new CvPoint2D32f(
                                evt.X * pictureBox.Image.Width / (float)pictureBox.Width,
                                evt.Y * pictureBox.Image.Height / (float)pictureBox.Height);

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

        public override IplImage Image
        {
            get { return base.Image; }
            set
            {
                var image = value.Clone();
                Core.cvLine(image, new CvPoint(quadrangle[0]), new CvPoint(quadrangle[1]), CvScalar.Rgb(255, 0, 0), 3, 8, 0);
                Core.cvLine(image, new CvPoint(quadrangle[1]), new CvPoint(quadrangle[2]), CvScalar.Rgb(255, 0, 0), 3, 8, 0);
                Core.cvLine(image, new CvPoint(quadrangle[2]), new CvPoint(quadrangle[3]), CvScalar.Rgb(255, 0, 0), 3, 8, 0);
                Core.cvLine(image, new CvPoint(quadrangle[3]), new CvPoint(quadrangle[0]), CvScalar.Rgb(255, 0, 0), 3, 8, 0);
                base.Image = image;
            }
        }
    }
}
