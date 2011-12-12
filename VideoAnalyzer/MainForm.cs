using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml;

namespace VideoAnalyzer
{
    public partial class MainForm : Form
    {
        Workflow workflow;
        WorkflowContext context;
        XmlSerializer serializer;

        public MainForm()
        {
            InitializeComponent();
            InitializeToolbox();

            workflow = new Workflow();
            serializer = new XmlSerializer(typeof(Workflow), ReflectionHelper.GetAssemblyTypes().Where(type => type.IsSubclassOf(typeof(WorkflowElement))).ToArray());
        }

        #region Toolbox

        void InitializeToolbox()
        {
            var types = ReflectionHelper.GetAssemblyTypes();
            InitializeToolboxCategory(toolboxTreeView.Nodes[0], types.Where(type => ReflectionHelper.MatchGenericType(type, typeof(Source<>))));
            InitializeToolboxCategory(toolboxTreeView.Nodes[1], types.Where(type => ReflectionHelper.MatchGenericType(type, typeof(Filter<,>))));
            InitializeToolboxCategory(toolboxTreeView.Nodes[2], types.Where(type => ReflectionHelper.MatchGenericType(type, typeof(Sink<>))));
        }

        void InitializeToolboxCategory(TreeNode category, IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                category.Nodes.Add(type.AssemblyQualifiedName, type.Name);
            }
        }

        private void toolboxTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var selectedNode = e.Item as TreeNode;
            if (selectedNode != null)
            {
                toolboxTreeView.DoDragDrop(selectedNode.Name, DragDropEffects.Copy);
            }
        }

        private void workflowLayoutPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else e.Effect = DragDropEffects.None;
        }

        void AddElement(WorkflowElement element)
        {
            var type = element.GetType();
            var elementControl = new WorkflowElementControl();
            elementControl.Element = element;
            elementControl.Dock = DockStyle.Fill;
            elementControl.Click += delegate
            {
                workflowLayoutPanel_Click(this, EventArgs.Empty);
                propertyGrid.SelectedObject = element;
                elementControl.Selected = true;
            };

            if (ReflectionHelper.MatchGenericType(type, typeof(Source<>))) elementControl.Connections = AnchorStyles.Right;
            if (ReflectionHelper.MatchGenericType(type, typeof(Filter<,>))) elementControl.Connections = AnchorStyles.Left | AnchorStyles.Right;
            if (ReflectionHelper.MatchGenericType(type, typeof(Sink<>))) elementControl.Connections = AnchorStyles.Left;

            if (workflowLayoutPanel.GetControlFromPosition(0, 0) == null)
            {
                workflowLayoutPanel.Controls.Add(elementControl, 0, 0);
            }
            else if (elementControl.Connections == AnchorStyles.Right)
            {
                workflowLayoutPanel.RowCount++;
                workflowLayoutPanel.Controls.Add(elementControl, 0, workflowLayoutPanel.RowCount - 2);
            }
            else if (elementControl.Connections.HasFlag(AnchorStyles.Left))
            {
                var row = 0;
                var columnStyle = workflowLayoutPanel.ColumnStyles[0];

                workflowLayoutPanel.ColumnCount++;
                workflowLayoutPanel.Controls.Add(elementControl, workflowLayoutPanel.ColumnCount - 2, row);
                workflowLayoutPanel.ColumnStyles.Insert(workflowLayoutPanel.ColumnStyles.Count - 1, new ColumnStyle(columnStyle.SizeType, columnStyle.Width));
            }
        }

        private void workflowLayoutPanel_DragDrop(object sender, DragEventArgs e)
        {
            var typeName = e.Data.GetData(DataFormats.Text).ToString();
            var type = Type.GetType(typeName);
            if (type != null && type.IsSubclassOf(typeof(WorkflowElement)))
            {
                var element = (WorkflowElement)Activator.CreateInstance(type);
                workflow.Components.Add(element);
                AddElement(element);
            }
        }

        #endregion

        #region File Menu

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openWorkflowDialog.ShowDialog() == DialogResult.OK)
            {
                saveWorkflowDialog.FileName = openWorkflowDialog.FileName;
                using (var reader = XmlReader.Create(openWorkflowDialog.FileName))
                {
                    workflow = (Workflow)serializer.Deserialize(reader);
                    foreach (var element in workflow.Components)
                    {
                        AddElement(element);
                    }
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(saveWorkflowDialog.FileName)) saveAsToolStripMenuItem_Click(this, e);
            else
            {
                using (var writer = XmlWriter.Create(saveWorkflowDialog.FileName, new XmlWriterSettings { Indent = true }))
                {
                    serializer.Serialize(writer, workflow);
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveWorkflowDialog.ShowDialog() == DialogResult.OK)
            {
                saveToolStripMenuItem_Click(this, e);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            context = new WorkflowContext();
            workflow.Load(context);
            workflow.Start();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            workflow.Stop();
            workflow.Unload();
            context.Dispose();
            context = null;
        }

        private void workflowLayoutPanel_Click(object sender, EventArgs e)
        {
            propertyGrid.SelectedObject = null;
            foreach (WorkflowElementControl control in workflowLayoutPanel.Controls)
            {
                control.Selected = false;
                control.Invalidate();
            }
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
        }
    }
}
