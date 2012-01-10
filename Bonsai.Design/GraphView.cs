﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;

namespace Bonsai.Design
{
    public partial class GraphView : UserControl
    {
        const int PenWidth = 3;
        const int NodeAirspace = 80;
        const int NodeSize = 30;
        const int TextOffset = 9;
        const int HalfSize = NodeSize / 2;
        static readonly Pen WhitePen = new Pen(Brushes.White, PenWidth);
        static readonly Pen BlackPen = new Pen(Brushes.Black, PenWidth);

        static readonly object EventItemDrag = new object();
        static readonly object EventNodeMouseClick = new object();
        static readonly object EventNodeMouseDoubleClick = new object();
        static readonly object EventNodeMouseHover = new object();
        static readonly object EventSelectedNodeChanged = new object();
        LayoutNodeCollection layoutNodes = new LayoutNodeCollection();

        GraphNode selectedNode;
        IEnumerable<GraphNodeGrouping> model;

        public GraphView()
        {
            InitializeComponent();
        }

        public event ItemDragEventHandler ItemDrag
        {
            add { Events.AddHandler(EventItemDrag, value); }
            remove { Events.RemoveHandler(EventItemDrag, value); }
        }

        public event EventHandler<GraphNodeMouseClickEventArgs> NodeMouseClick
        {
            add { Events.AddHandler(EventNodeMouseClick, value); }
            remove { Events.RemoveHandler(EventNodeMouseClick, value); }
        }

