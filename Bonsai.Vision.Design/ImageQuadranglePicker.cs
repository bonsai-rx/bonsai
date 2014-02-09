using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK;

namespace Bonsai.Vision.Design
{
    class ImageQuadranglePicker : ImageBox
    {
        Point2f[] quadrangle = new Point2f[4];
        const float LineWidth = 2;

        public ImageQuadranglePicker()
        {
            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseDown").Select(e => e.EventArgs);
            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseMove").Select(e => e.EventArgs);
            var mouseRightButtonDown = mouseDown.Where(evt => evt.Button == MouseButtons.Right);
            var mouseDrag = from evt in mouseMove
                            let image = Image
                            where image != null && evt.Button.HasFlag(MouseButtons.Left)
                            select new Point2f(
                                evt.X * image.Width / (float)Canvas.Width,
                                evt.Y * image.Height / (float)Canvas.Height);

            mouseRightButtonDown.Subscribe(evt =>
            {
                var image = Image;
                if (image != null)
                {
                    quadrangle[0] = new Point2f(0, 0);
                    quadrangle[1] = new Point2f(0, image.Height);
                    quadrangle[2] = new Point2f(image.Width, image.Height);
                    quadrangle[3] = new Point2f(image.Width, 0);
                    OnQuadrangleChanged(EventArgs.Empty);
                    Canvas.Invalidate();
                }
            });

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
                Canvas.Invalidate();
            });
        }

        public Point2f[] Quadrangle
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

        Vector2 NormalizePoint(Point2f point, IplImage image)
        {
            return new Vector2(
                (point.X * 2 / image.Width) - 1,
                -((point.Y * 2 / image.Height) - 1));
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.LineWidth(LineWidth);
            base.OnLoad(e);
        }

        protected override void OnRenderFrame(EventArgs e)
        {
            GL.Color3(Color.White);
            base.OnRenderFrame(e);

            var image = Image;
            if (image != null)
            {
                GL.Color3(Color.Red);
                GL.Disable(EnableCap.Texture2D);
                GL.Begin(PrimitiveType.LineLoop);
                for (int i = 0; i < quadrangle.Length; i++)
                {
                    GL.Vertex2(NormalizePoint(quadrangle[i], image));
                }
                GL.End();
            }
        }
    }
}
