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
using Bonsai.Editor.Properties;
using System.IO;

namespace Bonsai.Editor
{
    public partial class MainForm : Form
    {
        const int CtrlModifier = 0x8;
        const string LayoutExtension = ".layout";

        int version;
        int saveVersion;
        EditorSite editorSite;
        WorkflowBuilder workflowBuilder;

        XmlSerializer serializer;
        XmlSerializer layoutSerializer;
        Dictionary<Type, Type> typeVisualizers;
        ExpressionBuilderGraph inspectableWorkflow;
        VisualizerLayout visualizerLayout;
        Dictionary<GraphNode, VisualizerDialogLauncher> visualizerMapping;
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
            layoutSerializer = new XmlSerializer(typeof(VisualizerLayout));
            typeVisualizers = TypeVisualizerLoader.GetTypeVisualizerDictionary();
            builderConverter = new ExpressionBuilderTypeConverter();
            propertyGrid.Site = editorSite;
        }

        #region Toolbox

        void InitializeToolbox()
        {
            var packages = WorkflowElementLoader.GetWorkflowElementTypes();
            foreach (var package in packages)
            {
                InitializeToolboxCategory(package.Key, package);
            }

            InitializeToolboxCategory("Combinator", new[]
            {
                typeof(DistinctUntilChangedBuilder), typeof(TimestampBuilder), typeof(TimeIntervalBuilder),
                typeof(ThrottleBuilder), typeof(SkipUntilBuilder), typeof(TakeUntilBuilder),
                typeof(SampleBuilder), typeof(SampleIntervalBuilder), typeof(GateBuilder), typeof(GateIntervalBuilder),
                typeof(TimedGateBuilder), typeof(CombineLatestBuilder), typeof(ConcatBuilder), typeof(ZipBuilder),
                typeof(AmbBuilder), typeof(DelayBuilder), typeof(RepeatBuilder), typeof(MemberSelectorBuilder)
            });
        }

        string GetPackageDisplayName(string packageKey)
        {
            return packageKey.Replace("Bonsai.", string.Empty);
        }

        int GetElementTypeIndex(string typeName)
        {
            return
                typeName == LoadableElementType.Source.ToString() ? 0 :
                typeName == LoadableElementType.Filter.ToString() ? 1 :
                typeName == LoadableElementType.Projection.ToString() ? 2 :
                typeName == LoadableElementType.Sink.ToString() ? 3 : 4;
        }

        int CompareLoadableElementType(string left, string right)
        {
            return GetElementTypeIndex(left).CompareTo(GetElementTypeIndex(right));
        }

        void InitializeToolboxCategory(string categoryName, IEnumerable<Type> types)
        {
            TreeNode category = null;

            foreach (var type in types)
            {
                foreach (var elementType in LoadableElementType.FromType(type))
                {
                    var name = type.IsSubclassOf(typeof(ExpressionBuilder)) ? type.Name.Remove(type.Name.LastIndexOf("Builder")) : type.Name;

                    if (elementType == null && type.IsSubclassOf(typeof(LoadableElement))) continue;
                    if (category == null)
                    {
                        category = toolboxTreeView.Nodes.Add(categoryName, GetPackageDisplayName(categoryName));
                    }

                    var elementTypeNode = elementType == null ? category : category.Nodes[elementType.ToString()];
                    if (elementTypeNode == null)
                    {
                        int index;
                        for (index = 0; index < category.Nodes.Count; index++)
                        {
                            if (CompareLoadableElementType(elementType.ToString(), category.Nodes[index].Name) <= 0)
                            {
                                break;
                            }
                        }

                        elementTypeNode = category.Nodes.Insert(index, elementType.ToString(), elementType.ToString());
                    }

                    var node = elementTypeNode.Nodes.Add(type.AssemblyQualifiedName, name);
                    node.Tag = elementType;
                }
            }
        }

        #endregion

        #region File Menu

        void ResetProjectStatus()
        {
            commandExecutor.Clear();
            UpdateGraphLayout();
            version = 0;
            saveVersion = 0;
        }

