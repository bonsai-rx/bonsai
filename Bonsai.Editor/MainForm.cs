using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Design;
using System.Xml.Serialization;
using System.Xml;
using Bonsai.Expressions;
using Bonsai.Dag;
using Bonsai.Design;
using System.Reactive.Disposables;
using System.Linq.Expressions;

namespace Bonsai.Editor
{
    public partial class MainForm : Form
    {
        const int CtrlModifier = 0x8;

        EditorSite editorSite;
        WorkflowBuilder workflowBuilder;
        XmlSerializer serializer;
        Dictionary<Type, Type> typeVisualizers;
        ExpressionBuilderGraph runningWorkflow;
        Dictionary<GraphNode, Action> inspectorMapping;
        ExpressionBuilderTypeConverter builderConverter;

        IDisposable loaded;
        IDisposable running;

        public MainForm()
        {
            InitializeComponent();
            InitializeToolbox();

            editorSite = new EditorSite(this);
            workflowBuilder = new WorkflowBuilder();
            serializer = new XmlSerializer(typeof(WorkflowBuilder));
            typeVisualizers = TypeVisualizerLoader.GetTypeVisualizerDictionary();
            builderConverter = new ExpressionBuilderTypeConverter();
            propertyGrid.Site = editorSite;
        }

        #region Toolbox

        void InitializeToolbox()
        {
            var types = WorkflowElementLoader.GetWorkflowElementTypes();

            InitializeToolboxCategory(toolboxTreeView.Nodes["Source"], types.Where(type => LoadableElementType.MatchGenericType(type, typeof(Source<>))));
            InitializeToolboxCategory(toolboxTreeView.Nodes["Filter"], types.Where(type => LoadableElementType.MatchGenericType(type, typeof(Filter<>))));
            InitializeToolboxCategory(toolboxTreeView.Nodes["Projection"], types.Where(type => LoadableElementType.MatchGenericType(type, typeof(Projection<,>))));
            InitializeToolboxCategory(toolboxTreeView.Nodes["Sink"], types.Where(type => LoadableElementType.MatchGenericType(type, typeof(Sink<>))));
            InitializeToolboxCategory(toolboxTreeView.Nodes["Combinator"], new[] { typeof(TimestampBuilder), typeof(SkipUntilBuilder) });
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

        private void workflowGraphView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else e.Effect = DragDropEffects.None;
        }

        private void workflowGraphView_DragDrop(object sender, DragEventArgs e)
        {
            var typeName = e.Data.GetData(DataFormats.Text).ToString();
            var dropLocation = workflowGraphView.PointToClient(new Point(e.X, e.Y));
            var closestGraphViewNode = workflowGraphView.GetClosestNodeTo(dropLocation);
            CreateGraphNode(typeName, closestGraphViewNode, (e.KeyState & CtrlModifier) != 0);
        }

        void UpdateGraphLayout()
        {
            workflowGraphView.Model = workflowBuilder.Workflow.LongestPathLayering().AverageMinimizeCrossings();
            workflowGraphView.Invalidate();
        }

        #endregion

