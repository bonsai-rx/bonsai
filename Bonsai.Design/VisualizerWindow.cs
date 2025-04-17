﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;
using Bonsai.Expressions;

namespace Bonsai.Design
{
    /// <summary>
    /// Represents an expression builder specifying that a visualizer window should be
    /// displayed on this operator at workflow start.
    /// </summary>
    [DefaultProperty(nameof(Name))]
    [WorkflowElementCategory(ElementCategory.Property)]
    [WorkflowElementIcon("Bonsai:ElementIcon.Visualizer")]
    [Description("Specifies that a visualizer window should be displayed on this operator at workflow start.")]
    public sealed class VisualizerWindow : SingleArgumentExpressionBuilder, INamedElement, IVisualizerMappingBuilder, ISerializableElement
    {
        /// <summary>
        /// Gets or sets the name of the visualizer window.
        /// </summary>
        [Category(nameof(CategoryAttribute.Design))]
        [Description("The name of the visualizer window.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether the visualizer window is visible
        /// on start.
        /// </summary>
        [Description("Specifies whether the visualizer window is visible on start.")]
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Gets or sets a value specifying the position of the upper-left corner of the
        /// visualizer window, in screen coordinates.
        /// </summary>
        [Description("Specifies the position of the upper-left corner of the visualizer window, in screen coordinates.")]
        public Point Location { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the size of the visualizer window.
        /// </summary>
        [Description("Specifies the size of the visualizer window.")]
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether the visualizer window should start
        /// minimized, maximized, or normal.
        /// </summary>
        [Description("Specifies whether the visualizer window should start minimized, maximized, or normal.")]
        public FormWindowState WindowState { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the type of the visualizer to display in
        /// the visualizer window.
        /// </summary>
        [Externalizable(false)]
        [Description("Specifies the type of the visualizer to display in the visualizer window.")]
        public TypeMapping VisualizerType { get; set; }

        object ISerializableElement.Element
        {
            get { return VisualizerType; }
        }

        /// <summary>
        /// Generates an <see cref="Expression"/> node from a collection of input arguments.
        /// The result can be chained with other builders in a workflow.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            return arguments.First();
        }
    }
}
