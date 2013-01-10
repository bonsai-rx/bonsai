using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Reactive;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK;

namespace Bonsai.Vision.Design
{
    class IplImageRectanglePicker : IplImageControl
    {
        CvRect rectangle;
        const float LineWidth = 2;

        public IplImageRectanglePicker()
        {
            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseMove").Select(e => e.EventArgs);
            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseDown").Select(e => e.EventArgs);
            var mouseUp = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseUp").Select(e => e.EventArgs);

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
                (int)(rect.X * Image.Width / (float)Canvas.Width),
                (int)(rect.Y * Image.Height / (float)Canvas.Height),
                (int)(rect.Width * Image.Width / (float)Canvas.Width),
                (int)(rect.Height * Image.Height / (float)Canvas.Height));
        }

        Box2 DrawingRectangle(CvRect rect)
        {
            var image = Image;
            if (image == null) return new Box2(0, 0, 0, 0);
            return new Box2(
                (rect.X * 2 / (float)image.Width) - 1,
                -((rect.Y * 2 / (float)image.Height) - 1),
                ((rect.X + rect.Width) * 2 / (float)image.Width) - 1,
                -(((rect.Y + rect.Height) * 2 / (float)image.Height) - 1));
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
            GL.Begin(BeginMode.LineLoop);
            var drawingRectangle = DrawingRectangle(rectangle);
            GL.Vertex2(drawingRectangle.Left, drawingRectangle.Top);
            GL.Vertex2(drawingRectangle.Right, drawingRectangle.Top);
            GL.Vertex2(drawingRectangle.Right, drawingRectangle.Bottom);
            GL.Vertex2(drawingRectangle.Left, drawingRectangle.Bottom);
            GL.End();
        }
    }
}
