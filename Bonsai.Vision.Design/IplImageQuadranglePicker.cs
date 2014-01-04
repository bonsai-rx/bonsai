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
    class IplImageQuadranglePicker : IplImageControl
    {
        Point2f[] quadrangle = new Point2f[4];
        const float LineWidth = 2;

        public IplImageQuadranglePicker()
        {
            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseMove").Select(e => e.EventArgs);
            var mouseDrag = from evt in mouseMove
                            where Image != null && evt.Button.HasFlag(MouseButtons.Left)
                            select new Point2f(
                                evt.X * Image.Width / (float)Canvas.Width,
                                evt.Y * Image.Height / (float)Canvas.Height);

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

        Vector2 NormalizePoint(Point2f point)
        {
            return new Vector2(
                (point.X * 2 / Image.Width) - 1,
                -((point.Y * 2 / Image.Height) - 1));
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

            GL.Color3(Color.Red);
            GL.Disable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.LineLoop);
            for (int i = 0; i < quadrangle.Length; i++)
            {
                GL.Vertex2(NormalizePoint(quadrangle[i]));
            }
            GL.End();
        }
    }
}
