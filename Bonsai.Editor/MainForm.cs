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
using Bonsai.Design;
using System.Linq.Expressions;

namespace Bonsai.Editor
{
    public partial class MainForm : Form
    {
        Workflow workflow;
        Workflow observableWorkflow;
        WorkflowContext context;
        XmlSerializer serializer;
        Dictionary<Type, Type> typeVisualizers;

        public MainForm()
        {
            InitializeComponent();
            InitializeToolbox();

            workflow = new Workflow();
            observableWorkflow = new Workflow();
            typeVisualizers = TypeVisualizerLoader.GetTypeVisualizerDictionary();
        }

        #region Toolbox

        void InitializeToolbox()
        {
            var types = WorkflowElementLoader.GetWorkflowElementTypes();
            serializer = new XmlSerializer(typeof(Workflow), types);

            InitializeToolboxCategory(toolboxTreeView.Nodes[0], types.Where(type => WorkflowElementLoader.MatchGenericType(type, typeof(Source<>))));
            InitializeToolboxCategory(toolboxTreeView.Nodes[1], types.Where(type => WorkflowElementLoader.MatchGenericType(type, typeof(Filter<,>))));
            InitializeToolboxCategory(toolboxTreeView.Nodes[2], types.Where(type => WorkflowElementLoader.MatchGenericType(type, typeof(Sink<>))));
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

        void StartWorkflow()
        {
            if (!observableWorkflow.Running)
            {
                observableWorkflow.Components.Clear();
                foreach (var component in workflow.Components)
                {
                    observableWorkflow.Components.Add(component);

                    var elementControl = workflowLayoutPanel.Controls.Find(component.GetType().Name, false)
                        .Cast<WorkflowElementControl>()
                        .FirstOrDefault(control => control.Element == component);
                    if (elementControl != null &&
                        elementControl.ObservableElement != null &&
                        elementControl.ObservableElement != component)
                    {
                        observableWorkflow.Components.Add(elementControl.ObservableElement);
                    }
                }

                observableWorkflow.Load(context);
                observableWorkflow.Start();
            }
        }

        void StopWorkflow()
        {
            if (observableWorkflow.Running)
            {
                observableWorkflow.Stop();
                observableWorkflow.Unload();
            }
        }

        void UpdateWorkflowLayout()
        {
            workflowLayoutPanel.SuspendLayout();
            for (int i = workflowLayoutPanel.Controls.Count - 1; i >= 0; i--)
            {
                workflowLayoutPanel.Controls[i].Dispose();
            }
            workflowLayoutPanel.Controls.Clear();
            for (int i = 1; i < workflowLayoutPanel.RowCount - 1; i++)
            {
                workflowLayoutPanel.RowStyles.RemoveAt(1);
            }
            for (int i = 1; i < workflowLayoutPanel.ColumnCount - 1; i++)
            {
                workflowLayoutPanel.ColumnStyles.RemoveAt(1);
            }
            workflowLayoutPanel.RowCount = 2;
            workflowLayoutPanel.ColumnCount = 2;

            foreach (var element in workflow.Components)
            {
                AddElement(element);
            }

            workflowLayoutPanel.ResumeLayout();
        }

        WorkflowElement CreateObservableFilter(WorkflowElement filter)
        {
            var outputType = WorkflowElementControl.GetWorkflowElementOutputType(filter.GetType());
            var observableFilterType = typeof(ObservableFilter<>).MakeGenericType(outputType);
            return (WorkflowElement)Activator.CreateInstance(observableFilterType);
        }

        void CreateVisualizerSource(WorkflowElementControl elementControl, WorkflowElement element)
        {
            Type visualizerType;
            var outputType = WorkflowElementControl.GetWorkflowElementOutputType(element.GetType());
            if (!typeVisualizers.TryGetValue(outputType, out visualizerType))
            {
                visualizerType = typeVisualizers[typeof(object)];
            }

            var visualizer = (DialogTypeVisualizer)Activator.CreateInstance(visualizerType);
            elementControl.SetObservableElement(element, visualizer, context);
        }

        void AddElement(WorkflowElement element)
        {
            var type = element.GetType();
            var elementControl = new WorkflowElementControl();
            elementControl.Name = type.Name;
            elementControl.Element = element;
            elementControl.Dock = DockStyle.Fill;
            elementControl.Click += delegate
            {
                workflowLayoutPanel_Click(this, EventArgs.Empty);
                propertyGrid.SelectedObject = element;
                elementControl.Selected = true;
            };

            if (WorkflowElementLoader.MatchGenericType(type, typeof(Source<>)))
            {
                elementControl.Connections = AnchorStyles.Right;
                CreateVisualizerSource(elementControl, elementControl.Element);
            }

            if (WorkflowElementLoader.MatchGenericType(type, typeof(Filter<,>)))
            {
                elementControl.Connections = AnchorStyles.Left | AnchorStyles.Right;
                CreateVisualizerSource(elementControl, CreateObservableFilter(elementControl.Element));
            }

            if (WorkflowElementLoader.MatchGenericType(type, typeof(Sink<>))) elementControl.Connections = AnchorStyles.Left;

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
            StopWorkflow();
            workflow.Components.Clear();
            UpdateWorkflowLayout();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openWorkflowDialog.ShowDialog() == DialogResult.OK)
            {
                saveWorkflowDialog.FileName = openWorkflowDialog.FileName;
                using (var reader = XmlReader.Create(openWorkflowDialog.FileName))
                {
                    workflow = (Workflow)serializer.Deserialize(reader);
                    UpdateWorkflowLayout();
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
            StartWorkflow();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopWorkflow();
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

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            StopWorkflow();
        }
    }
}
