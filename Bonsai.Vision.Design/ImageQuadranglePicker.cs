using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using Bonsai.Design;
using OpenTK;

namespace Bonsai.Vision.Design
{
    class ImageQuadranglePicker : ImageBox
    {
        CommandExecutor commandExecutor = new CommandExecutor();
        Point2f[] quadrangle = new Point2f[4];
        const double ScaleIncrement = 0.1;
        const float LineWidth = 2;

        public ImageQuadranglePicker()
        {
            Canvas.KeyDown += Canvas_KeyDown;
            var lostFocus = Observable.FromEventPattern<EventArgs>(Canvas, "LostFocus").Select(e => e.EventArgs);
            var mouseUp = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseUp").Select(e => e.EventArgs);
            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseDown").Select(e => e.EventArgs);
            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseMove").Select(e => e.EventArgs);
            var mouseLeftButtonUp = mouseUp.Where(evt => evt.Button == MouseButtons.Left);
            var mouseLeftButtonDown = mouseDown.Where(evt => evt.Button == MouseButtons.Left);
            var mouseRightButtonDown = mouseDown.Where(evt => evt.Button == MouseButtons.Right &&
                                                              !MouseButtons.HasFlag(MouseButtons.Left));
            var mouseDrag = mouseLeftButtonDown.SelectMany(downEvt =>
            {
                var image = Image;
                var downPoint = ImagePoint(downEvt.X, downEvt.Y, image);
                var cornerIndex = CornerIndex(downPoint);
                commandExecutor.BeginCompositeCommand();
                commandExecutor.Execute(() => { }, UpdateQuadrangle);
                UpdatePoint(cornerIndex, quadrangle[cornerIndex]);
                return mouseMove
                    .TakeUntil(mouseLeftButtonUp.Merge(lostFocus))
                    .Select(evt => ImagePoint(evt.X, evt.Y, image))
                    .Do(point =>
                    {
                        quadrangle[cornerIndex] = point;
                        UpdateQuadrangle();
                    })
                    .TakeLast(1)
                    .Do(point =>
                    {
                        UpdatePoint(cornerIndex, point);
                        commandExecutor.Execute(UpdateQuadrangle, () => { });
                    })
                    .Finally(commandExecutor.EndCompositeCommand);
            });

            mouseDrag.Subscribe();
            mouseRightButtonDown.Subscribe(evt =>
            {
                var image = Image;
                if (image != null)
                {
                    InitializeQuadrangle(
                        new Point2f(0, 0),
                        new Point2f(0, image.Height),
                        new Point2f(image.Width, image.Height),
                        new Point2f(image.Width, 0));
                }
                else
                {
                    InitializeQuadrangle(
                        new Point2f(-1, -1),
                        new Point2f(-1, 1),
                        new Point2f(1, 1),
                        new Point2f(1, -1));
                }
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

        void InitializeQuadrangle(Point2f point0, Point2f point1, Point2f point2, Point2f point3)
        {
            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(() => { }, UpdateQuadrangle);
            UpdatePoint(0, point0);
            UpdatePoint(1, point1);
            UpdatePoint(2, point2);
            UpdatePoint(3, point3);
            commandExecutor.Execute(UpdateQuadrangle, () => { });
            commandExecutor.EndCompositeCommand();
        }

        int CornerIndex(Point2f point)
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

            return cornerIndex;
        }

        void UpdatePoint(int index, Point2f point)
        {
            var current = quadrangle[index];
            commandExecutor.Execute(
                () => quadrangle[index] = point,
                () => quadrangle[index] = current);
        }

        void UpdateQuadrangle()
        {
            OnQuadrangleChanged(EventArgs.Empty);
            Canvas.Invalidate();
        }

        Point2f ImagePoint(int x, int y, IplImage image)
        {
            if (image == null)
            {
                return new Point2f(
                    2 * x / (float)Canvas.Width - 1,
                    -2 * y / (float)Canvas.Height + 1);
            }
            else
            {
                return new Point2f(
                    x * image.Width / (float)Canvas.Width,
                    y * image.Height / (float)Canvas.Height);
            }
        }

        Vector2 NormalizePoint(Point2f point, IplImage image)
        {
            if (image == null) return new Vector2(point.X, point.Y);
            return new Vector2(
                (point.X * 2 / image.Width) - 1,
                -((point.Y * 2 / image.Height) - 1));
        }

        void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.PageUp) ImageScale += ScaleIncrement;
            if (e.KeyCode == Keys.PageDown) ImageScale -= ScaleIncrement;
            if (e.Control && e.KeyCode == Keys.Z) commandExecutor.Undo();
            if (e.Control && e.KeyCode == Keys.Y) commandExecutor.Redo();
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
