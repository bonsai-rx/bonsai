using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using SvgNet.SvgGdi;
using System.Drawing.Text;

namespace Bonsai.Editor.GraphView
{
    partial class GraphViewControl : UserControl
    {
        const float DefaultDpi = 96f;
        const float DefaultPenWidth = 2;
        const float DefaultNodeSize = 30;
        const float DefaultNodeAirspace = 80;
        const float DefaultConnectorSize = 6;
        const float DefaultLabelTextOffset = 5;
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
        static readonly StringFormat TextFormat = new StringFormat(StringFormatFlags.NoWrap);
        static readonly StringFormat CenteredTextFormat = new StringFormat(StringFormat.GenericTypographic)
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        static readonly StringFormat VectorTextFormat = new StringFormat(StringFormat.GenericTypographic)
        {
            FormatFlags = StringFormatFlags.NoWrap,
            Trimming = StringTrimming.Character
        };

        static readonly object EventItemDrag = new object();
        static readonly object EventNodeMouseClick = new object();
        static readonly object EventNodeMouseDoubleClick = new object();
        static readonly object EventNodeMouseEnter = new object();
        static readonly object EventNodeMouseLeave = new object();
        static readonly object EventNodeMouseHover = new object();
        static readonly object EventSelectedNodeChanged = new object();

        float PenWidth;
        float NodeAirspace;
        float NodeSize;
        float HalfSize;
        float ConnectorSize;
        float LabelTextOffset;
        SizeF VectorTextOffset;
        SizeF EntryOffset;
        SizeF ExitOffset;
        SizeF ConnectorOffset;
        Pen SolidPen;
        Pen DashPen;
        Font DefaultIconFont;
        Font ExportFont;

        float drawScale;
        bool ignoreMouseUp;
        bool mouseDownHandled;
        RectangleF rubberBand;
        RectangleF previousRectangle;
        Point? previousScrollOffset;
        GraphNode[] selectionBeforeDrag;
        GraphViewTextDrawMode textDrawMode;
        LayoutNodeCollection layoutNodes = new LayoutNodeCollection();
        HashSet<GraphNode> selectedNodes = new HashSet<GraphNode>();
        SvgRendererState iconRendererState = new SvgRendererState();
        IEnumerable<GraphNodeGrouping> nodes;
        GraphNode pivot;
        GraphNode cursor;
        GraphNode hot;

