using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZedGraph;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;

namespace Bonsai.Design.Visualizers
{
    public class GraphControl : ZedGraphControl
    {
        const int PenWidth = 3;
        const int MinimumDragDisplacement = 100;
        static readonly TimeSpan DragRefreshInterval = TimeSpan.FromMilliseconds(30);
        static readonly Pen RubberBandPen = new Pen(Color.FromArgb(51, 153, 255));
        static readonly Brush RubberBandBrush = new SolidBrush(Color.FromArgb(128, 170, 204, 238));

        int colorIndex;
        Rectangle rubberBand;
        Rectangle previousRectangle;
        PaneLayout? paneLayout;
        Size? paneLayoutSize;
        IDisposable rubberBandNotifications;
        static readonly Color[] BrightPastelPalette = new[]
        {
            ColorTranslator.FromHtml("#418CF0"),
            ColorTranslator.FromHtml("#FCB441"),
            ColorTranslator.FromHtml("#E0400A"),
            ColorTranslator.FromHtml("#056492"),
            ColorTranslator.FromHtml("#BFBFBF"),
            ColorTranslator.FromHtml("#1A3B69"),
            ColorTranslator.FromHtml("#FFE382"),
            ColorTranslator.FromHtml("#129CDD"),
            ColorTranslator.FromHtml("#CA6B4B"),
            ColorTranslator.FromHtml("#005CDB"),
            ColorTranslator.FromHtml("#F3D288"),
            ColorTranslator.FromHtml("#506381"),
            ColorTranslator.FromHtml("#F1B9A8"),
            ColorTranslator.FromHtml("#E0830A"),
            ColorTranslator.FromHtml("#7893BE")
        };

        public GraphControl()
        {
            SuspendLayout();
            AutoScaleAxis = true;
            Size = new Size(320, 240);
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            GraphPane.Title.IsVisible = false;
            GraphPane.Border.IsVisible = false;
            GraphPane.Chart.Border.IsVisible = false;
            GraphPane.YAxis.Scale.MaxGrace = 0.05;
            GraphPane.YAxis.Scale.MinGrace = 0;
            GraphPane.XAxis.Scale.MaxGrace = 0;
            GraphPane.XAxis.Scale.MinGrace = 0;
            GraphPane.YAxis.MajorGrid.IsZeroLine = false;
            GraphPane.YAxis.MinorTic.IsAllTics = false;
            GraphPane.XAxis.MinorTic.IsOpposite = false;
            GraphPane.YAxis.MajorTic.IsOpposite = false;
            GraphPane.XAxis.MajorTic.IsOpposite = false;
            GraphPane.YAxis.Title.IsVisible = false;
            GraphPane.XAxis.Title.IsVisible = false;
            GraphPane.YAxis.Scale.MagAuto = false;
            GraphPane.XAxis.Scale.MagAuto = false;
            MasterPane.Border.IsVisible = false;
            InitializeReactiveEvents();
            ResumeLayout(false);
        }

        [DefaultValue(true)]
        public bool AutoScaleAxis { get; set; }

        public new IObservable<MouseEventArgs> MouseDown { get; private set; }

        public new IObservable<MouseEventArgs> MouseUp { get; private set; }

        public new IObservable<MouseEventArgs> MouseMove { get; private set; }

        public Color GetNextColor()
        {
            var color = BrightPastelPalette[colorIndex];
            colorIndex = (colorIndex + 1) % BrightPastelPalette.Length;
            return color;
        }

        public static Color GetColor(int colorIndex)
        {
            if (colorIndex < 0)
            {
                throw new ArgumentOutOfRangeException("colorIndex");
            }

            return BrightPastelPalette[colorIndex % BrightPastelPalette.Length];
        }

        public void ResetColorCycle()
        {
            colorIndex = 0;
        }

        public void SetLayout(PaneLayout layout)
        {
            paneLayout = layout;
        }

        public void SetLayout(int rows, int columns)
        {
            paneLayoutSize = new Size(columns, rows);
        }

        bool IsMinimumDragDisplacement(int displacementX, int displacementY)
        {
            return displacementX * displacementX + displacementY * displacementY > MinimumDragDisplacement;
        }

        bool IsZoomButton(MouseButtons button)
        {
            var modifiers = Control.ModifierKeys;
            return button == ZoomButtons && modifiers == ZoomModifierKeys ||
                   button == ZoomButtons2 && modifiers == ZoomModifierKeys2;
        }

