using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Reactive.Linq;
using Bonsai.Editor.GraphModel;
using SvgNet.Interfaces;
using SvgNet;

namespace Bonsai.Editor.GraphView
{
    partial class GraphViewControl : UserControl, IGraphView
    {
        const float DefaultDpi = 96f;
        const float DefaultPenWidth = 2;
        const float DefaultNodeSize = 30;
        const float DefaultNodeAirspace = 80;
        const float DefaultPortSize = 6;
        const float DefaultLabelTextOffset = 5;
        static readonly float SpinnerRotation = (float)Math.Cos(Math.PI / 4);
        static readonly Color CursorLight = Color.White;
        static readonly Color CursorDark = Color.Black;
        static readonly Color NodeEdgeColor = Color.DarkGray;
        static readonly Color HighlightedColor = Color.FromArgb(217, 129, 119);
        static readonly Color HighlightedSelectionColor = Color.FromArgb(171, 39, 47);
        static readonly Color SelectionWhite = Color.FromArgb(250, 250, 250);
        static readonly Color UnfocusedSelectionWhite = Color.FromArgb(224, 224, 224);
        static readonly Color SelectionBlack = Color.FromArgb(51, 51, 51);
        static readonly Color UnfocusedSelectionBlack = Color.FromArgb(81, 81, 81);
        static readonly Color RubberBandPenColor = Color.FromArgb(51, 153, 255);
        static readonly Color RubberBandBrushColor = Color.FromArgb(128, 170, 204, 238);
        static readonly Color HotBrushColor = Color.FromArgb(128, 229, 243, 251);
        static readonly StringFormat TextFormat = new(StringFormatFlags.NoWrap);
        static readonly StringFormat CenteredTextFormat = new(StringFormat.GenericTypographic)
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        static readonly StringFormat VectorTextFormat = new(StringFormat.GenericTypographic)
        {
            FormatFlags = StringFormatFlags.NoWrap,
            Trimming = StringTrimming.Character
        };

        static readonly object EventItemDrag = new();
        static readonly object EventNodeMouseClick = new();
        static readonly object EventNodeMouseDoubleClick = new();
        static readonly object EventNodeMouseEnter = new();
        static readonly object EventNodeMouseLeave = new();
        static readonly object EventNodeMouseHover = new();
        static readonly object EventSelectedNodeChanged = new();

        float PenWidth;
        float NodeAirspace;
        float NodeSize;
        float PortSize;
        float HalfNodeSize;
        float HalfPortSize;
        float HalfPenWidth;
        float LabelTextOffset;
        SizeF VectorTextOffset;
        SizeF EntryOffset;
        SizeF ExitOffset;
        SizeF InputPortOffset;
        SizeF OutputPortOffset;
        SpinnerOffset[] SpinnerOffsets;
        Pen SolidPen;
        Pen DashPen;
        Font DefaultIconFont;

        float drawScale;
        bool ignoreMouseUp;
        bool mouseDownHandled;
        RectangleF rubberBand;
        RectangleF previousRectangle;
        Point? previousScrollOffset;
        GraphNode[] selectionBeforeDrag;
        readonly LayoutNodeCollection layoutNodes = new();
        readonly HashSet<GraphNode> selectedNodes = new();
        readonly SvgRendererState iconRendererState = new();
        IReadOnlyList<GraphNodeGrouping> nodes;
        GraphNode pivot;
        GraphNode hot;

        public GraphViewControl()
        {
            InitializeComponent();
            InitializeReactiveEvents();
            FocusedSelectionColor = Color.Black;
            UnfocusedSelectionColor = Color.Gray;
            ContrastSelectionColor = Color.White;
        }

        void InitializeReactiveEvents()
        {
            var mouseDownEvent = Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                handler => canvas.MouseDown += handler,
                handler => canvas.MouseDown -= handler)
                .Select(evt => evt.EventArgs);

            var mouseUpEvent = Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                handler => canvas.MouseUp += handler,
                handler => canvas.MouseUp -= handler)
                .Select(evt => EventArgs.Empty);

            var lostFocusEvent = Observable.FromEventPattern<EventHandler, EventArgs>(
                handler => LostFocus += handler,
                handler => LostFocus -= handler)
                .Select(evt => evt.EventArgs);