        public event EventHandler<GraphNodeMouseClickEventArgs> NodeMouseDoubleClick
        {
            add { Events.AddHandler(EventNodeMouseDoubleClick, value); }
            remove { Events.RemoveHandler(EventNodeMouseDoubleClick, value); }
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

        public IEnumerable<GraphNodeGrouping> Model
        {
            get { return model; }
            set
            {
                model = value;
                SelectedNode = null;
                UpdateModelLayout();
            }
        }

        public GraphNode SelectedNode
        {
            get { return selectedNode; }
            set
            {
                if (selectedNode != value)
                {
                    InvalidateNode(selectedNode);
                    selectedNode = value;
                    InvalidateNode(selectedNode);
                    OnSelectedNodeChanged(EventArgs.Empty);
                }
            }
        }

        void InvalidateNode(GraphNode node)
        {
            if (selectedNode != null)
            {
                var nodeLayout = layoutNodes[selectedNode];
                var boundingRectangle = nodeLayout.BoundingRectangle;
                boundingRectangle.X -= canvas.HorizontalScroll.Value;
                boundingRectangle.Y -= canvas.VerticalScroll.Value;

                canvas.Invalidate(boundingRectangle);
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

        protected virtual void OnNodeMouseClick(GraphNodeMouseClickEventArgs e)
        {
            var handler = Events[EventNodeMouseClick] as EventHandler<GraphNodeMouseClickEventArgs>;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnNodeMouseDoubleClick(GraphNodeMouseClickEventArgs e)
        {
            var handler = Events[EventNodeMouseDoubleClick] as EventHandler<GraphNodeMouseClickEventArgs>;
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

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            canvas.Invalidate(e.InvalidRect);
            base.OnInvalidated(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Size selectionOffset;
            switch (keyData)
            {
                case Keys.Up: selectionOffset = new Size(0, -NodeAirspace); break;
                case Keys.Down: selectionOffset = new Size(0, NodeAirspace); break;
                case Keys.Left: selectionOffset = new Size(-NodeAirspace, 0); break;
                case Keys.Right: selectionOffset = new Size(NodeAirspace, 0); break;
                default: selectionOffset = Size.Empty; break;
            }

            if (selectionOffset != Size.Empty)
            {
                SelectedNode = SelectedNode == null
                    ? layoutNodes.Select(layoutNode => layoutNode.Node).FirstOrDefault()
                    : GetClosestNodeTo(Point.Add(layoutNodes[SelectedNode].Location, selectionOffset));
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        float SquaredDistance(ref Point point, ref Point center)
        {
            var xdiff = point.X - center.X;
            var ydiff = point.Y - center.Y;
            return xdiff * xdiff + ydiff * ydiff;
        }

        bool CircleIntersect(Point point, Point center, int radius)
        {
            return SquaredDistance(ref point, ref center) <= radius * radius;
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

                if (CircleIntersect(point, layout.Center, HalfSize))
                {
                    return layout.Node;
                }
            }

            return null;
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

        private void UpdateModelLayout()
        {
            layoutNodes.Clear();
            var model = Model;
            Size size = Size.Empty;
            if (model != null)
            {
                var layerCount = model.Count();
                foreach (var layer in model)
                {
                    var column = layerCount - layer.Key - 1;
                    foreach (var node in layer)
                    {
                        var row = node.LayerIndex;
                        var location = new Point(column * NodeAirspace + PenWidth, row * NodeAirspace + PenWidth);
                        var entryPoint = new Point(location.X - PenWidth / 2, location.Y + NodeSize / 2);
                        var exitPoint = new Point(location.X + NodeSize + PenWidth / 2, location.Y + NodeSize / 2);
                        layoutNodes.Add(new LayoutNode(node, location, entryPoint, exitPoint));
                    }

                    var rowHeight = layer.Count * NodeAirspace;
                    size.Height = Math.Max(rowHeight, size.Height);
                }

                size.Width = layerCount * NodeAirspace;
            }

            canvas.AutoScrollMinSize = size;
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            var offset = new Point(-canvas.HorizontalScroll.Value, -canvas.VerticalScroll.Value);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value != null)
                {
                    var selected = layout.Node == SelectedNode;
                    var nodeRectangle = new Rectangle(layout.Location.X + offset.X, layout.Location.Y + offset.Y, NodeSize, NodeSize);

                    var pen = selected ? WhitePen : BlackPen;
                    var brush = selected ? Brushes.Black : Brushes.White;
                    var textBrush = selected ? Brushes.White : Brushes.Black;

                    e.Graphics.DrawEllipse(pen, nodeRectangle);
                    e.Graphics.FillEllipse(brush, nodeRectangle);
                    e.Graphics.DrawString(
                        layout.Node.Text.Substring(0, 1),
                        Font, textBrush,
                        new Point(layout.Location.X + offset.X + TextOffset, layout.Location.Y + offset.Y + TextOffset));
                }
                else e.Graphics.DrawLine(
                    Pens.Black,
                    layout.EntryPoint.X + offset.X, layout.EntryPoint.Y + offset.Y,
                    layout.ExitPoint.X + offset.X, layout.ExitPoint.Y + offset.Y);

                foreach (var successor in layout.Node.Successors)
                {
                    var successorLayout = layoutNodes[successor];
                    e.Graphics.DrawLine(
                        Pens.Black,
                        layout.ExitPoint.X + offset.X, layout.ExitPoint.Y + offset.Y,
                        successorLayout.EntryPoint.X + offset.X, successorLayout.EntryPoint.Y + offset.Y);
                }
            }
        }

        private void canvas_MouseClick(object sender, MouseEventArgs e)
        {
            var node = GetNodeAt(e.Location);
            if (node != null)
            {
                SelectedNode = node;
                OnNodeMouseClick(new GraphNodeMouseClickEventArgs(node, e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }
        }

        private void canvas_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var node = GetNodeAt(e.Location);
            if (node != null)
            {
                OnNodeMouseDoubleClick(new GraphNodeMouseClickEventArgs(node, e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }
        }

        private void canvas_MouseHover(object sender, EventArgs e)
        {
            var node = GetNodeAt(PointToClient(MousePosition));
            if (node != null)
            {
                OnNodeMouseHover(new GraphNodeMouseHoverEventArgs(node));
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
            public LayoutNode(GraphNode node, Point location, Point entryPoint, Point exitPoint)
            {
                Node = node;
                Location = location;
                EntryPoint = entryPoint;
                ExitPoint = exitPoint;
            }

            public GraphNode Node { get; set; }

            public Point Location { get; set; }

            public Point Center
            {
                get { return new Point(Location.X + HalfSize, Location.Y + HalfSize); }
            }

            public Point EntryPoint { get; set; }

            public Point ExitPoint { get; set; }

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
