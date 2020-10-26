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
using System.Xml.Linq;
using System.Xml.Serialization;

[assembly: TypeVisualizer(typeof(MashupVisualizer<>), Target = typeof(VisualizerMashup<TableLayoutPanelVisualizer>))]

namespace Bonsai.Design.Visualizers
{
    public class TableLayoutPanelVisualizer : DialogMashupVisualizer
    {
        internal TableLayoutPanel Panel { get; private set; }

        [XmlAnyElement]
        public XElement[] MashupsXml { get; set; }

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

        public override void Load(IServiceProvider provider)
        {
            Mashups.Deserialize(MashupsXml);
            Panel = new TableLayoutPanel();
            Panel.Dock = DockStyle.Fill;
            Panel.Size = new Size(320, 240);

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(Panel);
            }

            base.Load(provider);
        }

        public override void LoadMashups(IServiceProvider provider)
        {
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var tableLayoutBuilder = (TableLayoutPanelBuilder)ExpressionBuilder.GetVisualizerElement(context.Source).Builder;
            UpdateLayoutPanel(tableLayoutBuilder);
            var container = new TableLayoutPanelContainer(Panel, tableLayoutBuilder.CellSpans, provider);
            base.LoadMashups(container);
        }

        public override void UnloadMashups()
        {
            base.UnloadMashups();
            Panel.Controls.Clear();
            Panel.RowStyles.Clear();
            Panel.ColumnStyles.Clear();
        }

        public override void Show(object value)
        {
        }

        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            return Observable.Merge(Mashups.Select(mashup => mashup.Visualizer.Visualize(mashup.Source, provider)));
        }

        public override void Unload()
        {
            MashupsXml = Mashups.Serialize();
            base.Unload();
            Panel.Dispose();
            Panel = null;
        }

        class TableLayoutPanelContainer : IDialogTypeVisualizerService, IServiceProvider
        {
            public TableLayoutPanelContainer(TableLayoutPanel panel, IReadOnlyList<TableLayoutPanelCellSpan> cellSpans, IServiceProvider provider)
            {
                Panel = panel ?? throw new ArgumentNullException(nameof(panel));
                CellSpans = cellSpans ?? throw new ArgumentNullException(nameof(cellSpans));
                Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            }

            private TableLayoutPanel Panel { get; }

            private IReadOnlyList<TableLayoutPanelCellSpan> CellSpans { get; }

            private IServiceProvider Provider { get; }

            public void AddControl(Control control)
            {
                int column, row;
                var index = Panel.Controls.Count;
                if (Panel.ColumnCount == 0)
                {
                    column = index / Panel.RowCount;
                    row = index % Panel.RowCount;
                }
                else
                {
                    column = index % Panel.ColumnCount;
                    row = index / Panel.ColumnCount;
                }
                Panel.Controls.Add(control, column, row);
                if (index < CellSpans.Count)
                {
                    var cellSpan = CellSpans[index];
                    Panel.SetColumnSpan(control, cellSpan.ColumnSpan);
                    Panel.SetRowSpan(control, cellSpan.RowSpan);
                }
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IDialogTypeVisualizerService))
                {
                    return this;
                }

                return Provider.GetService(serviceType);
            }
        }
    }
}
