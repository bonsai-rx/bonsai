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
            context = new WorkflowContext();
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
                for (int i = 0; i < workflow.Components.Count; i++)
                {
                    var component = workflow.Components[i];
                    observableWorkflow.Components.Add(component);

                    var elementControl = workflowLayoutPanel.GetElementFromPosition(i, 0);
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
                observableWorkflow.Unload(context);
            }
        }

        void UpdateWorkflowLayout()
        {
            propertyGrid.SelectedObject = null;
            workflowLayoutPanel.ClearLayout();
            workflowLayoutPanel.SuspendLayout();

            foreach (var element in workflow.Components)
            {
                AddElement(element);
            }

            workflowLayoutPanel.ResumeLayout();
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
            elementControl.Click += delegate { propertyGrid.SelectedObject = element; };

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

            workflowLayoutPanel.AddElement(elementControl);
        }

        private void workflowLayoutPanel_DragDrop(object sender, DragEventArgs e)
        {
            var typeName = e.Data.GetData(DataFormats.Text).ToString();
            var type = Type.GetType(typeName);
            if (type != null && type.IsSubclassOf(typeof(WorkflowElement)))
            {
                var element = (WorkflowElement)Activator.CreateInstance(type);
                var point = workflowLayoutPanel.PointToClient(new Point(e.X, e.Y));
                var targetElement = workflowLayoutPanel.GetChildAtPoint(point) as WorkflowElementControl;
                if (targetElement != null)
                {
                    var targetPosition = workflowLayoutPanel.GetPositionFromElement(targetElement);
                    workflow.Components.Insert(targetPosition.Column, element);
                    UpdateWorkflowLayout();
                }
                else
                {
                    workflow.Components.Add(element);
                    AddElement(element);
                }
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
            StartWorkflow();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopWorkflow();
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            StopWorkflow();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var activeElement = workflowLayoutPanel.ActiveControl as WorkflowElementControl;
            if (activeElement != null)
            {
                var elementPosition = workflowLayoutPanel.GetPositionFromElement(activeElement);
                workflow.Components.RemoveAt(elementPosition.Column);
                UpdateWorkflowLayout();
            }
        }
    }
}