        #region File Menu

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            workflowBuilder.Workflow.Clear();
            UpdateGraphLayout();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openWorkflowDialog.ShowDialog() == DialogResult.OK)
            {
                saveWorkflowDialog.FileName = openWorkflowDialog.FileName;
                using (var reader = XmlReader.Create(openWorkflowDialog.FileName))
                {
                    workflowBuilder = (WorkflowBuilder)serializer.Deserialize(reader);
                    UpdateGraphLayout();
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
                    serializer.Serialize(writer, workflowBuilder);
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

        protected override void OnClosing(CancelEventArgs e)
        {
            stopToolStripMenuItem_Click(this, EventArgs.Empty);
            base.OnClosing(e);
        }

        #endregion

        #region EditorSite Class

        class EditorSite : ISite
        {
            MainForm siteForm;

            public EditorSite(MainForm form)
            {
                siteForm = form;
            }

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
                if (serviceType == typeof(ExpressionBuilderGraph))
                {
                    return siteForm.runningWorkflow;
                }

                return null;
            }
        }

        #endregion

        private void workflowGraphView_SelectedNodeChanged(object sender, EventArgs e)
        {
            var node = workflowGraphView.SelectedNode;
            if (node != null && node.Value != null)
            {
                var loadableElement = node.Value.GetType().GetProperties().FirstOrDefault(property => typeof(LoadableElement).IsAssignableFrom(property.PropertyType));
                if (loadableElement != null)
                {
                    propertyGrid.SelectedObject = loadableElement.GetValue(node.Value, null);
                }
                else propertyGrid.SelectedObject = node.Value;
            }
            else propertyGrid.SelectedObject = null;
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (running == null)
            {
                loaded = workflowBuilder.Load();
                runningWorkflow = workflowBuilder.Workflow.ToInspectableGraph();
                var subscriber = runningWorkflow.BuildSubscribe().Compile();
                inspectorMapping = (from node in runningWorkflow
                                    where !(node.Value is InspectBuilder)
                                    let inspectBuilder = node.Successors.First().Node.Value as InspectBuilder
                                    where inspectBuilder != null
                                    let nodeName = builderConverter.ConvertToString(node.Value)
                                    select new { node, nodeName, inspectBuilder })
                                    .ToDictionary(mapping => workflowGraphView.Model.SelectMany(layer => layer).First(node => node.Value == mapping.node.Value),
                                                  mapping =>
                                                  {
                                                      Type visualizerType;
                                                      if (!typeVisualizers.TryGetValue(mapping.inspectBuilder.ObservableType, out visualizerType))
                                                      {
                                                          visualizerType = typeVisualizers[typeof(object)];
                                                      };

                                                      var visualizer = (DialogTypeVisualizer)Activator.CreateInstance(visualizerType);
                                                      return mapping.inspectBuilder.CreateVisualizer(mapping.nodeName, visualizer, editorSite);
                                                  });

                var sourceConnections = workflowBuilder.GetSources().Select(source => source.Connect());
                running = new CompositeDisposable(Enumerable.Repeat(subscriber(), 1).Concat(sourceConnections));
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (running != null)
            {
                running.Dispose();
                loaded.Dispose();
                loaded = null;
                running = null;
                runningWorkflow = null;
                inspectorMapping = null;
            }
        }

        private void workflowGraphView_NodeMouseDoubleClick(object sender, GraphNodeMouseClickEventArgs e)
        {
            if (running != null)
            {
                var inspector = inspectorMapping[e.Node];
                inspector();
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = workflowGraphView.SelectedNode;
            if (running == null && node != null)
            {
                var workflowNode = workflowBuilder.Workflow.First(n => n.Value == node.Value);
                var predecessor = workflowBuilder.Workflow.Predecessors(workflowNode).FirstOrDefault();
                var successor = workflowBuilder.Workflow.Successors(workflowNode).FirstOrDefault();
                if (predecessor != null && successor != null)
                {
                    workflowBuilder.Workflow.AddEdge(predecessor, successor, "Source");
                }

                workflowBuilder.Workflow.Remove(workflowNode);
                UpdateGraphLayout();
            }
        }

        private void toolboxTreeView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && toolboxTreeView.SelectedNode != null && toolboxTreeView.SelectedNode.Parent != null)
            {
                var typeName = toolboxTreeView.SelectedNode.Name;
                CreateGraphNode(typeName, workflowGraphView.SelectedNode, e.Modifiers == Keys.Control);
            }
        }

        void CreateGraphNode(string typeName, GraphNode closestGraphViewNode, bool branch)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                ExpressionBuilder builder;
                var elementType = LoadableElementType.FromType(type);
                if (type.IsSubclassOf(typeof(LoadableElement)))
                {
                    var element = (LoadableElement)Activator.CreateInstance(type);
                    builder = ExpressionBuilder.FromLoadableElement(element);
                }
                else builder = (ExpressionBuilder)Activator.CreateInstance(type);

                var node = new Node<ExpressionBuilder, string>(builder);
                workflowBuilder.Workflow.Add(node);

                var closestNode = closestGraphViewNode != null ? workflowBuilder.Workflow.First(n => n.Value == closestGraphViewNode.Value) : null;
                if (elementType == LoadableElementType.Source)
                {
                    if (closestNode != null && !(closestNode.Value is SourceBuilder) && !workflowBuilder.Workflow.Predecessors(closestNode).Any())
                    {
                        workflowBuilder.Workflow.AddEdge(node, closestNode, "Source");
                    }
                }
                else
                {
                    if (closestGraphViewNode == null) throw new InvalidOperationException("A source must be placed before other elements.");

                    if (!branch)
                    {
                        var oldSuccessor = closestNode.Successors.FirstOrDefault();
                        if (oldSuccessor.Node != null)
                        {
                            //TODO: Decide when to insert or branch
                            workflowBuilder.Workflow.RemoveEdge(closestNode, oldSuccessor.Node, oldSuccessor.Label);
                            workflowBuilder.Workflow.AddEdge(node, oldSuccessor.Node, oldSuccessor.Label);
                        }
                    }

                    workflowBuilder.Workflow.AddEdge(closestNode, node, "Source");
                }

                UpdateGraphLayout();
                workflowGraphView.SelectedNode = workflowGraphView.Model.SelectMany(layer => layer).First(n => node.Value == n.Value);
            }
        }
    }
}