        bool CheckUnsavedChanges()
        {
            if (saveVersion != version)
            {
                var result = MessageBox.Show("Workflow has unsaved changes. Save project file?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(this, EventArgs.Empty);
                    return saveVersion == version;
                }
                else return result == DialogResult.No;
            }

            return true;
        }

        string GetLayoutPath(string fileName)
        {
            return Path.ChangeExtension(fileName, Path.GetExtension(fileName) + LayoutExtension);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckUnsavedChanges()) return;

            visualizerLayout = null;
            saveWorkflowDialog.FileName = null;
            workflowBuilder.Workflow.Clear();
            ResetProjectStatus();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckUnsavedChanges()) return;

            if (openWorkflowDialog.ShowDialog() == DialogResult.OK)
            {
                saveWorkflowDialog.FileName = openWorkflowDialog.FileName;
                using (var reader = XmlReader.Create(openWorkflowDialog.FileName))
                {
                    workflowBuilder = (WorkflowBuilder)serializer.Deserialize(reader);
                    ResetProjectStatus();
                }

                var layoutPath = GetLayoutPath(openWorkflowDialog.FileName);
                if (File.Exists(layoutPath))
                {
                    using (var reader = XmlReader.Create(layoutPath))
                    {
                        visualizerLayout = (VisualizerLayout)layoutSerializer.Deserialize(reader);
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
                    serializer.Serialize(writer, workflowBuilder);
                    saveVersion = version;
                }

                if (visualizerLayout != null)
                {
                    var layoutPath = GetLayoutPath(saveWorkflowDialog.FileName);
                    using (var writer = XmlWriter.Create(layoutPath, new XmlWriterSettings { Indent = true }))
                    {
                        layoutSerializer.Serialize(writer, visualizerLayout);
                    }
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
            if (!CheckUnsavedChanges()) e.Cancel = true;
            else StopWorkflow();
            base.OnClosing(e);
        }

        #endregion

        #region Workflow Model

        void ConnectGraphNodes(GraphNode graphViewSource, GraphNode graphViewTarget)
        {
            var source = (Node<ExpressionBuilder, ExpressionBuilderParameter>)graphViewSource.Tag;
            var target = (Node<ExpressionBuilder, ExpressionBuilderParameter>)graphViewTarget.Tag;
            if (workflowBuilder.Workflow.Successors(source).Contains(target)) return;
            var connection = string.Empty;

            var combinator = target.Value as CombinatorBuilder;
            if (combinator != null)
            {
                if (!workflowBuilder.Workflow.Predecessors(target).Any()) connection = "Source";
                else
                {
                    var binaryCombinator = combinator as BinaryCombinatorBuilder;
                    if (binaryCombinator != null && binaryCombinator.Other == null)
                    {
                        connection = "Other";
                    }
                }
            }

            if (!string.IsNullOrEmpty(connection))
            {
                var parameter = new ExpressionBuilderParameter(connection);
                commandExecutor.Execute(
                () =>
                {
                    workflowBuilder.Workflow.AddEdge(source, target, parameter);
                    UpdateGraphLayout();
                },
                () =>
                {
                    workflowBuilder.Workflow.RemoveEdge(source, target, parameter);
                    UpdateGraphLayout();
                });
            }
        }

        void CreateGraphNode(TreeNode typeNode, GraphNode closestGraphViewNode, bool branch)
        {
            if(typeNode== null) throw new ArgumentNullException("typeNode");

            var type = Type.GetType(typeNode.Name);
            if (running == null && type != null)
            {
                ExpressionBuilder builder;
                var elementType = LoadableElementType.FromType(type);
                if (type.IsSubclassOf(typeof(LoadableElement)))
                {
                    var element = (LoadableElement)Activator.CreateInstance(type);
                    builder = ExpressionBuilder.FromLoadableElement(element, (LoadableElementType)typeNode.Tag);
                }
                else builder = (ExpressionBuilder)Activator.CreateInstance(type);

                var node = new Node<ExpressionBuilder, ExpressionBuilderParameter>(builder);
                Action addNode = () => workflowBuilder.Workflow.Add(node);
                Action removeNode = () => workflowBuilder.Workflow.Remove(node);
                Action addConnection = () => { };
                Action removeConnection = () => { };

                var closestNode = closestGraphViewNode != null ? (Node<ExpressionBuilder, ExpressionBuilderParameter>)closestGraphViewNode.Tag : null;
                if (elementType.Contains(LoadableElementType.Source))
                {
                    if (closestNode != null && !(closestNode.Value is SourceBuilder) && !workflowBuilder.Workflow.Predecessors(closestNode).Any())
                    {
                        var parameter = new ExpressionBuilderParameter("Source");
                        addConnection = () => workflowBuilder.Workflow.AddEdge(node, closestNode, parameter);
                        removeConnection = () => workflowBuilder.Workflow.RemoveEdge(node, closestNode, parameter);
                    }
                }
                else if (closestNode != null)
                {
                    if (!branch)
                    {
                        var oldSuccessor = closestNode.Successors.FirstOrDefault();
                        if (oldSuccessor.Node != null)
                        {
                            //TODO: Decide when to insert or branch
                            addConnection = () =>
                            {
                                workflowBuilder.Workflow.RemoveEdge(closestNode, oldSuccessor.Node, oldSuccessor.Label);
                                workflowBuilder.Workflow.AddEdge(node, oldSuccessor.Node, oldSuccessor.Label);
                            };

                            removeConnection = () =>
                            {
                                workflowBuilder.Workflow.RemoveEdge(node, oldSuccessor.Node, oldSuccessor.Label);
                                workflowBuilder.Workflow.AddEdge(closestNode, oldSuccessor.Node, oldSuccessor.Label);
                            };
                        }
                    }

                    var insertSuccessor = addConnection;
                    var removeSuccessor = removeConnection;
                    var parameter = new ExpressionBuilderParameter("Source");
                    addConnection = () => { insertSuccessor(); workflowBuilder.Workflow.AddEdge(closestNode, node, parameter); };
                    removeConnection = () => { workflowBuilder.Workflow.RemoveEdge(closestNode, node, parameter); removeSuccessor(); };
                }

                commandExecutor.Execute(
                () =>
                {
                    addNode();
                    addConnection();
                    UpdateGraphLayout();
                    workflowGraphView.SelectedNode = workflowGraphView.Nodes.SelectMany(layer => layer).First(n => n.Tag == node);
                },
                () =>
                {
                    removeConnection();
                    removeNode();
                    UpdateGraphLayout();
                    workflowGraphView.SelectedNode = workflowGraphView.Nodes.SelectMany(layer => layer).FirstOrDefault(n => n.Tag == closestNode);
                });
            }
        }

        void DeleteGraphNode(GraphNode node)
        {
            if (running == null && node != null)
            {
                Action addEdge = () => { };
                Action removeEdge = () => { };

                var workflowNode = (Node<ExpressionBuilder, ExpressionBuilderParameter>)node.Tag;
                var predecessorEdges = workflowBuilder.Workflow.PredecessorEdges(workflowNode).ToArray();
                var sourcePredecessor = Array.Find(predecessorEdges, edge => edge.Item2.Label.Value == "Source");
                if (sourcePredecessor != null)
                {
                    addEdge = () =>
                    {
                        foreach (var successor in workflowNode.Successors)
                        {
                            if (workflowBuilder.Workflow.Successors(sourcePredecessor.Item1).Contains(successor.Node)) continue;
                            workflowBuilder.Workflow.AddEdge(sourcePredecessor.Item1, successor.Node, successor.Label);
                        }
                    };

                    removeEdge = () =>
                    {
                        foreach (var successor in workflowNode.Successors)
                        {
                            workflowBuilder.Workflow.RemoveEdge(sourcePredecessor.Item1, successor.Node, successor.Label);
                        }
                    };
                }

                Action removeNode = () => workflowBuilder.Workflow.Remove(workflowNode);
                Action addNode = () =>
                {
                    workflowBuilder.Workflow.Add(workflowNode);
                    foreach (var edge in predecessorEdges)
                    {
                        edge.Item1.Successors.Insert(edge.Item3, edge.Item2);
                    }
                };

                commandExecutor.Execute(
                () =>
                {
                    addEdge();
                    removeNode();
                    UpdateGraphLayout();
                    workflowGraphView.SelectedNode = workflowGraphView.Nodes.SelectMany(layer => layer).FirstOrDefault(n => n.Tag != null && n.Tag == sourcePredecessor);
                },
                () =>
                {
                    addNode();
                    removeEdge();
                    UpdateGraphLayout();
                    workflowGraphView.SelectedNode = workflowGraphView.Nodes.SelectMany(layer => layer).FirstOrDefault(n => n.Tag == node.Tag);
                });
            }
        }

        void StartWorkflow()
        {
            if (running == null)
            {
                inspectableWorkflow = workflowBuilder.Workflow.ToInspectableGraph();
                try
                {
                    var runningWorkflow = inspectableWorkflow.Build();
                    var subscribeExpression = runningWorkflow.BuildSubscribe(HandleWorkflowError);

                    var layoutSettings = visualizerLayout != null ? visualizerLayout.DialogSettings.GetEnumerator() : null;
                    Action setLayout = null;

                    visualizerMapping = (from node in inspectableWorkflow
                                         where !(node.Value is InspectBuilder)
                                         let inspectBuilder = node.Successors.First().Node.Value as InspectBuilder
                                         where inspectBuilder != null
                                         let nodeName = builderConverter.ConvertToString(node.Value)
                                         select new { node, nodeName, inspectBuilder })
                                        .ToDictionary(mapping => workflowGraphView.Nodes.SelectMany(layer => layer).First(node => node.Value == mapping.node.Value),
                                                      mapping =>
                                                      {
                                                          Type visualizerType;
                                                          if (!typeVisualizers.TryGetValue(mapping.inspectBuilder.ObservableType, out visualizerType))
                                                          {
                                                              visualizerType = typeVisualizers[typeof(object)];
                                                          };

                                                          var visualizer = (DialogTypeVisualizer)Activator.CreateInstance(visualizerType);
                                                          var launcher = new VisualizerDialogLauncher(mapping.inspectBuilder, visualizer);
                                                          launcher.Text = mapping.nodeName;
                                                          if (layoutSettings != null && layoutSettings.MoveNext())
                                                          {
                                                              launcher.Bounds = layoutSettings.Current.Bounds;
                                                              if (layoutSettings.Current.Visible)
                                                              {
                                                                  setLayout += () => launcher.Show(editorSite);
                                                              }
                                                          }

                                                          return launcher;
                                                      });

                    loaded = runningWorkflow.Load();

                    var subscriber = subscribeExpression.Compile();
                    var sourceConnections = workflowBuilder.GetSources().Select(source => source.Connect());
                    running = new CompositeDisposable(Enumerable.Repeat(subscriber(), 1).Concat(sourceConnections));
                    if (setLayout != null)
                    {
                        setLayout();
                    }
                }
                catch (InvalidOperationException ex) { HandleWorkflowError(ex); return; }
                catch (ArgumentException ex) { HandleWorkflowError(ex); return; }
            }

            runningStatusLabel.Text = Resources.RunningStatus;
        }

        void StopWorkflow()
        {
            if (running != null)
            {
                if (visualizerLayout == null)
                {
                    visualizerLayout = new VisualizerLayout();
                }
                visualizerLayout.DialogSettings.Clear();

                foreach (var visualizerDialog in visualizerMapping.Values)
                {
                    var visible = visualizerDialog.Visible;
                    visualizerDialog.Hide();

                    visualizerLayout.DialogSettings.Add(new VisualizerDialogSettings
                    {
                        Visible = visible,
                        Bounds = visualizerDialog.Bounds
                    });
                }

                running.Dispose();
                loaded.Dispose();
                loaded = null;
                running = null;
                inspectableWorkflow = null;
                visualizerMapping = null;
            }

            runningStatusLabel.Text = Resources.StoppedStatus;
        }

        void HandleWorkflowError(Exception e)
        {
            MessageBox.Show(e.Message, "Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (running != null)
            {
                BeginInvoke((Action)StopWorkflow);
            }
        }

        void UpdateGraphLayout()
        {
            workflowGraphView.Nodes = workflowBuilder.Workflow.LongestPathLayering().AverageMinimizeCrossings().ToList();
            workflowGraphView.Invalidate();
        }

        #endregion

        #region Workflow Controller

        private void toolboxTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var selectedNode = e.Item as TreeNode;
            if (selectedNode != null && selectedNode.GetNodeCount(false) == 0)
            {
                toolboxTreeView.DoDragDrop(selectedNode, DragDropEffects.Copy);
            }
        }

        private void workflowGraphView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(typeof(GraphNode)))
            {
                e.Effect = DragDropEffects.Link;
            }
            else e.Effect = DragDropEffects.None;
        }

        private void workflowGraphView_DragDrop(object sender, DragEventArgs e)
        {
            var dropLocation = workflowGraphView.PointToClient(new Point(e.X, e.Y));
            if (e.Effect == DragDropEffects.Copy)
            {
                var typeName = (TreeNode)e.Data.GetData(typeof(TreeNode));
                var closestGraphViewNode = workflowGraphView.GetClosestNodeTo(dropLocation);
                CreateGraphNode(typeName, closestGraphViewNode, (e.KeyState & CtrlModifier) != 0);
            }

            if (e.Effect == DragDropEffects.Link)
            {
                var linkNode = workflowGraphView.GetNodeAt(dropLocation);
                if (linkNode != null)
                {
                    var node = (GraphNode)e.Data.GetData(typeof(GraphNode));
                    ConnectGraphNodes(node, linkNode);
                }
            }
        }

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
            StartWorkflow();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopWorkflow();
        }

        private void workflowGraphView_NodeMouseDoubleClick(object sender, GraphNodeMouseClickEventArgs e)
        {
            if (running != null)
            {
                var visualizerDialog = visualizerMapping[e.Node];
                visualizerDialog.Show();
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = workflowGraphView.SelectedNode;
            DeleteGraphNode(node);
        }

        private void toolboxTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && toolboxTreeView.SelectedNode != null && toolboxTreeView.SelectedNode.GetNodeCount(false) == 0)
            {
                var typeNode = toolboxTreeView.SelectedNode;
                CreateGraphNode(typeNode, workflowGraphView.SelectedNode, e.Modifiers == Keys.Control);
            }
        }

        private void workflowGraphView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var selectedNode = e.Item as GraphNode;
            if (selectedNode != null)
            {
                workflowGraphView.DoDragDrop(selectedNode, DragDropEffects.Link);
            }
        }

        private void toolboxTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Node.GetNodeCount(false) == 0)
            {
                var typeNode = e.Node;
                CreateGraphNode(typeNode, workflowGraphView.SelectedNode, Control.ModifierKeys == Keys.Control);
            }
        }

        #endregion

        #region Undo/Redo

        private void commandExecutor_StatusChanged(object sender, EventArgs e)
        {
            undoToolStripMenuItem.Enabled = commandExecutor.CanUndo;
            redoToolStripMenuItem.Enabled = commandExecutor.CanRedo;
            version++;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            version -= 2;
            commandExecutor.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            commandExecutor.Redo();
        }

        #endregion

        #region Help Menu

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var about = new AboutBox())
            {
                about.ShowDialog();
            }
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
                    return siteForm.inspectableWorkflow;
                }

                if (serviceType == typeof(WorkflowBuilder))
                {
                    return siteForm.workflowBuilder;
                }

                return null;
            }
        }

        #endregion
    }
}