            var mouseMoveEvent = Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                handler => canvas.MouseMove += handler,
                handler => canvas.MouseMove -= handler)
                .Select(evt => evt.EventArgs);

            var mouseUpOrLostFocus = mouseUpEvent.Merge(lostFocusEvent);
            var selectionDrag = (from mouseDown in mouseDownEvent
                                 where hot == null
                                 let scrollOrigin = canvas.AutoScrollPosition
                                 select (from mouseMove in mouseMoveEvent.TakeUntil(mouseUpOrLostFocus)
                                         let origin = GetScrollablePoint(mouseDown.Location, scrollOrigin)
                                         let displacementX = mouseMove.X - origin.X
                                         let displacementY = mouseMove.Y - origin.Y
                                         where displacementX * displacementX + displacementY * displacementY > 16
                                         select (Rectangle?)GetNormalizedRectangle(origin, mouseMove.Location))
                                         .Concat(Observable.Return<Rectangle?>(null)))
                                         .SelectMany(selection => selection.Select((rect, i) =>
                                         {
                                             if (i == 0) selectionBeforeDrag = selectedNodes.ToArray();
                                             return rect;
                                         }).Finally(() => selectionBeforeDrag = null));

            var itemDrag = (from mouseDown in mouseDownEvent
                            let node = hot
                            where node != null
                            select (from mouseMove in mouseMoveEvent.TakeUntil(mouseUpOrLostFocus)
                                    let displacementX = mouseMove.X - mouseDown.X
                                    let displacementY = mouseMove.Y - mouseDown.Y
                                    where displacementX * displacementX + displacementY * displacementY > 16
                                    select new { node, mouseMove.Button })
                                    .Take(1)).Switch();

            mouseMoveEvent.Subscribe(mouseMove =>
            {
                var node = GetNodeAt(mouseMove.Location);
                if (node != hot)
                {
                    if (hot != null)
                    {
                        Invalidate(hot);
                        OnNodeMouseLeave(new GraphNodeMouseEventArgs(
                            hot,
                            mouseMove.Button,
                            mouseMove.Clicks,
                            mouseMove.X,
                            mouseMove.Y,
                            mouseMove.Delta));
                    }
                    if (node != null)
                    {
                        Invalidate(node);
                        OnNodeMouseEnter(new GraphNodeMouseEventArgs(
                            node,
                            mouseMove.Button,
                            mouseMove.Clicks,
                            mouseMove.X,
                            mouseMove.Y,
                            mouseMove.Delta));
                    }
                    hot = node;
                }
            });

            selectionDrag.Subscribe(ProcessRubberBand);
            itemDrag.Subscribe(drag => OnItemDrag(new ItemDragEventArgs(drag.Button, drag.node)));
        }

        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                base.BackColor = value;
                canvas.BackColor = value;
                UpdateCursorPen();
            }
        }

        public event ItemDragEventHandler ItemDrag
        {
            add { Events.AddHandler(EventItemDrag, value); }
            remove { Events.RemoveHandler(EventItemDrag, value); }
        }

        public event EventHandler<GraphNodeMouseEventArgs> NodeMouseClick
        {
            add { Events.AddHandler(EventNodeMouseClick, value); }
            remove { Events.RemoveHandler(EventNodeMouseClick, value); }
        }

        public event EventHandler<GraphNodeMouseEventArgs> NodeMouseDoubleClick
        {
            add { Events.AddHandler(EventNodeMouseDoubleClick, value); }
            remove { Events.RemoveHandler(EventNodeMouseDoubleClick, value); }
        }

        public event EventHandler<GraphNodeMouseEventArgs> NodeMouseEnter
        {
            add { Events.AddHandler(EventNodeMouseEnter, value); }
            remove { Events.RemoveHandler(EventNodeMouseEnter, value); }
        }

        public event EventHandler<GraphNodeMouseEventArgs> NodeMouseLeave
        {
            add { Events.AddHandler(EventNodeMouseLeave, value); }
            remove { Events.RemoveHandler(EventNodeMouseLeave, value); }
        }

        public event EventHandler<GraphNodeMouseHoverEventArgs> NodeMouseHover
        {
            add { Events.AddHandler(EventNodeMouseHover, value); }
            remove { Events.RemoveHandler(EventNodeMouseHover, value); }
        }

        public event EventHandler SelectedNodeChanged
        {
            add { Events.AddHandler(EventSelectedNodeChanged, value); }
            remove { Events.RemoveHandler(EventSelectedNodeChanged, value); }
        }

        public Color FocusedSelectionColor { get; set; }

        public Color ContrastSelectionColor { get; set; }

        public Color UnfocusedSelectionColor { get; set; }

        public Color CursorColor { get; set; }

        public SvgRendererFactory IconRenderer { get; set; }

        public Image GraphicsProvider { get; set; }

        public IReadOnlyList<GraphNodeGrouping> Nodes
        {
            get { return nodes; }
            set
            {
                pivot = null;
                nodes = value;
                hot = null;
                SelectedNode = null;
                UpdateModelLayout();
            }
        }

        public GraphNode CursorNode { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GraphNode SelectedNode
        {
            get { return selectedNodes.FirstOrDefault(); }
            set
            {
                var selectedNode = SelectedNode;
                if (selectedNode != value)
                {
                    UpdateSelection(() =>
                    {
                        selectedNodes.Clear();
                        if (value != null)
                        {
                            selectedNodes.Add(value);
                            SetCursor(value);
                            pivot = value;
                        }
                    });
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable<GraphNode> SelectedNodes
        {
            get { return selectedNodes; }
            set
            {
                if (selectedNodes != value)
                {
                    UpdateSelection(() =>
                    {
                        var cursorNode = CursorNode;
                        selectedNodes.Clear();
                        if (value != null)
                        {
                            foreach (var node in value)
                            {
                                selectedNodes.Add(node);
                                cursorNode = node;
                            }

                            if (cursorNode != CursorNode)
                            {
                                SetCursor(cursorNode);
                                pivot = cursorNode;
                            }
                        }
                    });
                }
            }
        }

        Graphics CreateVectorGraphics()
        {
            return GraphicsProvider != null ? Graphics.FromImage(GraphicsProvider) : CreateGraphics();
        }

        void UpdateSelection(Action update)
        {
            InvalidateSelection();
            update();
            InvalidateSelection();
            OnSelectedNodeChanged(EventArgs.Empty);
        }

        void InvalidateSelection()
        {
            foreach (var selectedNode in selectedNodes)
            {
                Invalidate(selectedNode);
            }
        }

        public override void Refresh()
        {
            using (var graphics = CreateGraphics())
            {
                foreach (var layout in layoutNodes)
                {
                    if (layout.Text != layout.Node.Text)
                    {
                        layout.SetNodeLabel(layout.Node.Text, Font, graphics);
                    }
                }
            }
            base.Refresh();
        }

        public void Invalidate(GraphNode node)
        {
            if (node != null && layoutNodes.Contains(node))
            {
                canvas.Invalidate(GetBoundingRectangle(node));
            }
        }

        void DisposeDrawResources()
        {
            if (SolidPen != null)
            {
                SolidPen.Dispose();
                DashPen.Dispose();
                SolidPen = DashPen = null;
            }
        }

        void UpdateCursorPen()
        {
            var brightness = canvas.BackColor.GetBrightness();
            if (brightness > 0.5)
            {
                CursorColor = CursorDark;
                FocusedSelectionColor = SelectionBlack;
                UnfocusedSelectionColor = UnfocusedSelectionWhite;
                ContrastSelectionColor = Color.Black;
            }
            else
            {
                CursorColor = CursorLight;
                FocusedSelectionColor = SelectionWhite;
                UnfocusedSelectionColor = UnfocusedSelectionBlack;
                ContrastSelectionColor = Color.White;
            }
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            DisposeDrawResources();
            using (var graphics = CreateVectorGraphics())
            {
                drawScale = graphics.DpiY / DefaultDpi;
                if (GraphicsProvider == null)
                {
                    drawScale *= Font.SizeInPoints / DefaultFont.SizeInPoints;
                }
            }

            iconRendererState.Scale = drawScale;
            PenWidth = DefaultPenWidth * drawScale;
            NodeAirspace = DefaultNodeAirspace * drawScale;
            NodeSize = DefaultNodeSize * drawScale;
            PortSize = DefaultPortSize * drawScale;
            HalfNodeSize = NodeSize / 2;
            HalfPortSize = PortSize / 2;
            HalfPenWidth = PenWidth / 2;
            LabelTextOffset = DefaultLabelTextOffset * drawScale;
            VectorTextOffset = new SizeF(0, 1.375f * drawScale);
            EntryOffset = new SizeF(-2 * PenWidth, HalfNodeSize);
            ExitOffset = new SizeF(NodeSize + 2 * PenWidth, HalfNodeSize);
            InputPortOffset = new SizeF(-HalfPortSize, EntryOffset.Height - HalfPortSize);
            OutputPortOffset = new SizeF(ExitOffset.Width - HalfPortSize, ExitOffset.Height - HalfPortSize);

            var spinnerTilt = (HalfPortSize - HalfPenWidth) * SpinnerRotation;
            SpinnerOffsets = new SpinnerOffset[]
            {
                new(x1: HalfPortSize, y1: HalfPenWidth, x2: HalfPortSize, y2: PortSize - HalfPenWidth),
                new(x1: HalfPortSize + spinnerTilt, y1: HalfPortSize - spinnerTilt,
                    x2: HalfPortSize - spinnerTilt, y2: HalfPortSize + spinnerTilt),
                new(x1: HalfPenWidth, y1: HalfPortSize, x2: PortSize - HalfPenWidth, y2: HalfPortSize),
                new(x1: HalfPortSize + spinnerTilt, y1: HalfPortSize + spinnerTilt,
                    x2: HalfPortSize - spinnerTilt, y2: HalfPortSize - spinnerTilt)
            };

            SolidPen = new Pen(NodeEdgeColor, drawScale);
            DashPen = new Pen(NodeEdgeColor, drawScale) { DashPattern = new[] { 4f, 2f } };
            DefaultIconFont = new Font(Font, FontStyle.Bold);
            UpdateCursorPen();
            UpdateModelLayout();
            base.ScaleControl(factor, specified);
        }

        Rectangle GetBoundingRectangle(GraphNode node)
        {
            var offset = new Point(-canvas.HorizontalScroll.Value, -canvas.VerticalScroll.Value);
            var nodeLayout = layoutNodes[node];
            var boundingRectangle = nodeLayout.BoundingRectangle;
            boundingRectangle.Offset(offset);

            var nodeText = node.Text;
            var labelRectangle = nodeLayout.LabelRectangle;
            if (nodeText != nodeLayout.Text)
            {
                using var graphics = CreateVectorGraphics();
                nodeLayout.SetNodeLabel(nodeText, Font, graphics);
                labelRectangle = RectangleF.Union(labelRectangle, nodeLayout.LabelRectangle);
            }

            labelRectangle.Offset(offset);
            return Rectangle.Truncate(RectangleF.Union(boundingRectangle, labelRectangle));
        }

        public void EnsureVisible(Point point)
        {
            var clientRectangle = canvas.ClientRectangle;
            if (!clientRectangle.Contains(point))
            {
                var scrollPosition = canvas.AutoScrollPosition;
                scrollPosition.X *= -1;
                scrollPosition.Y *= -1;
                scrollPosition.X += Math.Min(0, point.X - clientRectangle.Left);
                scrollPosition.X += Math.Max(0, point.X - clientRectangle.Right);
                scrollPosition.Y += Math.Min(0, point.Y - clientRectangle.Top);
                scrollPosition.Y += Math.Max(0, point.Y - clientRectangle.Bottom);
                canvas.AutoScrollPosition = scrollPosition;
            }
        }

        void EnsureVisible(GraphNode node)
        {
            var clientRectangle = canvas.ClientRectangle;
            var boundingRectangle = GetBoundingRectangle(node);
            if (!clientRectangle.Contains(boundingRectangle))
            {
                var scrollPosition = canvas.AutoScrollPosition;
                scrollPosition.X *= -1;
                scrollPosition.Y *= -1;
                scrollPosition.X += Math.Min(0, boundingRectangle.Left - clientRectangle.Left);
                scrollPosition.X += Math.Max(0, boundingRectangle.Right - clientRectangle.Right);
                scrollPosition.Y += Math.Min(0, boundingRectangle.Top - clientRectangle.Top);
                scrollPosition.Y += Math.Max(0, boundingRectangle.Bottom - clientRectangle.Bottom);
                canvas.AutoScrollPosition = scrollPosition;
            }
        }

        protected virtual void OnItemDrag(ItemDragEventArgs e)
        {
            (Events[EventItemDrag] as ItemDragEventHandler)?.Invoke(this, e);
        }

        protected virtual void OnNodeMouseClick(GraphNodeMouseEventArgs e)
        {
            (Events[EventNodeMouseClick] as EventHandler<GraphNodeMouseEventArgs>)?.Invoke(this, e);
        }

        protected virtual void OnNodeMouseDoubleClick(GraphNodeMouseEventArgs e)
        {
            (Events[EventNodeMouseDoubleClick] as EventHandler<GraphNodeMouseEventArgs>)?.Invoke(this, e);
        }

        protected virtual void OnNodeMouseEnter(GraphNodeMouseEventArgs e)
        {
            (Events[EventNodeMouseEnter] as EventHandler<GraphNodeMouseEventArgs>)?.Invoke(this, e);
        }

        protected virtual void OnNodeMouseLeave(GraphNodeMouseEventArgs e)
        {
            (Events[EventNodeMouseLeave] as EventHandler<GraphNodeMouseEventArgs>)?.Invoke(this, e);
        }

        protected virtual void OnNodeMouseHover(GraphNodeMouseHoverEventArgs e)
        {
            (Events[EventNodeMouseHover] as EventHandler<GraphNodeMouseHoverEventArgs>)?.Invoke(this, e);
        }

        protected virtual void OnSelectedNodeChanged(EventArgs e)
        {
            (Events[EventSelectedNodeChanged] as EventHandler)?.Invoke(this, e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            InvalidateSelection();
            Invalidate(CursorNode);
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            ignoreMouseUp = true;
            InvalidateSelection();
            Invalidate(CursorNode);
            base.OnLostFocus(e);
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            canvas.Invalidate(e.InvalidRect);
            base.OnInvalidated(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var shift = keyData.HasFlag(Keys.Shift);
            if (shift) keyData &= ~Keys.Shift;
            var control = keyData.HasFlag(Keys.Control);
            if (control) keyData &= ~Keys.Control;

            if (keyData == Keys.Space && CursorNode != null)
            {
                if (selectedNodes.Contains(CursorNode))
                {
                    if (control) ClearNode(CursorNode);
                    else ClearSelection();
                }
                else SelectNode(CursorNode, control);
            }

            var stepCursor = false;
            switch (keyData)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    stepCursor = true;
                    break;
            }

            if (CursorNode != null && stepCursor)
            {
                StepCursor(keyData);
                if (shift)
                {
                    SelectRange(CursorNode, control);
                }
                else
                {
                    pivot = CursorNode;
                    if (!control) SelectNode(CursorNode, false);
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        IEnumerable<GraphNode> GetRubberBandSelection(Rectangle rect)
        {
            var offset = new Size(canvas.HorizontalScroll.Value, canvas.VerticalScroll.Value);
            var selectionRect = new Rectangle(Point.Add(rect.Location, offset), rect.Size);
            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value != null)
                {
                    var selected = CircleIntersect(layout.Center, HalfNodeSize, selectionRect);
                    if (selected)
                    {
                        yield return layout.Node;
                    }
                }
            }
        }

        void ProcessRubberBand(Rectangle? rect)
        {
            if (!Focused) Select();
            var selectionRect = RectangleF.Empty;
            if (rect.HasValue)
            {
                var selection = new HashSet<GraphNode>(GetRubberBandSelection(rect.Value));
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    selection.SymmetricExceptWith(selectionBeforeDrag);
                }
                else if (ModifierKeys.HasFlag(Keys.Shift))
                {
                    selection.UnionWith(selectionBeforeDrag);
                }

                var selectionChanged = !selection.SetEquals(selectedNodes);
                if (selectionChanged)
                {
                    foreach (var node in selectedNodes) selectionRect = RectangleF.Union(selectionRect, GetBoundingRectangle(node));
                    selectedNodes.Clear();
                    foreach (var node in selection)
                    {
                        selectedNodes.Add(node);
                        selectionRect = RectangleF.Union(selectionRect, GetBoundingRectangle(node));
                    }
                    OnSelectedNodeChanged(EventArgs.Empty);
                }
            }

            rubberBand = rect.GetValueOrDefault();
            var invalidateRect = rubberBand;
            invalidateRect.Inflate(PenWidth, PenWidth);
            invalidateRect = (selectionRect.Width > 0 || selectionRect.Height > 0) ? RectangleF.Union(invalidateRect, selectionRect) : invalidateRect;
            if (previousScrollOffset.HasValue)
            {
                var scrollPosition = canvas.AutoScrollPosition;
                var previousScroll = previousScrollOffset.Value;
                scrollPosition.X -= previousScroll.X;
                scrollPosition.Y -= previousScroll.Y;
                previousRectangle.Offset(scrollPosition);
            }

            invalidateRect = RectangleF.Union(invalidateRect, previousRectangle);
            canvas.Invalidate(Rectangle.Truncate(invalidateRect));
            previousRectangle = invalidateRect;
            previousScrollOffset = rect.HasValue ? canvas.AutoScrollPosition : (Point?)null;
        }

        Point GetScrollablePoint(Point point, Point scrollOrigin)
        {
            var scrollPosition = canvas.AutoScrollPosition;
            scrollPosition.X -= scrollOrigin.X;
            scrollPosition.Y -= scrollOrigin.Y;
            point.Offset(scrollPosition);
            return point;
        }

        static Rectangle GetNormalizedRectangle(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p2.X - p1.X),
                Math.Abs(p2.Y - p1.Y));
        }

        static float SquaredDistance(ref PointF point, ref PointF center)
        {
            var xdiff = point.X - center.X;
            var ydiff = point.Y - center.Y;
            return xdiff * xdiff + ydiff * ydiff;
        }

        static bool CircleIntersect(PointF center, float radius, PointF point)
        {
            return SquaredDistance(ref point, ref center) <= radius * radius;
        }

        static bool CircleIntersect(PointF center, float radius, RectangleF rect)
        {
            float closestX = Math.Max(rect.Left, Math.Min(center.X, rect.Right));
            float closestY = Math.Max(rect.Top, Math.Min(center.Y, rect.Bottom));
            float distanceX = center.X - closestX;
            float distanceY = center.Y - closestY;
            float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
            return distanceSquared < (radius * radius);
        }

        public PointF GetNodeLocation(GraphNode node)
        {
            return layoutNodes[node].Location;
        }

        GraphNode GetClosestNode(Point point)
        {
            point.X += canvas.HorizontalScroll.Value;
            point.Y += canvas.VerticalScroll.Value;

            if (layoutNodes.Count > 0)
            {
                var rightMost = layoutNodes[0];
                point.X = Math.Min(point.X, (int)rightMost.Location.X);
            }

            var bottomY = 0f;
            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value == null) continue;

                var boundingRectangle = new RectangleF(
                    layout.BoundingRectangle.Location,
                    new SizeF(NodeAirspace, NodeAirspace));
                bottomY = Math.Max(bottomY, boundingRectangle.Bottom);
                if (boundingRectangle.Contains(point))
                {
                    return layout.Node;
                }
            }

            return point.Y >= bottomY ? GetLastNode() : null;
        }

        public GraphNode GetNodeAt(Point point)
        {
            point.X += canvas.HorizontalScroll.Value;
            point.Y += canvas.VerticalScroll.Value;

            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value == null) continue;

                if (CircleIntersect(layout.Center, HalfNodeSize, point))
                {
                    return layout.Node;
                }
            }

            return null;
        }

        GraphNode GetLastNode()
        {
            if (nodes.Count > 0)
            {
                var layer = nodes[0];
                return layer[layer.Count - 1];
            }

            return null;
        }

        IEnumerable<GraphNode> GetPredecessors(GraphNode node)
        {
            return from layer in nodes
                   where layer.Key > node.Layer
                   from predecessor in layer
                   where predecessor.Successors.Any(edge => edge.Node == node)
                   select predecessor;
        }

        GraphNode ResolveDummyPredecessor(GraphNode source)
        {
            while (source.Value == null)
            {
                source = GetPredecessors(source).First();
            }

            return source;
        }

        static GraphNode ResolveDummySuccessor(GraphNode source)
        {
            while (source.Value == null)
            {
                source = source.Successors.First().Node;
            }

            return source;
        }

        static GraphNode GetDefaultSuccessor(GraphNode node)
        {
            return (from successor in node.Successors
                    orderby successor.Node.LayerIndex ascending
                    select ResolveDummySuccessor(successor.Node))
                   .FirstOrDefault();
        }

        GraphNode GetDefaultPredecessor(GraphNode node)
        {
            return (from predecessor in GetPredecessors(node)
                    orderby predecessor.LayerIndex ascending
                    select ResolveDummyPredecessor(predecessor))
                   .FirstOrDefault();
        }

        IEnumerable<GraphNode> GetSiblings(GraphNode node)
        {
            return (from predecessor in GetPredecessors(node)
                    orderby predecessor.LayerIndex ascending
                    let predecessorNode = ResolveDummyPredecessor(predecessor)
                    from successor in predecessorNode.Successors
                    orderby successor.Node.LayerIndex ascending
                    select ResolveDummySuccessor(successor.Node))
                   .Distinct();
        }

        void StepCursor(Keys step)
        {
            if (CursorNode == null) return;
            var layer = CursorNode.Layer;
            var layerIndex = CursorNode.LayerIndex;
            if (step == Keys.Right) layer--;
            if (step == Keys.Left) layer++;
            if (step == Keys.Up) layerIndex--;
            if (step == Keys.Down) layerIndex++;

            GraphNode selection = null;
            if (selection == null)
            {
                if (layer != CursorNode.Layer)
                {
                    selection = (from layout in layoutNodes
                                 where layout.Node.Value != null &&
                                       layout.Node.LayerIndex == layerIndex && (layer > CursorNode.Layer
                                     ? layout.Node.Layer >= layer
                                     : layout.Node.Layer <= layer)
                                 orderby Math.Abs(layout.Node.Layer - layer) ascending
                                 select layout.Node)
                                .FirstOrDefault();
                }
                else
                {
                    selection = (from layout in layoutNodes
                                 where layout.Node.Value != null &&
                                       layout.Node.Layer == layer && (layerIndex > CursorNode.LayerIndex
                                     ? layout.Node.LayerIndex >= layerIndex
                                     : layout.Node.LayerIndex <= layerIndex)
                                 orderby Math.Abs(layout.Node.LayerIndex - layerIndex) ascending
                                 select layout.Node)
                                .FirstOrDefault();
                }
            }

            if (selection == null)
            {
                if (step == Keys.Right)
                {
                    selection = GetDefaultSuccessor(CursorNode);
                }

                if (step == Keys.Left)
                {
                    selection = GetDefaultPredecessor(CursorNode);
                }

                if (step == Keys.Down)
                {
                    selection = GetSiblings(CursorNode).SkipWhile(node => node != CursorNode).Skip(1).FirstOrDefault();
                }

                if (step == Keys.Up)
                {
                    selection = GetSiblings(CursorNode).TakeWhile(node => node != CursorNode).LastOrDefault();
                }
            }

            if (selection != null)
            {
                SetCursor(selection);
            }
        }

        private void UpdateModelLayout()
        {
            layoutNodes.Clear();
            var model = Nodes;
            var size = SizeF.Empty;
            var cursorIndex = CursorNode?.Index;
            CursorNode = null;
            if (model != null)
            {
                using (var graphics = CreateVectorGraphics())
                {
                    var layerCount = model.Count;
                    foreach (var layer in model)
                    {
                        var maxRow = 0;
                        var column = layerCount - layer.Key - 1;
                        foreach (var node in layer)
                        {
                            if (node.Index == cursorIndex)
                            {
                                CursorNode = node;
                            }

                            var row = node.LayerIndex;
                            var location = new PointF(column * NodeAirspace + 2 * PenWidth, row * NodeAirspace + 2 * PenWidth);
                            var layout = new LayoutNode(this, node, location);
                            layout.SetNodeLabel(node.Text, Font, graphics);
                            layoutNodes.Add(layout);
                            maxRow = Math.Max(row, maxRow);
                        }

                        var rowHeight = (maxRow + 1) * NodeAirspace;
                        size.Height = Math.Max(rowHeight, size.Height);
                    }

                    size.Width = layerCount * NodeAirspace;
                }

                CursorNode ??= GetLastNode();
                pivot = CursorNode;
            }

            canvas.AutoScrollMinSize = Size.Truncate(size);
        }

        private static IEnumerable<GraphNode> GetAllPaths(GraphNode from, GraphNode to)
        {
            if (from == to) yield return from;
            else foreach (var successor in from.Successors)
            {
                var inPath = false;
                var successorPaths = GetAllPaths(successor.Node, to);
                foreach (var node in successorPaths)
                {
                    inPath = true;
                    yield return node;
                }

                if (inPath) yield return from;
            }
        }

        private static IEnumerable<TSource> ConcatEmpty<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            var any = false;
            foreach (var xs in first)
            {
                any = true;
                yield return xs;
            }

            if (!any) foreach (var xs in second) yield return xs;
        }

        static private IEnumerable<GraphNode> GetSelectionRange(GraphNode from, GraphNode to)
        {
            return ConcatEmpty(
                ConcatEmpty(GetAllPaths(from, to), GetAllPaths(to, from)),
                new[] { from, to }).Where(node => node.Value != null);
        }

        private void SelectRange(GraphNode node, bool clearSelection)
        {
            var path = GetSelectionRange(pivot, node).ToArray();
            if (path.Length == 0) return;

            if (!clearSelection)
            {
                UpdateSelection(() => selectedNodes.UnionWith(path));
            }
            else
            {
                UpdateSelection(() =>
                {
                    selectedNodes.Clear();
                    foreach (var element in path) selectedNodes.Add(element);
                });
            }
        }

        private void SelectNode(GraphNode node, bool append)
        {
            UpdateSelection(() =>
            {
                if (!append) selectedNodes.Clear();
                selectedNodes.Add(node);
            });
        }

        private void ClearNode(GraphNode node)
        {
            UpdateSelection(() => selectedNodes.Remove(node));
        }

        private void ClearSelection()
        {
            UpdateSelection(() => selectedNodes.Clear());
        }

        private void SetCursor(GraphNode node)
        {
            Invalidate(CursorNode);
            CursorNode = node;
            Invalidate(node);
            EnsureVisible(CursorNode);
        }

        private static IEnumerable<string> WordWrap(Graphics graphics, string text, Font font, float lineWidth)
        {
            var trimStart = true;
            var words = text.SplitOnWordBoundaries();
            var lineBreak = words.Length <= 1 ? 0 : 2;
            var result = new StringBuilder(text.Length);
            foreach (var word in words)
            {
                if (lineBreak > 0 && result.Length > 0)
                {
                    var line = result.ToString();
                    var wordSize = graphics.MeasureString(word, font);
                    var lineSize = graphics.MeasureString(line, font);
                    if ((wordSize.Width + lineSize.Width) > lineWidth)
                    {
                        yield return line;
                        trimStart = true;
                        result.Clear();
                        lineBreak--;
                    }
                }

                foreach (var c in word)
                {
                    if (trimStart)
                    {
                        if (char.IsWhiteSpace(c)) continue;
                        else trimStart = false;
                    }
                    result.Append(c);
                }
            }

            if (result.Length > 0)
            {
                yield return result.ToString();
            }
        }

        public SizeF GetLayoutSize()
        {
            var boundingRect = RectangleF.Empty;
            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value != null)
                {
                    var labelRect = layout.LabelRectangle;
                    var layoutBounds = RectangleF.Union(layout.BoundingRectangle, labelRect);
                    boundingRect = RectangleF.Union(boundingRect, layoutBounds);
                }
                else if (layout.Node.Tag != null)
                {
                    boundingRect = RectangleF.Union(boundingRect, layout.BoundingRectangle);
                }
            }

            return boundingRect.Size;
        }

        private void DrawNode(
            IGraphics graphics,
            LayoutNode layout,
            Size offset,
            SolidBrush selection,
            SolidBrush fill,
            Pen stroke,
            Color currentColor,
            Color? cursor = null,
            bool hot = false,
            Font vectorFont = null)
        {
            var nodeRectangle = new RectangleF(
                layout.Location.X + offset.Width,
                layout.Location.Y + offset.Height,
                NodeSize, NodeSize);

            SvgRenderer renderer;
            iconRendererState.Fill = fill;
            iconRendererState.Stroke = stroke;
            iconRendererState.CurrentColor = currentColor;
            iconRendererState.Translation = nodeRectangle.Location;
            if (layout.Node.IsDisabled)
            {
                graphics.FillEllipse(Brushes.DarkGray, nodeRectangle);
            }
            else if (IconRenderer != null &&
                    (renderer = IconRenderer.GetIconRenderer(layout.Node.Category)) != null)
            {
                renderer(iconRendererState, graphics);
            }
            else graphics.FillEllipse(iconRendererState.FillStyle(layout.Node.FillColor), nodeRectangle);

            var nestedCategory = layout.Node.NestedCategory;
            if (nestedCategory != null)
            {
                if (IconRenderer != null &&
                   (renderer = IconRenderer.GetIconRenderer(nestedCategory.Value)) != null)
                {
                    renderer(iconRendererState, graphics);
                }
                else graphics.DrawEllipse(iconRendererState.StrokeStyle(NodeEdgeColor, PenWidth), nodeRectangle);
            }

            if (selection != null)
            {
                graphics.FillEllipse(selection, nodeRectangle);
            }

            if (IconRenderer != null && layout.Node.Icon != null &&
               (renderer = IconRenderer.GetIconRenderer(layout.Node)) != null)
            {
                renderer(iconRendererState, graphics);
            }
            else if (vectorFont != null)
            {
                graphics.DrawString(
                    layout.Label.Substring(0, 1),
                    vectorFont,
                    iconRendererState.FillStyle(),
                    new RectangleF(
                        nodeRectangle.Location,
                        SizeF.Add(nodeRectangle.Size, VectorTextOffset)),
                    CenteredTextFormat);
            }
            else
            {
                graphics.DrawString(
                    layout.Label.Substring(0, 1),
                    DefaultIconFont,
                    iconRendererState.FillStyle(),
                    nodeRectangle, CenteredTextFormat);
            }

            if (layout.Node.ModifierBrush != null)
            {
                graphics.FillEllipse(layout.Node.ModifierBrush, nodeRectangle);
            }

            if (cursor.HasValue && ContainsFocus)
            {
                graphics.DrawEllipse(iconRendererState.StrokeStyle(cursor.Value, PenWidth), nodeRectangle);
            }

            if (hot) graphics.FillEllipse(iconRendererState.FillStyle(HotBrushColor), nodeRectangle);
            if (layout.Node.ArgumentCount < layout.Node.ArgumentRange.UpperBound)
            {
                var inputPortRectangle = layout.InputPortRectangle;
                var argumentRequired = layout.Node.ArgumentCount < layout.Node.ArgumentRange.LowerBound;
                var inputPortBrush = argumentRequired ? Brushes.Black : Brushes.White;
                inputPortRectangle.Offset(offset.Width, offset.Height);
                graphics.DrawEllipse(iconRendererState.StrokeStyle(NodeEdgeColor, PenWidth), inputPortRectangle);
                graphics.FillEllipse(inputPortBrush, inputPortRectangle);
            }

            if (layout.Node.Status != null)
            {
                var nodeStatus = layout.Node.Status.GetValueOrDefault();
                var outputPortRectangle = layout.OutputPortRectangle;
                var outputPortBrush = nodeStatus switch
                {
                    Diagnostics.WorkflowElementStatus.Completed => Brushes.LimeGreen,
                    Diagnostics.WorkflowElementStatus.Error => Brushes.Red,
                    Diagnostics.WorkflowElementStatus.Canceled => Brushes.Orange,
                    _ => Brushes.White
                };
                outputPortRectangle.Offset(offset.Width, offset.Height);

                var active = nodeStatus == Diagnostics.WorkflowElementStatus.Active ||
                             nodeStatus == Diagnostics.WorkflowElementStatus.Notifying;
                var terminated = nodeStatus == Diagnostics.WorkflowElementStatus.Completed ||
                                 nodeStatus == Diagnostics.WorkflowElementStatus.Error ||
                                 nodeStatus == Diagnostics.WorkflowElementStatus.Canceled;
                var edgeColor = active || terminated ? CursorColor : NodeEdgeColor;

                graphics.DrawEllipse(iconRendererState.StrokeStyle(edgeColor, PenWidth), outputPortRectangle);
                graphics.FillEllipse(outputPortBrush, outputPortRectangle);
                if (active && layout.Node.NotifyingCounter >= 0)
                {
                    var spinnerIndex = layout.Node.NotifyingCounter % SpinnerOffsets.Length;
                    var notify1 = outputPortRectangle.Location + SpinnerOffsets[spinnerIndex].Offset1;
                    var notify2 = outputPortRectangle.Location + SpinnerOffsets[spinnerIndex].Offset2;
                    graphics.DrawLine(iconRendererState.StrokeStyle(CursorDark, PenWidth), notify1, notify2);
                }
            }
        }

        private void DrawDummyNode(IGraphics graphics, LayoutNode layout, Size offset)
        {
            if (layout.Node.Tag != null)
            {
                graphics.DrawLine(
                    layout.Node.IsBuildDependency ? DashPen : SolidPen,
                    PointF.Add(layout.EntryPoint, offset),
                    PointF.Add(layout.ExitPoint, offset));
            }
        }

        private void DrawEdges(IGraphics graphics, Size offset)
        {
            foreach (var layout in layoutNodes)
            {
                foreach (var successor in layout.Node.Successors)
                {
                    var successorLayout = layoutNodes[successor.Node];
                    graphics.DrawLine(
                        layout.Node.IsBuildDependency ? DashPen : SolidPen,
                        PointF.Add(layout.ExitPoint, offset),
                        PointF.Add(successorLayout.EntryPoint, offset));
                }
            }
        }

        public void DrawGraphics(IGraphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.White);

            var font = Font;
            using var measureGraphics = CreateVectorGraphics();
            using var fill = new SolidBrush(Color.White);
            using var stroke = new Pen(NodeEdgeColor, PenWidth);
            DrawEdges(graphics, Size.Empty);
            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value != null)
                {
                    DrawNode(graphics, layout, Size.Empty, null, fill, stroke, Color.White, vectorFont: font);
                    var labelRect = layout.LabelRectangle;
                    foreach (var line in layout.Label.Split(Environment.NewLine.ToArray(),
                                                            StringSplitOptions.RemoveEmptyEntries))
                    {
                        var size = measureGraphics.MeasureString(
                            line, font,
                            labelRect.Size,
                            VectorTextFormat,
                            out int charactersFitted, out _);
                        var lineLabel = line.Length > charactersFitted ? line.Substring(0, charactersFitted) : line;
                        graphics.DrawString(lineLabel, font, Brushes.Black, labelRect);
                        labelRect.Y += size.Height;
                    }
                }
                else DrawDummyNode(graphics, layout, Size.Empty);
            }
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            var graphics = new GdiGraphics(e.Graphics);
            var offset = new Size(-canvas.HorizontalScroll.Value, -canvas.VerticalScroll.Value);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var selection = new SolidBrush(Color.Empty))
            using (var fill = new SolidBrush(Color.Empty))
            using (var stroke = new Pen(NodeEdgeColor, PenWidth))
            {
                DrawEdges(graphics, offset);
                foreach (var layout in layoutNodes)
                {
                    if (layout.Node.Value != null)
                    {
                        Color currentColor;
                        Color selectionColor;
                        var cursorColor = CursorNode == layout.Node ? CursorColor : default(Color?);
                        var selected = selectedNodes.Contains(layout.Node);
                        if (layout.Node.Highlight)
                        {
                            selectionColor = selected ? HighlightedSelectionColor : HighlightedColor;
                            currentColor = Color.White;
                        }
                        else
                        {
                            selectionColor = selected ? (Focused ? FocusedSelectionColor : UnfocusedSelectionColor) : Color.Empty;
                            currentColor = selected ? Color.DarkGray : Color.White;
                        }

                        SolidBrush activeSelection = null;
                        if (!selectionColor.IsEmpty)
                        {
                            activeSelection = selection;
                            activeSelection.Color = selectionColor;
                        }

                        DrawNode(graphics, layout, offset, activeSelection, fill, stroke, currentColor, cursorColor, layout.Node == hot);
                        selection.Color = ContrastSelectionColor;
                        var labelRect = layout.LabelRectangle;
                        labelRect.Location += offset;
                        graphics.DrawString(layout.Label, Font, selection, labelRect, TextFormat);
                    }
                    else DrawDummyNode(graphics, layout, offset);
                }
            }

            if (rubberBand.Width > 0 && rubberBand.Height > 0)
            {
                using var rubberBandPen = new Pen(RubberBandPenColor);
                using var rubberBandBrush = new SolidBrush(RubberBandBrushColor);
                e.Graphics.FillRectangle(rubberBandBrush, rubberBand);
                e.Graphics.DrawRectangle(rubberBandPen, rubberBand.X, rubberBand.Y, rubberBand.Width, rubberBand.Height);
            }
        }

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            ignoreMouseUp = false;
            if (!Focused) Select();
            if (hot != null)
            {
                SetCursor(hot);
                if (ModifierKeys.HasFlag(Keys.Shift))
                {
                    mouseDownHandled = true;
                    SelectRange(hot, ModifierKeys.HasFlag(Keys.Control));
                }
                else
                {
                    pivot = CursorNode;
                    if (!selectedNodes.Contains(hot))
                    {
                        mouseDownHandled = true;
                        SelectNode(hot, ModifierKeys.HasFlag(Keys.Control));
                    }
                }
            }

            OnMouseDown(e);
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (selectionBeforeDrag != null || ignoreMouseUp) return;
            if (hot != null)
            {
                if (e.Button == MouseButtons.Right && selectedNodes.Contains(hot)) return;
                if (!mouseDownHandled)
                {
                    if (ModifierKeys.HasFlag(Keys.Control)) ClearNode(hot);
                    else SelectNode(hot, false);
                }
            }
            else
            {
                var cursorNode = GetClosestNode(e.Location);
                if (cursorNode != null) SetCursor(cursorNode);
                if (ModifierKeys == Keys.None)
                {
                    ClearSelection();
                }
            }

            mouseDownHandled = false;
            OnMouseUp(e);
        }

        private void canvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (hot != null)
            {
                OnNodeMouseClick(new GraphNodeMouseEventArgs(hot, e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }
        }

        private void canvas_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (hot != null)
            {
                OnNodeMouseDoubleClick(new GraphNodeMouseEventArgs(hot, e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }
        }

        private void canvas_MouseHover(object sender, EventArgs e)
        {
            if (hot != null)
            {
                OnNodeMouseHover(new GraphNodeMouseHoverEventArgs(hot));
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (canvas.Capture)
            {
                EnsureVisible(e.Location);
            }
        }

        class LayoutNodeCollection : KeyedCollection<GraphNode, LayoutNode>
        {
            protected override GraphNode GetKeyForItem(LayoutNode item)
            {
                return item.Node;
            }
        }

        class LayoutNode
        {
            public LayoutNode(GraphViewControl view, GraphNode node, PointF location)
            {
                View = view;
                Node = node;
                Location = location;
            }

            private GraphViewControl View { get; set; }

            public GraphNode Node { get; private set; }

            public PointF Location { get; private set; }

            public string Text { get; private set; }

            public string Label { get; private set; }

            public RectangleF LabelRectangle { get; private set; }

            public PointF Center
            {
                get { return PointF.Add(Location, new SizeF(View.HalfNodeSize, View.HalfNodeSize)); }
            }

            public PointF EntryPoint
            {
                get { return PointF.Add(Location, View.EntryOffset); }
            }

            public PointF ExitPoint
            {
                get { return PointF.Add(Location, View.ExitOffset); }
            }

            public PointF InputPortLocation
            {
                get { return PointF.Add(Location, View.InputPortOffset); }
            }

            public PointF OutputPortLocation
            {
                get { return PointF.Add(Location, View.OutputPortOffset); }
            }

            public RectangleF BoundingRectangle
            {
                get
                {
                    return new RectangleF(
                        InputPortLocation.X - View.PenWidth, Location.Y - View.PenWidth,
                        View.HalfPortSize + View.NodeSize + 2 * View.PenWidth, View.NodeSize + 2 * View.PenWidth);
                }
            }

            public RectangleF InputPortRectangle
            {
                get
                {
                    return new RectangleF(
                        InputPortLocation,
                        new SizeF(View.PortSize, View.PortSize));
                }
            }

            public RectangleF OutputPortRectangle
            {
                get
                {
                    return new RectangleF(
                        OutputPortLocation,
                        new SizeF(View.PortSize, View.PortSize));
                }
            }

            public void SetNodeLabel(string text, Font font, Graphics graphics)
            {
                Text = text;
                var labelSize = SizeF.Empty;
                var labelLocation = Location;
                labelLocation.Y += View.NodeSize + View.LabelTextOffset;
                var labelBuilder = new StringBuilder(text.Length);
                foreach (var word in WordWrap(graphics, text, font, View.NodeAirspace))
                {
                    if (labelBuilder.Length > 0) labelBuilder.AppendLine();
                    labelBuilder.Append(word);
                    var size = graphics.MeasureString(word, font, (int)View.NodeAirspace, TextFormat);
                    labelSize.Width = Math.Max(size.Width, labelSize.Width);
                    labelSize.Height += size.Height;
                }

                Label = labelBuilder.ToString();
                LabelRectangle = new RectangleF(labelLocation, labelSize);
            }
        }

        struct SpinnerOffset
        {
            public SpinnerOffset(float x1, float y1, float x2, float y2)
            {
                Offset1 = new SizeF(x1, y1);
                Offset2 = new SizeF(x2, y2);
            }

            public SizeF Offset1;
            public SizeF Offset2;
        }
    }
}
