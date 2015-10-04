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

namespace Bonsai.Design
{
    partial class GraphView : UserControl
    {
        const int PenWidth = 3;
        const int NodeAirspace = 80;
        const int NodeSize = 30;
        const int IconSize = 16;
        const int HalfSize = NodeSize / 2;
        const int IconOffset = HalfSize - (IconSize / 2);
        const int LabelTextOffset = 5;
        static readonly Size TextOffset = new Size(9, 9);
        static readonly SizeF VectorTextOffset = new SizeF(11.3f, 8.3f);
        static readonly Size EntryOffset = new Size(-PenWidth / 2, NodeSize / 2);
        static readonly Size ExitOffset = new Size(NodeSize + PenWidth / 2, NodeSize / 2);
        static readonly Pen RubberBandPen = new Pen(Color.FromArgb(51, 153, 255));
        static readonly Brush RubberBandBrush = new SolidBrush(Color.FromArgb(128, 170, 204, 238));
        static readonly Brush HotBrush = new SolidBrush(Color.FromArgb(128, 229, 243, 251));
        static readonly Pen CursorPen = new Pen(Brushes.DarkGray, PenWidth);
        static readonly Pen WhitePen = new Pen(Brushes.White, PenWidth);
        static readonly Pen BlackPen = new Pen(Brushes.Black, PenWidth);

        static readonly object EventItemDrag = new object();
        static readonly object EventNodeMouseClick = new object();
        static readonly object EventNodeMouseDoubleClick = new object();
        static readonly object EventNodeMouseEnter = new object();
        static readonly object EventNodeMouseLeave = new object();
        static readonly object EventNodeMouseHover = new object();
        static readonly object EventSelectedNodeChanged = new object();

        bool ignoreMouseUp;
        bool mouseDownHandled;
        Rectangle rubberBand;
        Rectangle previousRectangle;
        Point? previousScrollOffset;
        GraphNode[] selectionBeforeDrag;
        LayoutNodeCollection layoutNodes = new LayoutNodeCollection();
        HashSet<GraphNode> selectedNodes = new HashSet<GraphNode>();
        IEnumerable<GraphNodeGrouping> nodes;
        GraphNode pivot;
        GraphNode cursor;
        GraphNode hot;

        public GraphView()
        {
            HighlightedBrush = Brushes.DarkRed;
            HighlightedSelectionBrush = Brushes.Red;
            FocusedSelectionBrush = Brushes.Black;
            UnfocusedSelectionBrush = Brushes.Gray;
            InitializeComponent();
            InitializeReactiveEvents();
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
                .Do(evt => ignoreMouseUp = true)
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

        public Brush HighlightedBrush { get; set; }

        public Brush HighlightedSelectionBrush { get; set; }

        public Brush FocusedSelectionBrush { get; set; }

        public Brush UnfocusedSelectionBrush { get; set; }

        public IEnumerable<GraphNodeGrouping> Nodes
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
                        if (value != null) selectedNodes.Add(value);
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
                        var selection = value.ToArray();
                        selectedNodes.Clear();
                        foreach (var node in selection)
                        {
                            selectedNodes.Add(node);
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
            if (layoutNodes.Contains(node))
            {
                canvas.Invalidate(GetBoundingRectangle(node));
            }
        }

        Rectangle GetBoundingRectangle(GraphNode node)
        {
            var offset = new Point(-canvas.HorizontalScroll.Value, -canvas.VerticalScroll.Value);
            var nodeLayout = layoutNodes[node];
            var boundingRectangle = nodeLayout.BoundingRectangle;
            boundingRectangle.Offset(offset);

            var nodeText = node.Text;
            var labelRectangle = nodeLayout.LabelRectangle;
            if (nodeText != nodeLayout.Label)
            {
                using (var graphics = CreateGraphics())
                {
                    nodeLayout.Label = nodeText;
                    labelRectangle = GetNodeLabelRectangle(nodeText, nodeLayout.Location, graphics);
                    nodeLayout.LabelRectangle = labelRectangle;
                }
            }

            labelRectangle.Offset(offset);
            return Rectangle.Union(boundingRectangle, Rectangle.Truncate(labelRectangle));
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
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            InvalidateSelection();
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

            if (keyData == Keys.Space && control && cursor != null)
            {
                if (selectedNodes.Contains(cursor)) ClearNode(cursor);
                else SelectNode(cursor, true);
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
                    var nodeCenter = new Point(
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
            var selectionRect = Rectangle.Empty;
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
                    foreach (var node in selectedNodes) selectionRect = Rectangle.Union(selectionRect, GetBoundingRectangle(node));
                    selectedNodes.Clear();
                    foreach (var node in selection)
                    {
                        selectedNodes.Add(node);
                        selectionRect = Rectangle.Union(selectionRect, GetBoundingRectangle(node));
                    }
                    OnSelectedNodeChanged(EventArgs.Empty);
                }
            }

            rubberBand = rect.GetValueOrDefault();
            var invalidateRect = rubberBand;
            invalidateRect.Inflate(PenWidth, PenWidth);
            invalidateRect = (selectionRect.Width > 0 || selectionRect.Height > 0) ? Rectangle.Union(invalidateRect, selectionRect) : invalidateRect;
            if (previousScrollOffset.HasValue)
            {
                var scrollPosition = canvas.AutoScrollPosition;
                var previousScroll = previousScrollOffset.Value;
                scrollPosition.X -= previousScroll.X;
                scrollPosition.Y -= previousScroll.Y;
                previousRectangle.Offset(scrollPosition);
            }

            canvas.Invalidate(Rectangle.Union(invalidateRect, previousRectangle));
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

        float SquaredDistance(ref Point point, ref Point center)
        {
            var xdiff = point.X - center.X;
            var ydiff = point.Y - center.Y;
            return xdiff * xdiff + ydiff * ydiff;
        }

        bool CircleIntersect(Point center, int radius, Point point)
        {
            return SquaredDistance(ref point, ref center) <= radius * radius;
        }

        bool CircleIntersect(Point center, int radius, Rectangle rect)
        {
            float closestX = Math.Max(rect.Left, Math.Min(center.X, rect.Right));
            float closestY = Math.Max(rect.Top, Math.Min(center.Y, rect.Bottom));
            float distanceX = center.X - closestX;
            float distanceY = center.Y - closestY;
            float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
            return distanceSquared < (radius * radius);
        }

        public Point GetNodeLocation(GraphNode node)
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

        public GraphNode GetClosestNodeTo(Point point)
        {
            point.X += canvas.HorizontalScroll.Value;
            point.Y += canvas.VerticalScroll.Value;

            float minDistance = 0;
            GraphNode closest = null;
            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value == null) continue;

                var center = layout.Center;
                var distance = SquaredDistance(ref point, ref center);
                if (closest == null || distance < minDistance)
                {
                    closest = layout.Node;
                    minDistance = distance;
                }
            }

            return closest;
        }

