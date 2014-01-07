using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    public class ChartControl : ZedGraphControl
    {
        int colorIndex;
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

        public ChartControl()
        {
            AutoScaleAxis = true;
            Size = new Size(320, 240);
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
        }

        [DefaultValue(true)]
        public bool AutoScaleAxis { get; set; }

        public Color GetNextColor()
        {
            var color = BrightPastelPalette[colorIndex];
            colorIndex = (colorIndex + 1) % BrightPastelPalette.Length;
            return color;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (AutoScaleAxis) GraphPane.AxisChange(e.Graphics);
            base.OnPaint(e);
        }
    }
}
