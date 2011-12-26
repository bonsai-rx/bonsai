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
using System.Reactive.Linq;
using System.Linq.Expressions;
using System.ComponentModel.Design;

namespace Bonsai.Editor
{
    public partial class MainForm : Form
    {
        EditorSite editorSite;
        WorkflowProject project;
        WorkflowContext context;
        IDisposable errorHandler;
        XmlSerializer serializer;
        Dictionary<Type, Type> typeVisualizers;

        public MainForm()
        {
            InitializeComponent();
            InitializeToolbox();

            editorSite = new EditorSite();
            project = new WorkflowProject();
            context = new WorkflowContext();
            editorSite.Context = context;
            typeVisualizers = TypeVisualizerLoader.GetTypeVisualizerDictionary();
            workflowLayoutPanel.Project = project;
            workflowLayoutPanel.Context = context;
            workflowLayoutPanel.PropertyGrid = propertyGrid;
            workflowLayoutPanel.TypeVisualizers = typeVisualizers;
            propertyGrid.Site = editorSite;
        }

        void HandleWorkflowError(Exception e)
        {
            if (project.Running)
            {
                StopWorkflow();
                MessageBox.Show(e.Message, "Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region EditorSite Class

        class EditorSite : ISite
        {
            public WorkflowContext Context { get; set; }

            public IComponent Component
            {
                get { return null; }
            }

            public IContainer Container
            {
                get { return null; }
            }

            public bool DesignMode
            {
                get { return false; }
            }

            public string Name { get; set; }

            public object GetService(Type serviceType)
            {
                if (Context == null) return null;
                else return Context.GetService(serviceType);
            }
        }

        #endregion

        #region Toolbox

        void InitializeToolbox()
        {
            var types = WorkflowElementLoader.GetWorkflowElementTypes();
            serializer = new XmlSerializer(typeof(WorkflowProject));

            InitializeToolboxCategory(toolboxTreeView.Nodes[0], types.Where(type => WorkflowElementControl.MatchGenericType(type, typeof(Source<>))));
            InitializeToolboxCategory(toolboxTreeView.Nodes[1], types
                .Where(type => WorkflowElementControl.MatchGenericType(type, typeof(Filter<,>)))
                .Concat(Enumerable.Repeat(typeof(ParallelFilter<>), 1)));
            InitializeToolboxCategory(toolboxTreeView.Nodes[2], types.Where(type => WorkflowElementControl.MatchGenericType(type, typeof(Sink<>))));
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
            if (!project.Running && e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else e.Effect = DragDropEffects.None;
        }

        void StartWorkflow()
        {
            if (!project.Running)
            {
                project.Visitor((container, index, column, row) =>
                {
                    var elementControl = workflowLayoutPanel.GetElementFromPosition(column, row);
                    if (elementControl != null &&
                        elementControl.ObservableElement != null &&
                        elementControl.ObservableElement != elementControl.Element)
                    {
                        container.Components.Insert(index + 1, elementControl.ObservableElement);
                    }
                });
                errorHandler = project.Error.ObserveOn(this).Subscribe(HandleWorkflowError);
                project.Load(context);
                project.Start();
            }
        }

        void StopWorkflow()
        {
            if (project.Running)
            {
                project.Stop();
                project.Unload(context);
                errorHandler.Dispose();

                project.Visitor((container, index, column, row) =>
                {
                    var isWorkflow = container is Workflow;
                    if (index > 0 &&
                       (isWorkflow && index % 2 == 0 ||
                       (!isWorkflow && index % 2 != 0)))
                    {
                        container.Components.RemoveAt(index);
                    }
                });
            }
        }

        void UpdateWorkflowLayout()
        {
            propertyGrid.SelectedObject = null;
            workflowLayoutPanel.UpdateWorkflowLayout();
        }

        private void workflowLayoutPanel_DragDrop(object sender, DragEventArgs e)
        {
            var typeName = e.Data.GetData(DataFormats.Text).ToString();
            var type = Type.GetType(typeName);
            if (type != null && type.IsSubclassOf(typeof(WorkflowElement)))
            {
                var point = new Point(e.X, e.Y);
                var position = workflowLayoutPanel.GetPositionFromPoint(point);
                var elementControl = workflowLayoutPanel.CreateWorkflowElement(type, point);
                workflowLayoutPanel.AddElement(elementControl, position.Column, position.Row);
            }
        }

        #endregion

        #region File Menu

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopWorkflow();
            project.Workflows.Clear();
            UpdateWorkflowLayout();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openWorkflowDialog.ShowDialog() == DialogResult.OK)
            {
                saveWorkflowDialog.FileName = openWorkflowDialog.FileName;
                using (var reader = XmlReader.Create(openWorkflowDialog.FileName))
                {
                    project = (WorkflowProject)serializer.Deserialize(reader);
                    workflowLayoutPanel.Project = project;
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
                    serializer.Serialize(writer, project);
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
            if (activeElement != null && !project.Running)
            {
                workflowLayoutPanel.RemoveElement(activeElement);
            }
        }
    }
}