        RectangleF GetNodeLabelRectangle(string text, Point location, Graphics graphics)
        {
            var labelSize = graphics.MeasureString(text, Font);
            var labelLocation = new PointF(0, NodeSize + LabelTextOffset);
            labelLocation.X += location.X;
            labelLocation.Y += location.Y;
            return new RectangleF(labelLocation, labelSize);
        }

        private void UpdateModelLayout()
        {
            layoutNodes.Clear();
            var model = Nodes;
            Size size = Size.Empty;
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
                            var location = new Point(column * NodeAirspace + PenWidth, row * NodeAirspace + PenWidth);
                            var labelRectangle = GetNodeLabelRectangle(node.Text, location, graphics);
                            layoutNodes.Add(new LayoutNode(node, location, node.Text, labelRectangle));
                            maxRow = Math.Max(row, maxRow);
                        }

                        var rowHeight = (maxRow + 1) * NodeAirspace;
                        size.Height = Math.Max(rowHeight, size.Height);
                    }

                    size.Width = layerCount * NodeAirspace;
                }
            }

            canvas.AutoScrollMinSize = size;
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
                selectedNodes).Where(node => node.Value != null);
        }

        private void SelectRange(GraphNode node, bool unionUpdate)
        {
            var path = GetSelectionRange(pivot, node).ToArray();
            if (path.Length == 0) return;

            if (unionUpdate)
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

        public RectangleF DrawGraphics(IGraphics graphics)
        {
            var textBrush = Brushes.Black;
            var boundingRect = RectangleF.Empty;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.White);

            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value != null)
                {
                    var unionRect = RectangleF.Union(layout.BoundingRectangle, layout.LabelRectangle);
                    boundingRect = RectangleF.Union(boundingRect, unionRect);

                    var selected = selectedNodes.Contains(layout.Node);
                    var nodeRectangle = new Rectangle(
                        layout.Location.X,
                        layout.Location.Y,
                        NodeSize, NodeSize);

                    graphics.DrawEllipse(BlackPen, nodeRectangle);
                    graphics.FillEllipse(layout.Node.Brush, nodeRectangle);
                    if (layout.Node.Image != null)
                    {
                        var imageRect = new Rectangle(
                            nodeRectangle.X + IconOffset,
                            nodeRectangle.Y + IconOffset,
                            IconSize, IconSize);
                        graphics.DrawImage(layout.Node.Image, imageRect);
                    }
                    else
                    {
                        graphics.DrawString(
                            layout.Label.Substring(0, 1),
                            Font, textBrush,
                            PointF.Add(layout.Location, VectorTextOffset));
                    }

                    var labelRect = layout.LabelRectangle;
                    labelRect.Width = NodeAirspace;
                    labelRect.Location = new PointF(
                        labelRect.Location.X,
                        labelRect.Location.Y);
                    graphics.DrawString(
                        layout.Label,
                        Font, textBrush,
                        labelRect);
                }
                else graphics.DrawLine(layout.Node.Pen, layout.EntryPoint, layout.ExitPoint);

                foreach (var successor in layout.Node.Successors)
                {
                    var successorLayout = layoutNodes[successor.Node];
                    graphics.DrawLine(layout.Node.Pen, layout.ExitPoint, successorLayout.EntryPoint);
                }
            }

            return boundingRect;
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            var offset = new Size(-canvas.HorizontalScroll.Value, -canvas.VerticalScroll.Value);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value != null)
                {
                    var selected = selectedNodes.Contains(layout.Node);
                    var nodeRectangle = new Rectangle(
                        layout.Location.X + offset.Width,
                        layout.Location.Y + offset.Height,
                        NodeSize, NodeSize);

                    Pen pen;
                    Brush brush;
                    if (layout.Node.Highlight)
                    {
                        brush = selected ? HighlightedSelectionBrush : HighlightedBrush;
                        pen = WhitePen;
                    }
                    else
                    {
                        pen = cursor == layout.Node ? CursorPen : selected ? WhitePen : BlackPen;
                        brush = selected ? (Focused ? FocusedSelectionBrush : UnfocusedSelectionBrush) : layout.Node.Brush;
                    }

                    var textBrush = selected ? Brushes.White : Brushes.Black;
                    e.Graphics.DrawEllipse(pen, nodeRectangle);
                    e.Graphics.FillEllipse(brush, nodeRectangle);
                    if (layout.Node == hot) e.Graphics.FillEllipse(HotBrush, nodeRectangle);
                    if (layout.Node.Image != null)
                    {
                        var imageRect = new Rectangle(
                            nodeRectangle.X + IconOffset,
                            nodeRectangle.Y + IconOffset,
                            IconSize, IconSize);
                        e.Graphics.DrawImage(layout.Node.Image, imageRect);
                    }
                    else
                    {
                        e.Graphics.DrawString(
                            layout.Label.Substring(0, 1),
                            Font, textBrush,
                            Point.Add(layout.Location, Size.Add(offset, TextOffset)));
                    }
                }
                else e.Graphics.DrawLine(layout.Node.Pen, Point.Add(layout.EntryPoint, offset), Point.Add(layout.ExitPoint, offset));

                foreach (var successor in layout.Node.Successors)
                {
                    var successorLayout = layoutNodes[successor.Node];
                    e.Graphics.DrawLine(layout.Node.Pen, Point.Add(layout.ExitPoint, offset), Point.Add(successorLayout.EntryPoint, offset));
                }
            }

            if (hot != null)
            {
                var layout = layoutNodes[hot];
                var labelRect = layout.LabelRectangle;
                labelRect.Location = new PointF(
                    labelRect.Location.X + offset.Width,
                    labelRect.Location.Y + offset.Height);
                e.Graphics.DrawString(
                    layout.Label,
                    Font, Brushes.Black,
                    labelRect);
            }

            if (rubberBand.Width > 0 && rubberBand.Height > 0)
            {
                e.Graphics.FillRectangle(RubberBandBrush, rubberBand);
                e.Graphics.DrawRectangle(RubberBandPen, rubberBand);
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
            public LayoutNode(GraphNode node, Point location, string label, RectangleF labelRectangle)
            {
                Node = node;
                Location = location;
                Label = label;
                LabelRectangle = labelRectangle;
            }

            public GraphNode Node { get; private set; }

            public Point Location { get; private set; }

            public string Label { get; set; }

            public RectangleF LabelRectangle { get; set; }

            public Point Center
            {
                get { return Point.Add(Location, new Size(HalfSize, HalfSize)); }
            }

            public Point EntryPoint
            {
                get { return Point.Add(Location, EntryOffset); }
            }

            public Point ExitPoint
            {
                get { return Point.Add(Location, ExitOffset); }
            }

            public Rectangle BoundingRectangle
            {
                get
                {
                    return new Rectangle(
                        Location.X - PenWidth, Location.Y - PenWidth,
                        NodeSize + 2 * PenWidth, NodeSize + 2 * PenWidth);
                }
            }
        }
    }
}
