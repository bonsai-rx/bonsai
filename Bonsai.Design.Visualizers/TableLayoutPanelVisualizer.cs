using Bonsai;
using Bonsai.Design;
using Bonsai.Design.Visualizers;
using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

[assembly: TypeVisualizer(typeof(MashupVisualizerAdapter), Target = typeof(VisualizerMashup<TableLayoutPanelVisualizer>))]

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Provides a type visualizer that can be used to arrange other visualizers in a grid.
    /// </summary>
    public class TableLayoutPanelVisualizer : MashupVisualizerContainer
    {
        internal TableLayoutPanel Panel { get; private set; }

        static void SetStyles(TableLayoutStyleCollection styles, IReadOnlyList<TableLayoutStyle> builderStyles, int count, Func<TableLayoutStyle> defaultStyle)
        {
            if (builderStyles.Count > 0)
            {
                foreach (var style in builderStyles)
                {
                    styles.Add(style);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    styles.Add(defaultStyle());
                }
            }
        }

        void UpdateLayoutPanel(TableLayoutPanelBuilder tableLayoutBuilder)
        {
            var columnCount = tableLayoutBuilder.ColumnCount;
            var rowCount = tableLayoutBuilder.RowCount;
            if (columnCount == 0 && rowCount == 0)
            {
                throw new InvalidOperationException("The table layout must have at least one non-zero dimension.");
            }
            if (columnCount == 0) columnCount = Mashups.Count / rowCount;
            if (rowCount == 0) rowCount = Mashups.Count / columnCount;

            Panel.ColumnCount = columnCount;
            Panel.RowCount = rowCount;
            SetStyles(Panel.ColumnStyles, tableLayoutBuilder.ColumnStyles, columnCount, () => new ColumnStyle(SizeType.Percent, 100f / columnCount));
            SetStyles(Panel.RowStyles, tableLayoutBuilder.RowStyles, rowCount, () => new RowStyle(SizeType.Percent, 100f / rowCount));
        }

        /// <inheritdoc/>
        public override MashupTypeVisualizer GetMashupAtPoint(int x, int y)
        {
            if (Panel == null) return null;
            var panelPoint = Panel.PointToClient(new Point(x, y));
            var childControl = Panel.GetChildAtPoint(panelPoint);
            if (childControl != null)
            {
                var index = Panel.Controls.GetChildIndex(childControl);
                return Mashups[index].Visualizer;
            }

            return null;
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            Panel = new TableLayoutPanel();
            Panel.Dock = DockStyle.Fill;
            Panel.Size = new Size(320, 240);
            base.Load(provider);

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(Panel);
            }
        }

        /// <inheritdoc/>
        public override void LoadMashups(IServiceProvider provider)
        {
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var tableLayoutBuilder = (TableLayoutPanelBuilder)ExpressionBuilder.GetVisualizerElement(context.Source).Builder;
            UpdateLayoutPanel(tableLayoutBuilder);
            var container = new TableLayoutPanelContainer(this, tableLayoutBuilder.CellSpans, provider);
            foreach (var mashup in Mashups)
            {
                mashup.Visualizer.Load(container);
            }
        }

        /// <inheritdoc/>
        public override void UnloadMashups()
        {
            base.UnloadMashups();
            Panel.Controls.Clear();
            Panel.RowStyles.Clear();
            Panel.ColumnStyles.Clear();
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
        }

        /// <inheritdoc/>
        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            return Observable.Merge(Mashups.Select(mashup => mashup.Visualizer.Visualize(((ITypeVisualizerContext)mashup).Source.Output, provider)));
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            base.Unload();
            Panel.Dispose();
            Panel = null;
        }

        class TableLayoutPanelContainer : IDialogTypeVisualizerService, IServiceProvider
        {
            public TableLayoutPanelContainer(TableLayoutPanelVisualizer visualizer, IReadOnlyList<TableLayoutPanelCellSpan> cellSpans, IServiceProvider provider)
            {
                Visualizer = visualizer ?? throw new ArgumentNullException(nameof(visualizer));
                CellSpans = cellSpans ?? throw new ArgumentNullException(nameof(cellSpans));
                Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            }

            private TableLayoutPanelVisualizer Visualizer { get; }

            private IReadOnlyList<TableLayoutPanelCellSpan> CellSpans { get; }

            private IServiceProvider Provider { get; }

            public void AddControl(Control control)
            {
                int column, row;
                var panel = Visualizer.Panel;
                var index = panel.Controls.Count;
                if (panel.ColumnCount == 0)
                {
                    column = index / panel.RowCount;
                    row = index % panel.RowCount;
                }
                else
                {
                    column = index % panel.ColumnCount;
                    row = index / panel.ColumnCount;
                }
                panel.Controls.Add(control, column, row);
                if (index < CellSpans.Count)
                {
                    var cellSpan = CellSpans[index];
                    panel.SetColumnSpan(control, cellSpan.ColumnSpan);
                    panel.SetRowSpan(control, cellSpan.RowSpan);
                }
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IDialogTypeVisualizerService))
                {
                    return this;
                }

                if (serviceType == typeof(DialogMashupVisualizer))
                {
                    return Visualizer;
                }

                if (serviceType == typeof(ITypeVisualizerContext))
                {
                    var index = Visualizer.Panel.Controls.Count;
                    return Visualizer.Mashups[index];
                }

                return Provider.GetService(serviceType);
            }
        }
    }
}