        void InitializeReactiveEvents()
        {
            var mouseDownEvent = Observable.Create<MouseEventArgs>(observer =>
            {
                ZedMouseEventHandler handler = (sender, e) =>
                {
                    observer.OnNext(e);
                    return IsZoomButton(e.Button);
                };
                MouseDownEvent += handler;
                return Disposable.Create(() => MouseDownEvent -= handler);
            });

            var mouseUpEvent = Observable.Create<MouseEventArgs>(observer =>
            {
                ZedMouseEventHandler handler = (sender, e) => { observer.OnNext(e); return false; };
                MouseUpEvent += handler;
                return Disposable.Create(() => MouseUpEvent -= handler);
            });

            var mouseMoveEvent = Observable.Create<MouseEventArgs>(observer =>
            {
                ZedMouseEventHandler handler = (sender, e) => { observer.OnNext(e); return false; };
                MouseMoveEvent += handler;
                return Disposable.Create(() => MouseMoveEvent -= handler);
            });

            var scheduler = new ControlScheduler(this);
            var selectionDrag = (from mouseDown in mouseDownEvent
                                 where (IsEnableHZoom || IsEnableVZoom) && IsZoomButton(mouseDown.Button)
                                 let selectedPane = MasterPane.FindChartRect(mouseDown.Location)
                                 where selectedPane != null
                                 select (from mouseMove in mouseMoveEvent.SkipWhile(move => IsMinimumDragDisplacement(move.X - mouseDown.X, move.Y - mouseDown.Y))
                                                                         .TakeUntil(mouseUpEvent)
                                                                         .Sample(DragRefreshInterval, scheduler)
                                         select (Rectangle?)GetNormalizedRectangle(selectedPane.Chart.Rect, mouseDown.Location, mouseMove.Location))
                                         .Concat(Observable.Return<Rectangle?>(null))
                                         .Select(rect => new { selectedPane, rect }))
                                         .Merge();
            rubberBandNotifications = selectionDrag.Subscribe(xs => ProcessRubberBand(xs.selectedPane, xs.rect));
            MouseDown = mouseDownEvent;
            MouseUp = mouseUpEvent;
            MouseMove = mouseMoveEvent;
        }

        protected static Rectangle GetNormalizedRectangle(RectangleF bounds, Point p1, Point p2)
        {
            p2.X = (int)Math.Max(bounds.Left, Math.Min(p2.X, bounds.Right));
            p2.Y = (int)Math.Max(bounds.Top, Math.Min(p2.Y, bounds.Bottom));
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p2.X - p1.X),
                Math.Abs(p2.Y - p1.Y));
        }

        void ProcessRubberBand(GraphPane selectedPane, Rectangle? rect)
        {
            if (!rect.HasValue &&
                !previousRectangle.IsEmpty &&
                IsMinimumDragDisplacement(previousRectangle.Width, previousRectangle.Height))
            {
                double minX, maxX;
                double minY, maxY;
                selectedPane.ZoomStack.Push(selectedPane, ZoomState.StateType.Zoom);
                selectedPane.ReverseTransform(previousRectangle.Location, out minX, out maxY);
                selectedPane.ReverseTransform(previousRectangle.Location + previousRectangle.Size, out maxX, out minY);
                if (IsEnableHZoom)
                {
                    selectedPane.XAxis.Scale.Min = minX;
                    selectedPane.XAxis.Scale.Max = maxX;
                }

                if (IsEnableVZoom)
                {
                    selectedPane.YAxis.Scale.Min = minY;
                    selectedPane.YAxis.Scale.Max = maxY;
                }

                selectedPane.AxisChange();
            }

            UpdateRubberBand(rect.GetValueOrDefault(), Rectangle.Truncate(selectedPane.Chart.Rect));
        }

        protected void UpdateRubberBand(Rectangle bandRect, Rectangle invalidateRect)
        {
            if (!Focused) Select();
            rubberBand = bandRect;
            invalidateRect.Inflate(PenWidth, PenWidth);
            Invalidate(invalidateRect);
            previousRectangle = bandRect;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (AutoScaleAxis) MasterPane.AxisChange(e.Graphics);
            if (paneLayout.HasValue)
            {
                MasterPane.SetLayout(e.Graphics, paneLayout.Value);
                paneLayout = null;
            }
            else if (paneLayoutSize.HasValue)
            {
                var layoutSize = paneLayoutSize.Value;
                MasterPane.SetLayout(e.Graphics, layoutSize.Height, layoutSize.Width);
                paneLayoutSize = null;
            }
            base.OnPaint(e);

            if (rubberBand.Width > 0 && rubberBand.Height > 0)
            {
                e.Graphics.FillRectangle(RubberBandBrush, rubberBand);
                e.Graphics.DrawRectangle(RubberBandPen, rubberBand);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (rubberBandNotifications != null)
            {
                rubberBandNotifications.Dispose();
                rubberBandNotifications = null;
            }
            base.OnHandleDestroyed(e);
        }
    }
}
