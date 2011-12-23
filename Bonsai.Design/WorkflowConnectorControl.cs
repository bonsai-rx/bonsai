using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Linq.Expressions;

namespace Bonsai.Design
{
    public partial class WorkflowConnectorControl : UserControl
    {
        public WorkflowConnectorControl()
        {
            InitializeComponent();
            Font = new Font(FontFamily.GenericMonospace, 8);
        }

        public AnchorStyles Connections { get; set; }

        private void WorkflowElementControl_Paint(object sender, PaintEventArgs e)
        {
            const float BorderSize = 5;

            var midX = Size.Width / 2f + BorderSize;
            var midY = Size.Height / 2f + BorderSize + 2;
            if (Connections.HasFlag(AnchorStyles.Left)) e.Graphics.DrawLine(Pens.Black, 0, midY, midX, midY);
            if (Connections.HasFlag(AnchorStyles.Right)) e.Graphics.DrawLine(Pens.Black, midX, midY, Size.Width, midY);
            if (Connections.HasFlag(AnchorStyles.Top)) e.Graphics.DrawLine(Pens.Black, midX, 0, midX, midY);
            if (Connections.HasFlag(AnchorStyles.Bottom)) e.Graphics.DrawLine(Pens.Black, midX, midY, midX, Size.Height);
        }
    }
}
