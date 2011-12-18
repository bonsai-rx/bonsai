using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Bonsai.Design;

namespace Bonsai.Editor
{
    public partial class WorkflowLayoutPanel : UserControl
    {
        public WorkflowLayoutPanel()
        {
            InitializeComponent();
        }

        public new Control GetChildAtPoint(Point point)
        {
            return tableLayoutPanel.GetChildAtPoint(point);
        }

        public WorkflowElementControl GetElementFromPosition(int column, int row)
        {
            return tableLayoutPanel.GetControlFromPosition(column, row) as WorkflowElementControl;
        }

        public TableLayoutPanelCellPosition GetPositionFromElement(WorkflowElementControl elementControl)
        {
            return tableLayoutPanel.GetPositionFromControl(elementControl);
        }

        public void AddElement(WorkflowElementControl elementControl)
        {
            if (tableLayoutPanel.GetControlFromPosition(0, 0) == null)
            {
                tableLayoutPanel.Controls.Add(elementControl, 0, 0);
            }
            else if (elementControl.Connections == AnchorStyles.Right)
            {
                tableLayoutPanel.RowCount++;
                tableLayoutPanel.Controls.Add(elementControl, 0, tableLayoutPanel.RowCount - 2);
            }
            else if (elementControl.Connections.HasFlag(AnchorStyles.Left))
            {
                var row = 0;
                var columnStyle = tableLayoutPanel.ColumnStyles[0];

                tableLayoutPanel.ColumnCount++;
                tableLayoutPanel.Controls.Add(elementControl, tableLayoutPanel.ColumnCount - 2, row);
                tableLayoutPanel.ColumnStyles.Insert(tableLayoutPanel.ColumnStyles.Count - 1, new ColumnStyle(columnStyle.SizeType, columnStyle.Width));
            }
        }

        public void ClearLayout()
        {
            tableLayoutPanel.SuspendLayout();

            for (int i = tableLayoutPanel.Controls.Count - 1; i >= 0; i--)
            {
                tableLayoutPanel.Controls[i].Dispose();
            }
            tableLayoutPanel.Controls.Clear();

            for (int i = 1; i < tableLayoutPanel.RowCount - 1; i++)
            {
                tableLayoutPanel.RowStyles.RemoveAt(1);
            }

            for (int i = 1; i < tableLayoutPanel.ColumnCount - 1; i++)
            {
                tableLayoutPanel.ColumnStyles.RemoveAt(1);
            }

            tableLayoutPanel.RowCount = 2;
            tableLayoutPanel.ColumnCount = 2;

            tableLayoutPanel.ResumeLayout();
        }
    }
}
