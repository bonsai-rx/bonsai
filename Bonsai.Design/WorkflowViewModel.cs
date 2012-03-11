using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Dag;
using Bonsai.Expressions;
using System.Windows.Forms;
using System.Reactive.Disposables;
using System.Drawing;
using System.ComponentModel;
using System.IO;

namespace Bonsai.Design
{
    public class WorkflowViewModel
    {
        const int CtrlModifier = 0x8;
        const int AltModifier = 0x20;
        const string BonsaiExtension = ".bonsai";

        CommandExecutor commandExecutor;
        ExpressionBuilderGraph workflow;
        GraphView workflowGraphView;
        WorkflowSelectionModel selectionModel;
        IWorkflowEditorService editorService;
        Dictionary<GraphNode, VisualizerDialogLauncher> visualizerMapping;
        Dictionary<GraphNode, WorkflowEditorLauncher> workflowEditorMapping;
        ExpressionBuilderTypeConverter builderConverter;
        VisualizerLayout visualizerLayout;
        IServiceProvider serviceProvider;

        public WorkflowViewModel(GraphView graphView, IServiceProvider provider)
        {
            if (graphView == null)
            {
                throw new ArgumentNullException("graphView");
            }

            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            serviceProvider = provider;
            workflowGraphView = graphView;
            commandExecutor = (CommandExecutor)provider.GetService(typeof(CommandExecutor));
            selectionModel = (WorkflowSelectionModel)provider.GetService(typeof(WorkflowSelectionModel));
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
            builderConverter = new ExpressionBuilderTypeConverter();
            workflowEditorMapping = new Dictionary<GraphNode, WorkflowEditorLauncher>();

            workflowGraphView.DragEnter += new DragEventHandler(workflowGraphView_DragEnter);
            workflowGraphView.DragOver += new DragEventHandler(workflowGraphView_DragOver);
            workflowGraphView.DragDrop += new DragEventHandler(workflowGraphView_DragDrop);
            workflowGraphView.ItemDrag += new ItemDragEventHandler(workflowGraphView_ItemDrag);
            workflowGraphView.KeyDown += new KeyEventHandler(workflowGraphView_KeyDown);
            workflowGraphView.SelectedNodeChanged += new EventHandler(workflowGraphView_SelectedNodeChanged);
            workflowGraphView.GotFocus += new EventHandler(workflowGraphView_SelectedNodeChanged);
            workflowGraphView.NodeMouseDoubleClick += new EventHandler<GraphNodeMouseClickEventArgs>(workflowGraphView_NodeMouseDoubleClick);
            workflowGraphView.HandleDestroyed += new EventHandler(workflowGraphView_HandleDestroyed);
            editorService.WorkflowStarted += new EventHandler(editorService_WorkflowStarted);
        }

        public VisualizerLayout VisualizerLayout
        {
            get { return visualizerLayout; }
            set { visualizerLayout = value; }
        }

        public ExpressionBuilderGraph Workflow
        {
            get { return workflow; }
            set
            {
                ClearEditorMapping();
                workflow = value;
                UpdateGraphLayout();
                InitializeVisualizerLayout();
                if (editorService.WorkflowRunning)
                {
                    InitializeVisualizerMapping();
                }
            }
        }

        #region Model

        private void ClearEditorMapping()
        {
            foreach (var mapping in workflowEditorMapping)
            {
                mapping.Value.Hide();
            }

            workflowEditorMapping.Clear();
        }

