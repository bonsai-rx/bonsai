using System.Collections.Generic;
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
    [DefaultProperty(nameof(Text))]
    [Description("Specifies that a visualizer window should be displayed on this operator at workflow start.")]
    public sealed class VisualizerWindow : VisualizerMappingExpressionBuilder, INamedElement
    {
        /// <summary>
        /// Gets or sets the text of the visualizer window title bar.
        /// </summary>
        [Category(nameof(CategoryAttribute.Appearance))]
        [Description("The text of the visualizer window title bar.")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether the visualizer window is visible
        /// on start.
        /// </summary>
        /// <remarks>
        /// If no value is specified, the value in the cached layout settings is used.
        /// </remarks>
        [Description("Specifies whether the visualizer window is visible on start.")]
        public bool? Visible { get; set; } = true;

        /// <summary>
        /// Gets or sets a value specifying the position of the upper-left corner of the
        /// visualizer window, in screen coordinates.
        /// </summary>
        /// <remarks>
        /// If no value is specified, the value in the cached layout settings is used.
        /// </remarks>
        [Category(nameof(CategoryAttribute.Layout))]
        [Description("Specifies the position of the upper-left corner of the visualizer window, in screen coordinates.")]
        public Point? Location { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the size of the visualizer window.
        /// </summary>
        /// <remarks>
        /// If no value is specified, the value in the cached layout settings is used.
        /// </remarks>
        [Category(nameof(CategoryAttribute.Layout))]
        [Description("Specifies the size of the visualizer window.")]
        public Size? Size { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether the visualizer window should start
        /// minimized, maximized, or normal.
        /// </summary>
        /// <remarks>
        /// If no value is specified, the value in the cached layout settings is used.
        /// </remarks>
        [Category(nameof(CategoryAttribute.Layout))]
        [Description("Specifies whether the visualizer window should start minimized, maximized, or normal.")]
        public FormWindowState? WindowState { get; set; }

        string INamedElement.Name => Text;

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
