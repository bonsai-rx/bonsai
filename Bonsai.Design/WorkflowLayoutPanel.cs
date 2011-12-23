using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public partial class WorkflowLayoutPanel : UserControl
    {
        public WorkflowLayoutPanel()
        {
            InitializeComponent();
        }

        public WorkflowProject Project { get; set; }

        public WorkflowContext Context { get; set; }

        public PropertyGrid PropertyGrid { get; set; }

        public Dictionary<Type, Type> TypeVisualizers { get; set; }

        #region CreateWorkflowElement Methods

        public WorkflowElementControl CreateWorkflowElement(Type type, Point point)
        {
            point = tableLayoutPanel.PointToClient(point);
            type = GetInstanceElementType(type, point);

            var element = (WorkflowElement)Activator.CreateInstance(type);
            return CreateWorkflowElement(element);
        }

        public WorkflowElementControl CreateWorkflowElement(WorkflowElement element)
        {
            var elementControl = WorkflowElementControl.FromWorkflowElement(element);
            if (PropertyGrid != null) elementControl.Click += delegate { PropertyGrid.SelectedObject = element; };

            if (elementControl.Connections == AnchorStyles.Right) CreateVisualizerSource(elementControl, elementControl.Element);
            else if (elementControl.Connections.HasFlag(AnchorStyles.Right))
            {
                CreateVisualizerSource(elementControl, CreateObservableFilter(elementControl.Element));
            }

            return elementControl;
        }

        WorkflowElement CreateObservableFilter(WorkflowElement filter)
        {
            var outputType = WorkflowElementControl.GetWorkflowElementOutputType(filter);
            var observableFilterType = typeof(ObservableFilter<>).MakeGenericType(outputType);
            return (WorkflowElement)Activator.CreateInstance(observableFilterType);
        }

        void CreateVisualizerSource(WorkflowElementControl elementControl, WorkflowElement element)
        {
            Type visualizerType;
            var outputType = WorkflowElementControl.GetWorkflowElementOutputType(element);
            if (!TypeVisualizers.TryGetValue(outputType, out visualizerType))
            {
                visualizerType = TypeVisualizers[typeof(object)];
            }

            var visualizer = (DialogTypeVisualizer)Activator.CreateInstance(visualizerType);
            elementControl.SetObservableElement(element, visualizer, Context);
        }

        Type GetInstanceElementType(Type type, Point point)
        {
            //TODO: Use the cell position here and make sure type inference is always attempted on the parent element
            if (type.ContainsGenericParameters)
            {
                var targetElement = tableLayoutPanel.GetChildAtPoint(point) as WorkflowElementControl;
                if (targetElement == null) throw new ArgumentException("Failed type inference on generic component.", "type");

                var targetOutputType = WorkflowElementControl.GetWorkflowElementOutputType(targetElement.Element);
                type = type.MakeGenericType(targetOutputType);
            }

            return type;
        }

        #endregion

        #region UpdateWorkflowLayout Methods

        public void UpdateWorkflowLayout()
        {
            ClearLayout();

            foreach (var workflow in Project.Workflows)
            {
                var row = tableLayoutPanel.RowCount <= 2 ? 0 : tableLayoutPanel.RowCount - 1;
                AddContainerElements(workflow, 0, row);
            }
        }

        void AddContainerElements(IWorkflowContainer container, int column, int row)
        {
            foreach (var component in container.Components)
            {
                var elementControl = CreateWorkflowElement(component);
                if (elementControl.Connections == AnchorStyles.Right) elementControl.Tag = container;
                LayoutElement(elementControl, column++, row);

                var childContainer = component as IWorkflowContainer;
                if (childContainer != null)
                {
                    AddContainerElements(childContainer, column, row + 1);
                }
            }
        }

        #endregion

        #region AddElement Methods

        public void AddElement(WorkflowElementControl elementControl, int column, int row)
        {
            Tuple<IWorkflowContainer, TableLayoutPanelCellPosition> containerInfo;
            if (elementControl.Connections == AnchorStyles.Right)
            {
                // Create a new workflow if the element is a source
                var workflow = new Workflow();
                elementControl.Tag = workflow;
                Project.Workflows.Add(workflow);

                // Do not expand row layout if this is the first workflow
                if (Project.Workflows.Count > 1)
                {
                    containerInfo = Tuple.Create((IWorkflowContainer)workflow, new TableLayoutPanelCellPosition(0, tableLayoutPanel.RowCount - 1));
                }
                else containerInfo = Tuple.Create((IWorkflowContainer)workflow, new TableLayoutPanelCellPosition(0, 0));
            }
            // Retrieve existing workflow otherwise
            else containerInfo = GetContainerFromPosition(column, row);

            // Get the container and the column offset
            var container = containerInfo.Item1;
            var containerOffset = containerInfo.Item2;

            // Compute the index in the container after which the element will be inserted
            int containerIndex;
            if (container.Components.Count == 0) containerIndex = -1;
            else containerIndex = Math.Max(containerOffset.Column == 0 ? 0 : -1, Math.Min(container.Components.Count - 1, column - containerOffset.Column));
            containerIndex++;

            // Insert the element into project and layout structure
            container.Components.Insert(containerIndex, elementControl.Element);
            LayoutElement(elementControl, containerOffset.Column + containerIndex, containerOffset.Row);
        }

        public void LayoutElement(WorkflowElementControl elementControl, int column, int row)
        {
            tableLayoutPanel.SuspendLayout();

            // Insert the element into the workflow and add container expansion if needed
            InsertElementControl(elementControl, column, row);
            if (elementControl.Connections.HasFlag(AnchorStyles.Bottom))
            {
                InsertContainerControl(column, row);
            }

            tableLayoutPanel.ResumeLayout();
        }

        void InsertContainerControl(int column, int row)
        {
            // Grow layout rows
            ExpandLayoutRows();

            // Compute the row where the container elements will be inserted
            var containerRow = row + GetWorkflowHeight(column + 1, row) + 1;

            // Shift container rows down if needed
            for (int i = 0; i < tableLayoutPanel.ColumnCount; i++)
            {
                var control = tableLayoutPanel.GetControlFromPosition(i, containerRow);
                if (control != null)
                {
                    ShiftElementsDown(containerRow);
                    break;
                }
            }

            // Add downward connectors
            for (int j = row + 1; j < containerRow; j++)
            {
                var connector = new WorkflowConnectorControl();
                connector.Connections = AnchorStyles.Top | AnchorStyles.Bottom;
                tableLayoutPanel.Controls.Add(connector, column, j);
            }

            // Add connector right turn
            var connectorTurn = new WorkflowConnectorControl();
            connectorTurn.Connections = AnchorStyles.Top | AnchorStyles.Right;
            tableLayoutPanel.Controls.Add(connectorTurn, column, containerRow);
        }

        void ShiftElementsDown(int row)
        {
            // Shift all controls in 'row' downward
            for (int j = tableLayoutPanel.RowCount - 3; j >= row; j--)
            {
                for (int i = 0; i < tableLayoutPanel.ColumnCount; i++)
                {
                    var control = tableLayoutPanel.GetControlFromPosition(i, j);
                    if (control != null)
                    {
                        tableLayoutPanel.Controls.Remove(control);
                        tableLayoutPanel.Controls.Add(control, i, j + 1);

                        // Add extension connector if needed
                        var connector = control as WorkflowConnectorControl;
                        if (connector != null && j == row)
                        {
                            var extender = new WorkflowConnectorControl();
                            extender.Connections = AnchorStyles.Top | AnchorStyles.Bottom;
                            tableLayoutPanel.Controls.Add(extender, i, j);
                        }
                    }
                }
            }
        }

        void ShiftElementsRight(int column, int row)
        {
            // Grow layout columns if needed
            if (column == tableLayoutPanel.ColumnCount - 1 || tableLayoutPanel.GetControlFromPosition(tableLayoutPanel.ColumnCount - 2, row) != null)
            {
                ExpandLayoutColumns();
            }

            // Shift subsequent controls to the right
            for (int i = tableLayoutPanel.ColumnCount - 3; i >= column; i--)
            {
                var control = tableLayoutPanel.GetControlFromPosition(i, row);
                if (control != null)
                {
                    tableLayoutPanel.Controls.Remove(control);
                    tableLayoutPanel.Controls.Add(control, i + 1, row);

                    // If a container element is found, shift its entire row to the right
                    var elementControl = control as WorkflowElementControl;
                    if (elementControl != null && elementControl.Connections.HasFlag(AnchorStyles.Bottom))
                    {
                        // Travel down the container branch, shifting all connectors
                        int connectorRow = row;
                        WorkflowConnectorControl connector;
                        while ((connector = (WorkflowConnectorControl)tableLayoutPanel.GetControlFromPosition(i, ++connectorRow))
                                .Connections.HasFlag(AnchorStyles.Bottom))
                        {
                            tableLayoutPanel.Controls.Remove(connector);
                            tableLayoutPanel.Controls.Add(connector, i + 1, connectorRow);
                        }

                        // Shift all container elements
                        ShiftElementsRight(i, connectorRow);
                    }
                }
            }
        }

        void InsertElementControl(WorkflowElementControl elementControl, int column, int row)
        {
            // Expand rows if the control is a new workflow source
            if (elementControl.Connections == AnchorStyles.Right && row > 0) ExpandLayoutRows();

            // Shift workflow elements right
            ShiftElementsRight(column, row);
            tableLayoutPanel.Controls.Add(elementControl, column, row);
        }

        void ExpandLayoutColumns()
        {
            var columnStyle = tableLayoutPanel.ColumnStyles[0];
            tableLayoutPanel.ColumnCount++;
            tableLayoutPanel.ColumnStyles.Insert(tableLayoutPanel.ColumnStyles.Count - 1, new ColumnStyle(columnStyle.SizeType, columnStyle.Width));
        }

        void ExpandLayoutRows()
        {
            var rowStyle = tableLayoutPanel.RowStyles[0];
            tableLayoutPanel.RowCount++;
            tableLayoutPanel.RowStyles.Insert(tableLayoutPanel.RowStyles.Count - 1, new RowStyle(rowStyle.SizeType, rowStyle.Height));
        }

        #endregion

        #region RemoveElement Methods

        public void RemoveElement(WorkflowElementControl elementControl)
        {
            var position = tableLayoutPanel.GetPositionFromControl(elementControl);

            // Retrieve the container where the element is inserted
            var containerInfo = GetContainerFromPosition(position.Column, position.Row);
            var container = containerInfo.Item1;
            var containerOffset = containerInfo.Item2;

            // Compute the index in the container where the element is stored
            var containerIndex = position.Column - containerOffset.Column;

            // Remove control from layout structure
            tableLayoutPanel.AutoScroll = false;
            tableLayoutPanel.SuspendLayout();

            // Recursively remove container layouts
            if (elementControl.Connections == AnchorStyles.Right ||
                elementControl.Connections.HasFlag(AnchorStyles.Bottom))
            {
                RemoveContainerControl(position.Column, position.Row);
            }
            // If element is a source, it has already been removed
            if (elementControl.Connections != AnchorStyles.Right)
            {
                RemoveElementControl(elementControl, position.Column, position.Row);
            }

            tableLayoutPanel.ResumeLayout();
            tableLayoutPanel.AutoScroll = true;

            if (elementControl.Connections == AnchorStyles.Right)
            {
                // Remove entire workflow if element is the source
                var workflow = (Workflow)elementControl.Tag;
                Project.Workflows.Remove(workflow);
            }
            container.Components.RemoveAt(containerIndex);
        }

        void RemoveContainerControl(int column, int row)
        {
            // Remove downward connectors
            WorkflowConnectorControl connector;
            while ((connector = tableLayoutPanel.GetControlFromPosition(column, ++row) as WorkflowConnectorControl) != null &&
                   connector.Connections.HasFlag(AnchorStyles.Bottom))
            {
                tableLayoutPanel.Controls.Remove(connector);
                connector.Dispose();
            }

            // Remove all controls in container line
            if (connector == null) row--;
            for (int i = 0; i < tableLayoutPanel.ColumnCount; i++)
            {
                var control = tableLayoutPanel.GetControlFromPosition(i, row);
                if (control != null)
                {
                    tableLayoutPanel.Controls.Remove(control);
                    if (control is WorkflowConnectorControl) control.Dispose();

                    // Recursively go down all containers and remove them as well
                    var container = control as WorkflowElementControl;
                    if (container != null && container.Connections.HasFlag(AnchorStyles.Bottom))
                    {
                        RemoveContainerControl(i, row);
                    }
                }
            }

            // Shift workflow elements up
            ShiftElementsUp(row);

            // Retract layout rows
            if (tableLayoutPanel.RowCount > 2) RetractLayoutRows();

            // Retract layout columns if last column is free
            if (tableLayoutPanel.ColumnCount > 2)
            {
                for (int j = 0; j < tableLayoutPanel.RowCount; j++)
                {
                    var control = tableLayoutPanel.GetControlFromPosition(tableLayoutPanel.ColumnCount - 2, j);
                    if (control != null) return;
                }
                RetractLayoutColumns();
            }
        }

        void RemoveElementControl(WorkflowElementControl elementControl, int column, int row)
        {
            // Remove and shift workflow elements left
            tableLayoutPanel.Controls.Remove(elementControl);
            ShiftElementsLeft(column + 1, row);
        }

        void ShiftElementsUp(int row)
        {
            // Shift all controls in 'row' upward
            for (int j = row; j < tableLayoutPanel.RowCount; j++)
            {
                for (int i = 0; i < tableLayoutPanel.ColumnCount; i++)
                {
                    var control = tableLayoutPanel.GetControlFromPosition(i, j);
                    if (control != null)
                    {
                        tableLayoutPanel.Controls.Remove(control);
                        tableLayoutPanel.Controls.Add(control, i, j - 1);
                    }
                }
            }
        }

        void ShiftElementsLeft(int column, int row)
        {
            // Shift subsequent controls to the left
            for (int i = column; i < tableLayoutPanel.ColumnCount; i++)
            {
                var control = tableLayoutPanel.GetControlFromPosition(i, row);
                if (control != null)
                {
                    tableLayoutPanel.Controls.Remove(control);
                    tableLayoutPanel.Controls.Add(control, i - 1, row);

                    // If a container element is found, shift its entire row to the left
                    var elementControl = control as WorkflowElementControl;
                    if (elementControl != null && elementControl.Connections.HasFlag(AnchorStyles.Bottom))
                    {
                        // Travel down the container branch, shifting all connectors
                        int connectorRow = row;
                        WorkflowConnectorControl connector;
                        while ((connector = (WorkflowConnectorControl)tableLayoutPanel.GetControlFromPosition(i, ++connectorRow))
                                .Connections.HasFlag(AnchorStyles.Bottom))
                        {
                            tableLayoutPanel.Controls.Remove(connector);
                            tableLayoutPanel.Controls.Add(connector, i - 1, connectorRow);
                        }

                        // Shift all container elements
                        ShiftElementsLeft(i, connectorRow);
                    }
                }
            }

            // Retract layout columns if possible
            if (column == tableLayoutPanel.ColumnCount - 2 || column > 1 && tableLayoutPanel.GetControlFromPosition(tableLayoutPanel.ColumnCount - 3, row) != null)
            {
                // Retract only if whole column is free
                for (int j = 0; j < tableLayoutPanel.RowCount; j++)
                {
                    var control = tableLayoutPanel.GetControlFromPosition(tableLayoutPanel.ColumnCount - 2, j);
                    if (control != null) return;
                }

                RetractLayoutColumns();
            }
        }

        void RetractLayoutColumns()
        {
            tableLayoutPanel.ColumnStyles.RemoveAt(tableLayoutPanel.ColumnCount - 2);
            tableLayoutPanel.ColumnCount--;
        }

        void RetractLayoutRows()
        {
            tableLayoutPanel.RowStyles.RemoveAt(tableLayoutPanel.RowCount - 2);
            tableLayoutPanel.RowCount--;
        }

        #endregion

        int GetWorkflowHeight(int column, int row)
        {
            int height = 0;
            // The height of the workflow is the maximum height of its container branches
            for (int i = column; i < tableLayoutPanel.ColumnCount; i++)
            {
                // The leftmost container is the one with the maximum height so we check only the first one
                var control = (WorkflowElementControl)tableLayoutPanel.GetControlFromPosition(i, row);
                if (control != null && control.Connections.HasFlag(AnchorStyles.Bottom))
                {
                    // Count the height of the leftmost container branch
                    int connectorRow = row;
                    WorkflowConnectorControl connector;
                    do
                    {
                        connector = (WorkflowConnectorControl)tableLayoutPanel.GetControlFromPosition(i, ++connectorRow);
                    }
                    while (connector.Connections.HasFlag(AnchorStyles.Bottom));

                    // Sum the height of the branch to the height of the container workflow
                    height += (connectorRow - row) + GetWorkflowHeight(i + 1, connectorRow);
                    break;
                }
            }
            return height;
        }

        Tuple<IWorkflowContainer, TableLayoutPanelCellPosition> GetContainerFromPosition(int column, int row)
        {
            for (int i = 0; i < tableLayoutPanel.ColumnCount; i++)
            {
                // Check if there is a container branch on this row
                var connector = tableLayoutPanel.GetControlFromPosition(i, row) as WorkflowConnectorControl;
                if (connector != null && connector.Connections == (AnchorStyles.Top | AnchorStyles.Right))
                {
                    for (int j = row - 1; j >= 0; j--)
                    {
                        var container = tableLayoutPanel.GetControlFromPosition(i, j) as WorkflowElementControl;
                        if (container != null)
                        {
                            // The offset of the container is one unit to the right of the container control
                            return Tuple.Create((IWorkflowContainer)container.Element, new TableLayoutPanelCellPosition(i + 1, row));
                        }
                    }
                }
            }

            // Workflows are stored in the Tag property of source element controls and their offset is always zero
            return Tuple.Create((IWorkflowContainer)tableLayoutPanel.GetControlFromPosition(0, row).Tag, new TableLayoutPanelCellPosition(0, row));
        }

        public WorkflowElementControl GetElementFromPosition(int column, int row)
        {
            return tableLayoutPanel.GetControlFromPosition(column, row) as WorkflowElementControl;
        }

        public TableLayoutPanelCellPosition GetPositionFromElement(WorkflowElementControl elementControl)
        {
            return tableLayoutPanel.GetPositionFromControl(elementControl);
        }

        public void AddElement(Control elementControl, TableLayoutPanelCellPosition position)
        {
            if (position.Column == tableLayoutPanel.ColumnCount - 1)
            {
                var columnStyle = tableLayoutPanel.ColumnStyles[0];

                tableLayoutPanel.ColumnCount++;
                tableLayoutPanel.ColumnStyles.Insert(tableLayoutPanel.ColumnStyles.Count - 1, new ColumnStyle(columnStyle.SizeType, columnStyle.Width));
            }

            if (position.Row == tableLayoutPanel.RowCount - 1)
            {
                var rowStyle = tableLayoutPanel.RowStyles[0];

                tableLayoutPanel.RowCount++;
                tableLayoutPanel.RowStyles.Insert(tableLayoutPanel.RowStyles.Count - 1, new RowStyle(rowStyle.SizeType, rowStyle.Height));
            }

            tableLayoutPanel.Controls.Add(elementControl, position.Column, position.Row);
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

        public TableLayoutPanelCellPosition GetPositionFromPoint(Point point)
        {
            // Convert point to client space
            point = tableLayoutPanel.PointToClient(point);

            //Cell position
            TableLayoutPanelCellPosition pos = new TableLayoutPanelCellPosition(0, 0);
            //Panel size.
            Size size = tableLayoutPanel.Size;
            //average cell size.
            SizeF cellAutoSize = new SizeF(size.Width / tableLayoutPanel.ColumnCount, size.Height / tableLayoutPanel.RowCount);

            //Get the cell row.
            //y coordinate
            float y = -tableLayoutPanel.VerticalScroll.Value;
            for (int i = 0; i < tableLayoutPanel.RowCount; i++)
            {
                //Calculate the summary of the row heights.
                SizeType type = tableLayoutPanel.RowStyles[i].SizeType;
                float height = tableLayoutPanel.RowStyles[i].Height;
                switch (type)
                {
                    case SizeType.Absolute:
                        y += height;
                        break;
                    case SizeType.Percent:
                        y += height / 100 * size.Height;
                        break;
                    case SizeType.AutoSize:
                        y += cellAutoSize.Height;
                        break;
                }
                //Check the mouse position to decide if the cell is in current row.
                if ((int)y > point.Y)
                {
                    pos.Row = i;
                    break;
                }
            }

            //Get the cell column.
            //x coordinate
            float x = -tableLayoutPanel.HorizontalScroll.Value;
            for (int i = 0; i < tableLayoutPanel.ColumnCount; i++)
            {
                //Calculate the summary of the row widths.
                SizeType type = tableLayoutPanel.ColumnStyles[i].SizeType;
                float width = tableLayoutPanel.ColumnStyles[i].Width;
                switch (type)
                {
                    case SizeType.Absolute:
                        x += width;
                        break;
                    case SizeType.Percent:
                        x += width / 100 * size.Width;
                        break;
                    case SizeType.AutoSize:
                        x += cellAutoSize.Width;
                        break;
                }
                //Check the mouse position to decide if the cell is in current column.
                if ((int)x > point.X)
                {
                    pos.Column = i;
                    break;
                }
            }

            //return the mouse position.
            if (pos.Column == tableLayoutPanel.ColumnCount - 1) pos.Column--;
            if (pos.Row == tableLayoutPanel.RowCount - 1) pos.Row--;
            return pos;
        }
    }
}