        private void InitializeVisualizerMapping()
        {
            if (workflow == null) return;

            Action setLayout = null;
            visualizerMapping = (from node in workflow
                                 where !(node.Value is InspectBuilder)
                                 let inspectBuilder = node.Successors.Single().Node.Value as InspectBuilder
                                 where inspectBuilder != null
                                 let nodeName = builderConverter.ConvertToString(node.Value)
                                 let key = workflowGraphView.Nodes.SelectMany(layer => layer).First(n => n.Value == node.Value)
                                 select new { node, nodeName, inspectBuilder, key })
                                 .ToDictionary(mapping => mapping.key,
                                               mapping =>
                                               {
                                                   var workflowElementType = ExpressionBuilder.GetWorkflowElementType(mapping.node.Value);
                                                   var visualizerType = editorService.GetTypeVisualizer(workflowElementType) ??
                                                                        editorService.GetTypeVisualizer(mapping.inspectBuilder.ObservableType) ??
                                                                        editorService.GetTypeVisualizer(typeof(object));

                                                   var visualizer = (DialogTypeVisualizer)Activator.CreateInstance(visualizerType);
                                                   var launcher = new VisualizerDialogLauncher(mapping.inspectBuilder, visualizer);
                                                   launcher.Text = mapping.nodeName;

                                                   var layoutSettings = visualizerLayout != null ? visualizerLayout.DialogSettings.FirstOrDefault(xs => xs.Tag == mapping.key) : null;
                                                   if (layoutSettings != null)
                                                   {
                                                       launcher.Bounds = layoutSettings.Bounds;
                                                       if (layoutSettings.Visible)
                                                       {
                                                           setLayout += () => launcher.Show(workflowGraphView, serviceProvider);
                                                       }
                                                   }

                                                   return launcher;
                                               });

            if (setLayout != null)
            {
                setLayout();
            }
        }

        private Node<ExpressionBuilder, ExpressionBuilderParameter> GetGraphNodeTag(GraphNode node)
        {
            var nodeTag = (Node<ExpressionBuilder, ExpressionBuilderParameter>)node.Tag;
            return workflow.First(ns => ns.Value == nodeTag.Value);
        }

        public void ConnectGraphNodes(GraphNode graphViewSource, GraphNode graphViewTarget)
        {
            var source = GetGraphNodeTag(graphViewSource).Successors.Single().Node;
            var target = GetGraphNodeTag(graphViewTarget);
            if (workflow.Successors(source).Contains(target)) return;
            var connection = string.Empty;

            var combinator = target.Value as CombinatorExpressionBuilder;
            if (combinator != null)
            {
                if (!workflow.Predecessors(target).Any()) connection = "Source";
                else
                {
                    var binaryCombinator = combinator as BinaryCombinatorExpressionBuilder;
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
                    workflow.AddEdge(source, target, parameter);
                    UpdateGraphLayout();
                },
                () =>
                {
                    workflow.RemoveEdge(source, target, parameter);
                    UpdateGraphLayout();
                });
            }
        }

        public void CreateGraphNode(TreeNode typeNode, GraphNode closestGraphViewNode, bool branch)
        {
            var typeName = typeNode.Name;
            var elementType = (WorkflowElementType)typeNode.Tag;
            CreateGraphNode(typeName, elementType, closestGraphViewNode, branch);
        }

