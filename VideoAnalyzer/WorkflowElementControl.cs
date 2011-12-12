using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VideoAnalyzer
{
    public partial class WorkflowElementControl : UserControl
    {
        public WorkflowElementControl()
        {
            InitializeComponent();
            Font = new Font(FontFamily.GenericMonospace, 8);
        }

        public bool Selected { get; set; }

        public AnchorStyles Connections { get; set; }

        public WorkflowElement Element { get; set; }

        private void WorkflowElementControl_Paint(object sender, PaintEventArgs e)
        {
            const float BorderSize = 5;
            const float ElementOffset = 25;
            var text = Element != null ? Element.GetType().Name.Substring(0, 1) : string.Empty;
            var textSize = e.Graphics.MeasureString(text, Font);

            var width = textSize.Width + 2 * BorderSize;
            var height = textSize.Height + 2 * BorderSize;
            if (Selected)
            {
                e.Graphics.FillRectangle(Brushes.Black, ElementOffset, ElementOffset, width, height);
                e.Graphics.DrawString(text, Font, Brushes.White, new PointF(ElementOffset + BorderSize, ElementOffset + BorderSize));
            }
            else
            {
                e.Graphics.DrawRectangle(Pens.Black, ElementOffset, ElementOffset, width, height);
                e.Graphics.DrawString(text, Font, Brushes.Black, new PointF(ElementOffset + BorderSize, ElementOffset + BorderSize));
            }

            var midX = ElementOffset + textSize.Width / 2f + BorderSize;
            var midY = ElementOffset + textSize.Height / 2f + BorderSize;
            if (Connections.HasFlag(AnchorStyles.Left)) e.Graphics.DrawLine(Pens.Black, 0, midY, ElementOffset, midY);
            if (Connections.HasFlag(AnchorStyles.Right)) e.Graphics.DrawLine(Pens.Black, ElementOffset + width, midY, Size.Width, midY);
            if (Connections.HasFlag(AnchorStyles.Top)) e.Graphics.DrawLine(Pens.Black, midX, 0, midX, ElementOffset);
            if (Connections.HasFlag(AnchorStyles.Bottom)) e.Graphics.DrawLine(Pens.Black, midX, ElementOffset + height, midX, Size.Height);
        }
    }
}