        public GraphViewControl()
        {
            InitializeComponent();
            InitializeReactiveEvents();
            FocusedSelectionColor = Color.Black;
            UnfocusedSelectionColor = Color.Gray;
            ContrastSelectionColor = Color.White;
            TextDrawMode = GraphViewTextDrawMode.All;
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

        public GraphViewTextDrawMode TextDrawMode
        {
            get { return textDrawMode; }
            set
            {
                textDrawMode = value;
                UpdateModelLayout();
            }
        }

        private bool WrapLabels
        {
            get { return textDrawMode == GraphViewTextDrawMode.All; }
        }

        public IEnumerable<GraphNodeGrouping> Nodes
        {
            get { return nodes; }
            set
            {
                cursor = null;
                pivot = null;
                nodes = value;
                hot = null;
                SelectedNode = null;
                UpdateModelLayout();
            }
        }

        public GraphNode CursorNode
        {
            get { return cursor; }
        }

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
                        var cursorNode = cursor;
                        selectedNodes.Clear();
                        if (value != null)
                        {
                            foreach (var node in value)
                            {
                                selectedNodes.Add(node);
                                cursorNode = node;
                            }

                            if (cursorNode != cursor)
                            {
                                SetCursor(cursorNode);
                                pivot = cursorNode;
                            }
                        }
                    });
                }
            }
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

            if (ExportFont != null)
            {
                ExportFont.Dispose();
                ExportFont = null;
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
            using (var graphics = CreateGraphics())
            {
                drawScale = graphics.DpiY / DefaultDpi * Font.SizeInPoints / Control.DefaultFont.SizeInPoints;
            }

            iconRendererState.Scale = drawScale;
            PenWidth = DefaultPenWidth * drawScale;
            NodeAirspace = DefaultNodeAirspace * drawScale;
            NodeSize = DefaultNodeSize * drawScale;
            HalfSize = NodeSize / 2;
            ConnectorSize = DefaultConnectorSize * drawScale;
            LabelTextOffset = DefaultLabelTextOffset * drawScale;
            VectorTextOffset = new SizeF(0, 1.375f * drawScale);
            EntryOffset = new SizeF(-2 * DefaultPenWidth * drawScale, DefaultNodeSize * drawScale / 2);
            ExitOffset = new SizeF(NodeSize + 2 * DefaultPenWidth * drawScale, DefaultNodeSize * drawScale / 2);
            ConnectorOffset = new SizeF(-ConnectorSize / 2, EntryOffset.Height - ConnectorSize / 2);
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
                using (var graphics = CreateGraphics())
                {
                    nodeLayout.SetNodeLabel(nodeText, Font, graphics, WrapLabels);
                    labelRectangle = RectangleF.Union(labelRectangle, nodeLayout.LabelRectangle);
                }
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
            var handler = Events[EventItemDrag] as ItemDragEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnNodeMouseClick(GraphNodeMouseEventArgs e)
        {
            var handler = Events[EventNodeMouseClick] as EventHandler<GraphNodeMouseEventArgs>;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnNodeMouseDoubleClick(GraphNodeMouseEventArgs e)
        {
            var handler = Events[EventNodeMouseDoubleClick] as EventHandler<GraphNodeMouseEventArgs>;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnNodeMouseEnter(GraphNodeMouseEventArgs e)
        {
            var handler = Events[EventNodeMouseEnter] as EventHandler<GraphNodeMouseEventArgs>;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnNodeMouseLeave(GraphNodeMouseEventArgs e)
        {
            var handler = Events[EventNodeMouseLeave] as EventHandler<GraphNodeMouseEventArgs>;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnNodeMouseHover(GraphNodeMouseHoverEventArgs e)
        {
            var handler = Events[EventNodeMouseHover] as EventHandler<GraphNodeMouseHoverEventArgs>;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelectedNodeChanged(EventArgs e)
        {
            var handler = Events[EventSelectedNodeChanged] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            InvalidateSelection();
            Invalidate(cursor);
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            ignoreMouseUp = true;
            InvalidateSelection();
            Invalidate(cursor);
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

            if (keyData == Keys.Space && cursor != null)
            {
                if (selectedNodes.Contains(cursor))
                {
                    if (control) ClearNode(cursor);
                    else ClearSelection();
                }
                else SelectNode(cursor, control);
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

            if (cursor != null && stepCursor)
            {
                StepCursor(keyData);
                if (shift)
                {
                    SelectRange(cursor, control);
                }
                else
                {
                    pivot = cursor;
                    if (!control) SelectNode(cursor, false);
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
                    var nodeCenter = new PointF(
                        layout.Location.X + offset.Width,
                        layout.Location.Y + offset.Height);
                    var selected = CircleIntersect(layout.Center, HalfSize, selectionRect);
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
                if (Control.ModifierKeys.HasFlag(Keys.Control))
                {
                    selection.SymmetricExceptWith(selectionBeforeDrag);
                }
                else if (Control.ModifierKeys.HasFlag(Keys.Shift))
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

        Rectangle GetNormalizedRectangle(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p2.X - p1.X),
                Math.Abs(p2.Y - p1.Y));
        }

        float SquaredDistance(ref PointF point, ref PointF center)
        {
            var xdiff = point.X - center.X;
            var ydiff = point.Y - center.Y;
            return xdiff * xdiff + ydiff * ydiff;
        }

        bool CircleIntersect(PointF center, float radius, PointF point)
        {
            return SquaredDistance(ref point, ref center) <= radius * radius;
        }

        bool CircleIntersect(PointF center, float radius, RectangleF rect)
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

        public GraphNode GetNodeAt(Point point)
        {
            point.X += canvas.HorizontalScroll.Value;
            point.Y += canvas.VerticalScroll.Value;

            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value == null) continue;

                if (CircleIntersect(layout.Center, HalfSize, point))
                {
                    return layout.Node;
                }
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

        GraphNode ResolveDummySuccessor(GraphNode source)
        {
            while (source.Value == null)
            {
                source = source.Successors.First().Node;
            }

            return source;
        }

        GraphNode GetDefaultSuccessor(GraphNode node)
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
            if (cursor == null) return;
            var layer = cursor.Layer;
            var layerIndex = cursor.LayerIndex;
            if (step == Keys.Right) layer--;
            if (step == Keys.Left) layer++;
            if (step == Keys.Up) layerIndex--;
            if (step == Keys.Down) layerIndex++;

            GraphNode selection = null;
            if (selection == null)
            {
                if (layer != cursor.Layer)
                {
                    selection = (from layout in layoutNodes
                                 where layout.Node.Value != null &&
                                       layout.Node.LayerIndex == layerIndex && (layer > cursor.Layer
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
                                       layout.Node.Layer == layer && (layerIndex > cursor.LayerIndex
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
                    selection = GetDefaultSuccessor(cursor);
                }

                if (step == Keys.Left)
                {
                    selection = GetDefaultPredecessor(cursor);
                }

                if (step == Keys.Down)
                {
                    selection = GetSiblings(cursor).SkipWhile(node => node != cursor).Skip(1).FirstOrDefault();
                }

                if (step == Keys.Up)
                {
                    selection = GetSiblings(cursor).TakeWhile(node => node != cursor).LastOrDefault();
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
            if (model != null)
            {
                using (var graphics = CreateGraphics())
                {
                    var layerCount = model.Count();
                    foreach (var layer in model)
                    {
                        var maxRow = 0;
                        var column = layerCount - layer.Key - 1;
                        foreach (var node in layer)
                        {
                            if (pivot == null) pivot = cursor = node;
                            var row = node.LayerIndex;
                            var location = new PointF(column * NodeAirspace + 2 * PenWidth, row * NodeAirspace + 2 * PenWidth);
                            var layout = new LayoutNode(this, node, location);
                            layout.SetNodeLabel(node.Text, Font, graphics, WrapLabels);
                            layoutNodes.Add(layout);
                            maxRow = Math.Max(row, maxRow);
                        }

                        var rowHeight = (maxRow + 1) * NodeAirspace;
                        size.Height = Math.Max(rowHeight, size.Height);
                    }

                    size.Width = layerCount * NodeAirspace;
                }
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

        private IEnumerable<GraphNode> GetSelectionRange(GraphNode from, GraphNode to)
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
            Invalidate(cursor);
            cursor = node;
            Invalidate(node);
            EnsureVisible(cursor);
        }

        private static string[] GetWords(string text)
        {
            var wordCount = 0;
            var words = new string[text.Length];
            var builder = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                if (builder.Length > 0 && (Char.IsUpper(c) || Char.IsWhiteSpace(c)))
                {
                    words[wordCount++] = builder.ToString();
                    builder.Clear();
                }

                builder.Append(c);
            }

            if (builder.Length > 0) words[wordCount++] = builder.ToString();
            Array.Resize(ref words, wordCount);
            return words;
        }

        private static IEnumerable<string> WordWrap(Graphics graphics, string text, Font font, float lineWidth)
        {
            var trimStart = true;
            var words = GetWords(text);
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
                        if (Char.IsWhiteSpace(c)) continue;
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
            if (layout.Node.ModifierBrush != null)
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
                var connectorRectangle = layout.ConnectorRectangle;
                var argumentRequired = layout.Node.ArgumentCount < layout.Node.ArgumentRange.LowerBound;
                var connectorBrush = argumentRequired ? Brushes.Black : Brushes.White;
                connectorRectangle.Offset(offset.Width, offset.Height);
                graphics.DrawEllipse(iconRendererState.StrokeStyle(NodeEdgeColor, PenWidth), connectorRectangle);
                graphics.FillEllipse(connectorBrush, connectorRectangle);
            }
        }

        private void DrawDummyNode(IGraphics graphics, LayoutNode layout, Size offset)
        {
            if (layout.Node.Tag != null)
            {
                graphics.DrawLine(
                    layout.Node.BuildDependency ? DashPen : SolidPen,
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
                        layout.Node.BuildDependency ? DashPen : SolidPen,
                        PointF.Add(layout.ExitPoint, offset),
                        PointF.Add(successorLayout.EntryPoint, offset));
                }
            }
        }

        public void DrawGraphics(IGraphics graphics, bool scaleFont)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.White);

            using (var measureGraphics = CreateGraphics())
            {
                var font = Font;
                var fontScale = measureGraphics.DpiY / DefaultDpi;
                if (scaleFont && fontScale != 1.0)
                {
                    ExportFont = ExportFont ?? new Font(Font.FontFamily, Font.SizeInPoints * fontScale);
                    font = ExportFont;
                }

                using (var fill = new SolidBrush(Color.White))
                using (var stroke = new Pen(NodeEdgeColor, PenWidth))
                {
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
                                int charactersFitted, linesFilled;
                                var size = measureGraphics.MeasureString(
                                    line, Font,
                                    labelRect.Size,
                                    VectorTextFormat,
                                    out charactersFitted, out linesFilled);
                                var lineLabel = line.Length > charactersFitted ? line.Substring(0, charactersFitted) : line;
                                graphics.DrawString(lineLabel, font, Brushes.Black, labelRect);
                                labelRect.Y += size.Height;
                            }
                        }
                        else DrawDummyNode(graphics, layout, Size.Empty);
                    }
                }
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
                        var cursorColor = cursor == layout.Node ? CursorColor : default(Color?);
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
                        if (TextDrawMode == GraphViewTextDrawMode.All || layout.Node == hot)
                        {
                            selection.Color = ContrastSelectionColor;
                            var labelRect = layout.LabelRectangle;
                            labelRect.Location += offset;
                            graphics.DrawString(layout.Label, Font, selection, labelRect, TextFormat);
                        }
                    }
                    else DrawDummyNode(graphics, layout, offset);
                }
            }

            if (rubberBand.Width > 0 && rubberBand.Height > 0)
            {
                using (var rubberBandPen = new Pen(RubberBandPenColor))
                using (var rubberBandBrush = new SolidBrush(RubberBandBrushColor))
                {
                    e.Graphics.FillRectangle(rubberBandBrush, rubberBand);
                    e.Graphics.DrawRectangle(rubberBandPen, rubberBand.X, rubberBand.Y, rubberBand.Width, rubberBand.Height);
                }
            }
        }

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            ignoreMouseUp = false;
            if (!Focused) Select();
            if (hot != null)
            {
                SetCursor(hot);
                if (Control.ModifierKeys.HasFlag(Keys.Shift))
                {
                    mouseDownHandled = true;
                    SelectRange(hot, Control.ModifierKeys.HasFlag(Keys.Control));
                }
                else
                {
                    pivot = cursor;
                    if (!selectedNodes.Contains(hot))
                    {
                        mouseDownHandled = true;
                        SelectNode(hot, Control.ModifierKeys.HasFlag(Keys.Control));
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
                    if (Control.ModifierKeys.HasFlag(Keys.Control)) ClearNode(hot);
                    else SelectNode(hot, false);
                }
            }
            else if (Control.ModifierKeys == Keys.None)
            {
                ClearSelection();
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
                get { return PointF.Add(Location, new SizeF(View.HalfSize, View.HalfSize)); }
            }

            public PointF EntryPoint
            {
                get { return PointF.Add(Location, View.EntryOffset); }
            }

            public PointF ExitPoint
            {
                get { return PointF.Add(Location, View.ExitOffset); }
            }

            public PointF ConnectorLocation
            {
                get { return PointF.Add(Location, View.ConnectorOffset); }
            }

            public RectangleF BoundingRectangle
            {
                get
                {
                    return new RectangleF(
                        ConnectorLocation.X - View.PenWidth, Location.Y - View.PenWidth,
                        View.ConnectorSize / 2 + View.NodeSize + 2 * View.PenWidth, View.NodeSize + 2 * View.PenWidth);
                }
            }

            public RectangleF ConnectorRectangle
            {
                get
                {
                    return new RectangleF(
                        ConnectorLocation,
                        new SizeF(View.ConnectorSize, View.ConnectorSize));
                }
            }

            public void SetNodeLabel(string text, Font font, Graphics graphics, bool wrap)
            {
                Text = text;
                var labelSize = SizeF.Empty;
                var labelLocation = (PointF)Location;
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
    }
}