        public void CreateGraphNode(string typeName, WorkflowElementType elementType, GraphNode closestGraphViewNode, bool branch)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                ExpressionBuilder builder;
                if (type.IsSubclassOf(typeof(LoadableElement)))
                {
                    var element = (LoadableElement)Activator.CreateInstance(type);
                    builder = ExpressionBuilder.FromLoadableElement(element, elementType);
                }
                else builder = (ExpressionBuilder)Activator.CreateInstance(type);
                CreateGraphNode(builder, elementType, closestGraphViewNode, branch);
            }
        }

        public void CreateGraphNode(ExpressionBuilder builder, WorkflowElementType elementType, GraphNode closestGraphViewNode, bool branch)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            var inspectBuilder = new InspectBuilder();
            var sourceNode = new Node<ExpressionBuilder, ExpressionBuilderParameter>(builder);
            var inspectNode = new Node<ExpressionBuilder, ExpressionBuilderParameter>(inspectBuilder);
            var inspectParameter = new ExpressionBuilderParameter("Source");
            Action addNode = () => { workflow.Add(sourceNode); workflow.Add(inspectNode); workflow.AddEdge(sourceNode, inspectNode, inspectParameter); };
            Action removeNode = () => { workflow.Remove(inspectNode); workflow.Remove(sourceNode); };
            Action addConnection = () => { };
            Action removeConnection = () => { };

            var closestNode = closestGraphViewNode != null ? GetGraphNodeTag(closestGraphViewNode) : null;
            var closestInspectNode = closestNode != null ? closestNode.Successors.Single().Node : null;
            if (elementType == WorkflowElementType.Source)
            {
                if (closestNode != null && !(closestNode.Value is SourceBuilder) && !workflow.Predecessors(closestNode).Any())
                {
                    var parameter = new ExpressionBuilderParameter("Source");
                    addConnection = () => workflow.AddEdge(inspectNode, closestNode, parameter);
                    removeConnection = () => workflow.RemoveEdge(inspectNode, closestNode, parameter);
                }
            }
            else if (closestNode != null)
            {
                if (!branch)
                {
                    var oldSuccessor = closestInspectNode.Successors.FirstOrDefault();
                    if (oldSuccessor.Node != null)
                    {
                        //TODO: Decide when to insert or branch
                        addConnection = () =>
                        {
                            workflow.RemoveEdge(closestInspectNode, oldSuccessor.Node, oldSuccessor.Label);
                            workflow.AddEdge(inspectNode, oldSuccessor.Node, oldSuccessor.Label);
                        };

                        removeConnection = () =>
                        {
                            workflow.RemoveEdge(inspectNode, oldSuccessor.Node, oldSuccessor.Label);
                            workflow.AddEdge(closestInspectNode, oldSuccessor.Node, oldSuccessor.Label);
                        };
                    }
                }

                var insertSuccessor = addConnection;
                var removeSuccessor = removeConnection;
                var parameter = new ExpressionBuilderParameter("Source");
                addConnection = () => { insertSuccessor(); workflow.AddEdge(closestInspectNode, sourceNode, parameter); };
                removeConnection = () => { workflow.RemoveEdge(closestInspectNode, sourceNode, parameter); removeSuccessor(); };
            }

            commandExecutor.Execute(
            () =>
            {
                addNode();
                addConnection();
                UpdateGraphLayout();
                workflowGraphView.SelectedNode = workflowGraphView.Nodes.SelectMany(layer => layer).First(n => GetGraphNodeTag(n).Value == sourceNode.Value);
            },
            () =>
            {
                removeConnection();
                removeNode();
                UpdateGraphLayout();
                workflowGraphView.SelectedNode = workflowGraphView.Nodes.SelectMany(layer => layer).FirstOrDefault(n => closestNode != null ? GetGraphNodeTag(n).Value == closestNode.Value : false);
            });
        }

        public void DeleteGraphNode(GraphNode node)
        {
            if (node != null)
            {
                Action addEdge = () => { };
                Action removeEdge = () => { };

                var workflowNode = GetGraphNodeTag(node);
                var inspectNode = workflowNode.Successors.Single().Node;

                var predecessorEdges = workflow.PredecessorEdges(workflowNode).ToArray();
                var sourcePredecessor = Array.Find(predecessorEdges, edge => edge.Item2.Label.Value == "Source");
                if (sourcePredecessor != null)
                {
                    addEdge = () =>
                    {
                        foreach (var successor in inspectNode.Successors)
                        {
                            if (workflow.Successors(sourcePredecessor.Item1).Contains(successor.Node)) continue;
                            workflow.AddEdge(sourcePredecessor.Item1, successor.Node, successor.Label);
                        }
                    };

                    removeEdge = () =>
                    {
                        foreach (var successor in inspectNode.Successors)
                        {
                            workflow.RemoveEdge(sourcePredecessor.Item1, successor.Node, successor.Label);
                        }
                    };
                }

                Action removeNode = () => { workflow.Remove(inspectNode); workflow.Remove(workflowNode); };
                Action addNode = () =>
                {
                    workflow.Add(workflowNode);
                    workflow.Add(inspectNode);
                    workflow.AddEdge(workflowNode, inspectNode, new ExpressionBuilderParameter("Source"));
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

        private void LaunchVisualizer(GraphNode node)
        {
            if (visualizerMapping != null && node != null)
            {
                var visualizerDialog = visualizerMapping[node];
                visualizerDialog.Show(workflowGraphView, serviceProvider);
            }
        }

        private void LaunchWorkflowView(GraphNode node)
        {
            var editorLauncher = GetWorkflowEditorLauncher(node);
            if (editorLauncher != null)
            {
                editorLauncher.Show(workflowGraphView, serviceProvider);
            }
        }

        private WorkflowEditorLauncher GetWorkflowEditorLauncher(GraphNode node)
        {
            var builderNode = node != null ? GetGraphNodeTag(node) : null;
            var workflowExpressionBuilder = builderNode != null ? builderNode.Value as WorkflowExpressionBuilder : null;
            if (workflowExpressionBuilder != null)
            {
                WorkflowEditorLauncher editorLauncher;
                if (!workflowEditorMapping.TryGetValue(node, out editorLauncher))
                {
                    editorLauncher = new WorkflowEditorLauncher(workflow, builderNode);
                    workflowEditorMapping.Add(node, editorLauncher);
                }

                return editorLauncher;
            }

            return null;
        }

        private void InitializeVisualizerLayout()
        {
            if (visualizerLayout == null) return;

            var layoutSettings = visualizerLayout.DialogSettings.GetEnumerator();
            foreach (var node in workflow.Where(n => !(n.Value is InspectBuilder)))
            {
                if (!layoutSettings.MoveNext()) break;
                var graphNode = workflowGraphView.Nodes.SelectMany(layer => layer).First(n => n.Value == node.Value);
                layoutSettings.Current.Tag = graphNode;

                var workflowEditorSettings = layoutSettings.Current as WorkflowEditorSettings;
                if (workflowEditorSettings != null)
                {
                    var editorLauncher = GetWorkflowEditorLauncher(graphNode);
                    editorLauncher.VisualizerLayout = workflowEditorSettings.EditorVisualizerLayout;
                    editorLauncher.Bounds = workflowEditorSettings.EditorDialogSettings.Bounds;
                    if (workflowEditorSettings.EditorDialogSettings.Visible)
                    {
                        LaunchWorkflowView(graphNode);
                    }
                }
            }
        }

        public void UpdateVisualizerLayout()
        {
            if (visualizerMapping != null)
            {
                if (visualizerLayout == null)
                {
                    visualizerLayout = new VisualizerLayout();
                }
                visualizerLayout.DialogSettings.Clear();

                foreach (var mapping in visualizerMapping)
                {
                    var visualizerDialog = mapping.Value;
                    var visible = visualizerDialog.Visible;
                    visualizerDialog.Hide();

                    VisualizerDialogSettings dialogSettings;
                    WorkflowEditorLauncher editorLauncher;
                    if (workflowEditorMapping.TryGetValue(mapping.Key, out editorLauncher))
                    {
                        if (editorLauncher.Visible) editorLauncher.UpdateEditorLayout();
                        dialogSettings = new WorkflowEditorSettings
                        {
                            EditorVisualizerLayout = editorLauncher.VisualizerLayout,
                            EditorDialogSettings = new VisualizerDialogSettings
                            {
                                Visible = editorLauncher.Visible,
                                Bounds = editorLauncher.Bounds,
                                Tag = editorLauncher
                            }
                        };
                    }
                    else dialogSettings = new VisualizerDialogSettings();

                    dialogSettings.Visible = visible;
                    dialogSettings.Bounds = visualizerDialog.Bounds;
                    dialogSettings.Tag = mapping.Key;
                    visualizerLayout.DialogSettings.Add(dialogSettings);
                }
            }

            visualizerMapping = null;
        }

        private void UpdateGraphLayout()
        {
            workflowGraphView.Nodes = workflow.FromInspectableGraph(false).LongestPathLayering().EnsureLayerPriority().ToList();
            workflowGraphView.Invalidate();
        }

        #endregion

        #region Controller

        private void workflowGraphView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(typeof(GraphNode)))
            {
                var graphViewSource = (GraphNode)e.Data.GetData(typeof(GraphNode));
                var node = GetGraphNodeTag(graphViewSource);
                if (node != null && workflow.Contains(node))
                {
                    e.Effect = DragDropEffects.Link;
                }
            }
            else e.Effect = DragDropEffects.None;
        }

        void workflowGraphView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var path = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                if (path != null && path.Length > 0 &&
                    Path.GetExtension(path[0]) == BonsaiExtension &&
                    File.Exists(path[0]))
                {
                    if (workflowGraphView.ParentForm.Owner == null &&
                        (e.KeyState & AltModifier) == 0)
                    {
                        e.Effect = DragDropEffects.Link;
                    }
                    else e.Effect = DragDropEffects.Copy;
                }
                else e.Effect = DragDropEffects.None;
            }
        }

        private void workflowGraphView_DragDrop(object sender, DragEventArgs e)
        {
            var dropLocation = workflowGraphView.PointToClient(new Point(e.X, e.Y));
            if (e.Effect == DragDropEffects.Copy)
            {
                var branch = (e.KeyState & CtrlModifier) != 0;
                var closestGraphViewNode = workflowGraphView.GetClosestNodeTo(dropLocation);
                if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                {
                    var path = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                    var workflowBuilder = editorService.LoadWorkflow(path[0]);
                    var workflowExpressionBuilder = new WorkflowExpressionBuilder(workflowBuilder.Workflow);
                    CreateGraphNode(workflowExpressionBuilder, WorkflowElementType.Combinator, closestGraphViewNode, branch);
                }
                else
                {
                    var typeNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
                    CreateGraphNode(typeNode, closestGraphViewNode, branch);
                }
            }

            if (e.Effect == DragDropEffects.Link)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                {
                    var path = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                    editorService.OpenWorkflow(path[0]);
                }
                else
                {
                    var linkNode = workflowGraphView.GetNodeAt(dropLocation);
                    if (linkNode != null)
                    {
                        var node = (GraphNode)e.Data.GetData(typeof(GraphNode));
                        ConnectGraphNodes(node, linkNode);
                    }
                }
            }

            if (e.Effect != DragDropEffects.None)
            {
                var parentForm = workflowGraphView.ParentForm;
                if (parentForm != null && !parentForm.Focused) parentForm.Activate();
                workflowGraphView.Select();
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

        void workflowGraphView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteGraphNode(workflowGraphView.SelectedNode);
            }

            if (e.KeyCode == Keys.Return)
            {
                if (e.Modifiers == Keys.Control)
                {
                    LaunchWorkflowView(workflowGraphView.SelectedNode);
                }
                else
                {
                    LaunchVisualizer(workflowGraphView.SelectedNode);
                }
            }

            if (e.KeyCode == Keys.Back && e.Modifiers == Keys.Control)
            {
                var owner = workflowGraphView.ParentForm.Owner;
                if (owner != null)
                {
                    owner.Activate();
                }
            }
        }

        private void workflowGraphView_SelectedNodeChanged(object sender, EventArgs e)
        {
            selectionModel.SetSelectedNode(this, workflowGraphView.SelectedNode);
        }

        private void workflowGraphView_NodeMouseDoubleClick(object sender, GraphNodeMouseClickEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                LaunchWorkflowView(e.Node);
            }
            else
            {
                LaunchVisualizer(e.Node);
            }
        }

        void workflowGraphView_HandleDestroyed(object sender, EventArgs e)
        {
            editorService.WorkflowStarted -= editorService_WorkflowStarted;
        }

        void editorService_WorkflowStarted(object sender, EventArgs e)
        {
            InitializeVisualizerMapping();
        }

        #endregion
    }
}
