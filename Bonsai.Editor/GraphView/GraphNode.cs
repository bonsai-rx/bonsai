using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Bonsai.Expressions;

namespace Bonsai.Editor.GraphView
{
    class GraphNode
    {
        static readonly Range<int> EmptyRange = Range.Create(0, 0);
        static readonly Brush DisabledBrush = new HatchBrush(HatchStyle.BackwardDiagonal, Color.Black, Color.Transparent);
        static readonly Brush ObsoleteBrush = new HatchBrush(HatchStyle.OutlinedDiamond, Color.Black, Color.Transparent);

        public GraphNode(ExpressionBuilder value, int layer, IEnumerable<GraphEdge> successors)
        {
            Value = value;
            Layer = layer;
            Successors = successors;

            if (value != null)
            {
                var expressionBuilder = ExpressionBuilder.Unwrap(value);
                var elementAttributes = TypeDescriptor.GetAttributes(expressionBuilder);
                var elementCategoryAttribute = (WorkflowElementCategoryAttribute)elementAttributes[typeof(WorkflowElementCategoryAttribute)];
                var obsolete = (ObsoleteAttribute)elementAttributes[typeof(ObsoleteAttribute)] != null;
                if (expressionBuilder is DisableBuilder) Flags |= NodeFlags.Disabled;

                var workflowElement = ExpressionBuilder.GetWorkflowElement(expressionBuilder);
                if (workflowElement != expressionBuilder)
                {
                    var builderCategoryAttribute = elementCategoryAttribute;
                    elementAttributes = TypeDescriptor.GetAttributes(workflowElement);
                    elementCategoryAttribute = (WorkflowElementCategoryAttribute)elementAttributes[typeof(WorkflowElementCategoryAttribute)];
                    obsolete |= (ObsoleteAttribute)elementAttributes[typeof(ObsoleteAttribute)] != null;
                    if (elementCategoryAttribute == WorkflowElementCategoryAttribute.Default)
                    {
                        elementCategoryAttribute = builderCategoryAttribute;
                    }
                }

                if (obsolete) Flags |= NodeFlags.Obsolete;
                Category = elementCategoryAttribute.Category;
                BuildDependency = expressionBuilder.IsBuildDependency();
                Icon = new ElementIcon(workflowElement);
                if (workflowElement is IWorkflowExpressionBuilder)
                {
                    if (Category == ElementCategory.Workflow)
                    {
                        Category = ElementCategory.Combinator;
                        Flags |= NodeFlags.NestedGroup;
                    }
                    else Flags |= NodeFlags.NestedScope;
                }
            }

            InitializeDummySuccessors();
        }

        void InitializeDummySuccessors()
        {
            foreach (var successor in Successors)
            {
                if (successor.Node.Value == null)
                {
                    successor.Node.BuildDependency = BuildDependency;
                    successor.Node.InitializeDummySuccessors();
                }
            }
        }

        private NodeFlags Flags { get; set; }

        public int Layer { get; internal set; }

        public int LayerIndex { get; internal set; }

        public int ArgumentCount { get; internal set; }

        public Range<int> ArgumentRange
        {
            get { return (Flags & NodeFlags.Disabled) != 0 || Value == null ? EmptyRange : Value.ArgumentRange; }
        }

        public ExpressionBuilder Value { get; private set; }

        public IEnumerable<GraphEdge> Successors { get; private set; }

        public object Tag { get; set; }

        public Color FillColor
        {
            get
            {
                switch (Category)
                {
                    case ElementCategory.Source: return CategoryColors.Source;
                    case ElementCategory.Condition: return CategoryColors.Combinator;
                    case ElementCategory.Transform: return CategoryColors.Transform;
                    case ElementCategory.Sink: return CategoryColors.Sink;
                    case ElementCategory.Nested:
                    case ElementCategory.Workflow: return CategoryColors.Combinator;
                    case ElementCategory.Property: return CategoryColors.Property;
                    case ElementCategory.Combinator:
                    default: return CategoryColors.Combinator;
                }
            }
        }

        public Brush ModifierBrush
        {
            get
            {
                if ((Flags & NodeFlags.Disabled) != 0) return DisabledBrush;
                else if ((Flags & NodeFlags.Obsolete) != 0) return ObsoleteBrush;
                else return null;
            }
        }

        public ElementCategory? NestedCategory
        {
            get
            {
                if ((Flags & NodeFlags.NestedScope) != 0) return ElementCategory.Nested;
                else if ((Flags & NodeFlags.NestedGroup) != 0) return ElementCategory.Workflow;
                else return null;
            }
        }

        public ElementCategory Category { get; private set; }

        public ElementIcon Icon { get; private set; }

        public bool BuildDependency { get; private set; }

        public string Text
        {
            get { return Value != null ? ExpressionBuilder.GetElementDisplayName(Value) : string.Empty; }
        }

        public bool Highlight
        {
            get { return (Flags & NodeFlags.Highlight) != 0; }
            set
            {
                if (value) Flags |= NodeFlags.Highlight;
                else Flags &= ~NodeFlags.Highlight;
            }
        }


        /// <summary>
        /// Returns a string that represents the value of this <see cref="GraphNode"/> instance.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="GraphNode"/> object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{{{0}}}", Text);
        }

        [Flags]
        enum NodeFlags
        {
            None = 0x0,
            Highlight = 0x1,
            Obsolete = 0x2,
            Disabled = 0x4,
            NestedScope = 0x8,
            NestedGroup = 0x10
        }

        static class CategoryColors
        {
            public static readonly Color Source = Color.FromArgb(91, 178, 126);
            public static readonly Color Transform = Color.FromArgb(68, 154, 223);
            public static readonly Color Sink = Color.FromArgb(155, 91, 179);
            public static readonly Color Combinator = Color.FromArgb(238, 192, 75);
            public static readonly Color Property = Color.Gray;
        }
    }
}
