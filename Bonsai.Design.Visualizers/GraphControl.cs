using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Provides a dynamic graph control with a built-in color cycle palette.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphControl"/> class.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether to recalculate the axis range automatically
        /// when redrawing the graph.
        /// </summary>
        [DefaultValue(true)]
        public bool AutoScaleAxis { get; set; }

        /// <summary>
        /// Occurs when the mouse pointer is over the control and a mouse button is pressed.
        /// </summary>
        public new IObservable<MouseEventArgs> MouseDown { get; private set; }

        /// <summary>
        /// Occurs when the mouse pointer is over the control and a mouse button is released.
        /// </summary>
        public new IObservable<MouseEventArgs> MouseUp { get; private set; }

        /// <summary>
        /// Occurs when the mouse pointer is moved over the control.
        /// </summary>
        public new IObservable<MouseEventArgs> MouseMove { get; private set; }

        /// <summary>
        /// Returns the next color in the color cycle, and increments the color index.
        /// </summary>
        /// <returns>
        /// A <see cref="Color"/> value representing the next color in the color cycle.
        /// </returns>
        public Color GetNextColor()
        {
            var color = BrightPastelPalette[colorIndex];
            colorIndex = (colorIndex + 1) % BrightPastelPalette.Length;
            return color;
        }

        /// <summary>
        /// Returns the color in the color cycle at the specified index.
        /// </summary>
        /// <param name="colorIndex">The index of the color to retrieve.</param>
        /// <returns>
        /// A <see cref="Color"/> value representing the color at the specified
        /// index of the color cycle.
        /// </returns>
        public static Color GetColor(int colorIndex)
        {
            if (colorIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(colorIndex));
            }

            return BrightPastelPalette[colorIndex % BrightPastelPalette.Length];
        }

        /// <summary>
        /// Resets the color cycle to the first color in the palette.
        /// </summary>
        public void ResetColorCycle()
        {
            colorIndex = 0;
        }

        /// <summary>
        /// Sets the auto layout strategy for graphs with multiple panes.
        /// </summary>
        /// <param name="layout">
        /// Specifies the auto layout options for graphs with multiple panes.
        /// </param>
        public void SetLayout(PaneLayout layout)
        {
            paneLayout = layout;
        }

        /// <summary>
        /// Sets the number of rows and columns in the layout explicitly for
        /// graphs with multiple panes.
        /// </summary>
        /// <param name="rows">The number of rows in the pane layout.</param>
        /// <param name="columns">The number of columns in the pane layout.</param>
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

        /// <summary>
        /// Computes a rectangle defined by the specified points which is also contained inside
        /// the pane boundaries.
        /// </summary>
        /// <param name="bounds">The bounds of the pane on which to contain the rectangle.</param>
        /// <param name="p1">The first point defining the selected rectangle.</param>
        /// <param name="p2">The second point defining the selected rectangle.</param>
        /// <returns>
        /// A <see cref="Rectangle"/> which is contained inside the pane boundaries and is
        /// defined by the specified points.
        /// </returns>
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
                selectedPane.ZoomStack.Push(selectedPane, ZoomState.StateType.Zoom);
                selectedPane.ReverseTransform(previousRectangle.Location, out double minX, out double maxY);
                selectedPane.ReverseTransform(previousRectangle.Location + previousRectangle.Size, out double maxX, out double minY);
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

        /// <summary>
        /// Updates the state of the rubber band selection overlay.
        /// </summary>
        /// <param name="bandRect">
        /// The location and size of the rubber band selection.
        /// </param>
        /// <param name="invalidateRect">
        /// The region of the control that should be invalidated following the rubber band
        /// update operation. See the <see cref="Control.Invalidate(Rectangle)"/> method.
        /// </param>
        protected void UpdateRubberBand(Rectangle bandRect, Rectangle invalidateRect)
        {
            if (!Focused) Select();
            rubberBand = bandRect;
            invalidateRect.Inflate(PenWidth, PenWidth);
            Invalidate(invalidateRect);
            previousRectangle = bandRect;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
