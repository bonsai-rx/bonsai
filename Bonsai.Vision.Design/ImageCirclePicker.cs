using OpenCV.Net;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Vision.Design
{
    class ImageCirclePicker : ImageBox
    {
        const float LineWidth = 2;

        public ImageCirclePicker()
        {
            var lostFocus = Observable.FromEventPattern<EventArgs>(Canvas, "LostFocus").Select(e => e.EventArgs);
            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseMove").Select(e => e.EventArgs);
            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseDown").Select(e => e.EventArgs);
            var mouseUp = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseUp").Select(e => e.EventArgs);
            var mouseLeftButtonUp = mouseUp.Where(evt => evt.Button == MouseButtons.Left);
            var mousePick = (from downEvt in mouseDown.Where(evt => Image != null && evt.Button == MouseButtons.Left)
                             let center = NormalizedCenter(new Point2f(downEvt.X, downEvt.Y))
                             select (from moveEvt in mouseMove.TakeUntil(mouseLeftButtonUp.Merge(lostFocus))
                                     let target = NormalizedCenter(new Point2f(moveEvt.X, moveEvt.Y))
                                     let displacementX = target.X - center.X
                                     let displacementY = target.Y - center.Y
                                     select Math.Sqrt(displacementX * displacementX + displacementY * displacementY))
                                     .Do(radius =>
                                     {
                                         Center = center;
                                         Radius = radius;
                                         Canvas.Invalidate();
                                         OnCircleChanged(EventArgs.Empty);
                                     })).Switch();
            mousePick.Subscribe();
        }

        public Point2f Center { get; set; }

        public double Radius { get; set; }

        public event EventHandler CircleChanged;

        protected virtual void OnCircleChanged(EventArgs e)
        {
            var handler = CircleChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        Point2f NormalizedCenter(Point2f center)
        {
            return new Point2f(
                (int)(center.X * Image.Width / (float)Canvas.Width),
                (int)(center.Y * Image.Height / (float)Canvas.Height));
        }

        Vector2 DrawingCenter(Point2f center)
        {
            var image = Image;
            if (image == null) return Vector2.Zero;
            return new Vector2(
                (center.X * 2 / (float)image.Width) - 1,
                -((center.Y * 2 / (float)image.Height) - 1));
        }

        Vector2 DrawingRadius(double radius)
        {
            var image = Image;
            if (image == null) return Vector2.Zero;
            return new Vector2(
                (float)((radius * 2 / (float)image.Width)),
                (float)(-((radius * 2 / (float)image.Height))));
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.LineWidth(LineWidth);
            base.OnLoad(e);
        }

        protected override void OnRenderFrame(EventArgs e)
        {
            var center = DrawingCenter(Center);
            var radius = DrawingRadius(Radius);
            GL.Color3(Color.White);
            base.OnRenderFrame(e);

            GL.Color3(Color.Red);
            GL.Disable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.LineLoop);
            for (int i = 0; i < 360; i++)
            {
                var radians = MathHelper.DegreesToRadians(i);
                GL.Vertex2(Math.Cos(radians) * radius.X + center.X, Math.Sin(radians) * radius.Y + center.Y);
            }
            GL.End();
        }
    }
}
