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
using Bonsai.Design;
using OpenTK;

namespace Bonsai.Vision.Design
{
    class ImageRectanglePicker : ImageBox
    {
        Rect previous;
        Rect rectangle;
        const float LineWidth = 2;
        const double ScaleIncrement = 0.1;
        CommandExecutor commandExecutor = new CommandExecutor();

        public ImageRectanglePicker()
        {
            Canvas.KeyDown += Canvas_KeyDown;
            var lostFocus = Observable.FromEventPattern<EventArgs>(Canvas, "LostFocus").Select(e => e.EventArgs);
            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseMove").Select(e => e.EventArgs);
            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseDown").Select(e => e.EventArgs);
            var mouseUp = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseUp").Select(e => e.EventArgs);
            var mouseLeftButtonUp = mouseUp.Where(evt => evt.Button == MouseButtons.Left);
            var mouseRightButtonUp = mouseUp.Where(evt => evt.Button == MouseButtons.Right &&
                                                          !MouseButtons.HasFlag(MouseButtons.Left));

            var mousePick = (from downEvt in mouseDown
                                .Where(evt => Image != null && evt.Button == MouseButtons.Left)
                                .Do(evt => previous = rectangle)
                             let origin = new OpenCV.Net.Point(downEvt.X, downEvt.Y)
                             let rect = CanvasRectangle(rectangle)
                             let intersect = IntersectRectangle(rect, origin.X, origin.Y)
                             select (from moveEvt in mouseMove.TakeUntil(mouseLeftButtonUp.Merge(lostFocus))
                                     let displacementX = moveEvt.X - origin.X
                                     let displacementY = moveEvt.Y - origin.Y
                                     select intersect
                                         ? new Rect(rect.X + displacementX, rect.Y + displacementY, rectangle.Width, rectangle.Height)
                                         : new Rect(origin.X, origin.Y, displacementX, displacementY))
                                     .Do(x =>
                                     {
                                         rectangle = NormalizedRectangle(x);
                                         if (intersect)
                                         {
                                             rectangle.Width = x.Width;
                                             rectangle.Height = x.Height;
                                         }
                                         Canvas.Invalidate();
                                     })
                                     .TakeLast(1)
                                     .Do(x =>
                                     {
                                         rectangle.X = Math.Min(rectangle.X, rectangle.X + rectangle.Width);
                                         rectangle.Y = Math.Min(rectangle.Y, rectangle.Y + rectangle.Height);
                                         rectangle.Width = Math.Abs(rectangle.Width);
                                         rectangle.Height = Math.Abs(rectangle.Height);
                                         rectangle = intersect ? FitRectangle(rectangle) : ClipRectangle(rectangle);
                                         UpdateRectangle(rectangle, previous);
                                     })).Switch();

            mouseRightButtonUp.Subscribe(evt => UpdateRectangle(default(Rect), rectangle));
            mousePick.Subscribe();
        }

        void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.PageUp) ImageScale += ScaleIncrement;
            if (e.KeyCode == Keys.PageDown) ImageScale -= ScaleIncrement;
            if (e.Control && e.KeyCode == Keys.Z) commandExecutor.Undo();
            if (e.Control && e.KeyCode == Keys.Y) commandExecutor.Redo();
        }

        void UpdateRectangle(Rect current, Rect previous)
        {
            commandExecutor.Execute(
            () =>
            {
                rectangle = current;
                OnRectangleChanged(EventArgs.Empty);
            },
            () =>
            {
                rectangle = previous;
                OnRectangleChanged(EventArgs.Empty);
            });
        }

        Rect ClipRectangle(Rect rect)
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

        Rect FitRectangle(Rect rect)
        {
            rect.X += rect.X < 0 ? -rect.X : -Math.Max(0, rect.X + rect.Width - Image.Width);
            rect.Y += rect.Y < 0 ? -rect.Y : -Math.Max(0, rect.Y + rect.Height - Image.Height);
            return rect;
        }

        static bool IntersectRectangle(Rect rect, int x, int y)
        {
            return x >= rect.X && y >= rect.Y &&
                x < (rect.X + rect.Width) &&
                y < (rect.Y + rect.Height);
        }

        Rect CanvasRectangle(Rect rect)
        {
            return new Rect(
                (int)(rect.X * Canvas.Width / (float)Image.Width),
                (int)(rect.Y * Canvas.Height / (float)Image.Height),
                (int)(rect.Width * Canvas.Width / (float)Image.Width),
                (int)(rect.Height * Canvas.Height / (float)Image.Height));
        }

        Rect NormalizedRectangle(Rect rect)
        {
            return new Rect(
                (int)(rect.X * Image.Width / (float)Canvas.Width),
                (int)(rect.Y * Image.Height / (float)Canvas.Height),
                (int)(rect.Width * Image.Width / (float)Canvas.Width),
                (int)(rect.Height * Image.Height / (float)Canvas.Height));
        }

        Box2 DrawingRectangle(Rect rect)
        {
            var image = Image;
            if (image == null) return new Box2(0, 0, 0, 0);
            return new Box2(
                (rect.X * 2 / (float)image.Width) - 1,
                -((rect.Y * 2 / (float)image.Height) - 1),
                ((rect.X + rect.Width) * 2 / (float)image.Width) - 1,
                -(((rect.Y + rect.Height) * 2 / (float)image.Height) - 1));
        }

        public Rect Rectangle
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
            GL.Begin(PrimitiveType.LineLoop);
            var drawingRectangle = DrawingRectangle(rectangle);
            GL.Vertex2(drawingRectangle.Left, drawingRectangle.Top);
            GL.Vertex2(drawingRectangle.Right, drawingRectangle.Top);
            GL.Vertex2(drawingRectangle.Right, drawingRectangle.Bottom);
            GL.Vertex2(drawingRectangle.Left, drawingRectangle.Bottom);
            GL.End();
        }
    }
}
