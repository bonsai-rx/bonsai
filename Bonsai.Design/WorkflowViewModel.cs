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
        const int ShiftModifier = 0x4;
        const int CtrlModifier = 0x8;
        const int AltModifier = 0x20;
        public const Keys BranchModifier = Keys.Alt;
        public const Keys PredecessorModifier = Keys.Shift;
        public const string BonsaiExtension = ".bonsai";

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

        public GraphView WorkflowGraphView
        {
            get { return workflowGraphView; }
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
                                                   var workflowElementType = ExpressionBuilder.GetWorkflowElement(mapping.node.Value).GetType();
                                                   var visualizerType = editorService.GetTypeVisualizer(workflowElementType) ??
                                                                        editorService.GetTypeVisualizer(mapping.inspectBuilder.ObservableType) ??
                                                                        editorService.GetTypeVisualizer(typeof(object));

                                                   var visualizer = (DialogTypeVisualizer)Activator.CreateInstance(visualizerType);
                                                   var launcher = new VisualizerDialogLauncher(mapping.inspectBuilder, visualizer, this);
                                                   launcher.Text = mapping.nodeName;
                                                   return launcher;
                                               });

            foreach (var mapping in visualizerMapping)
            {
                var layoutSettings = visualizerLayout != null ? visualizerLayout.DialogSettings.FirstOrDefault(xs => xs.Tag == mapping.Key) : null;
                if (layoutSettings != null)
                {
                    var launcher = mapping.Value;
                    var mashupVisualizer = launcher.Visualizer as DialogMashupVisualizer;
                    if (mashupVisualizer != null)
                    {
                        foreach (var mashup in layoutSettings.Mashups)
                        {
                            if (mashup < 0 || mashup >= visualizerLayout.DialogSettings.Count) continue;
                            var mashupNode = (GraphNode)visualizerLayout.DialogSettings[mashup].Tag;
                            launcher.CreateMashup(mashupNode, editorService);
                        }
                    }

                    launcher.Bounds = layoutSettings.Bounds;
                    if (layoutSettings.Visible)
                    {
                        setLayout += () => launcher.Show(workflowGraphView, serviceProvider);
                    }
                }
            }

            if (setLayout != null)
            {
                setLayout();
            }
        }

        private Node<ExpressionBuilder, ExpressionBuilderParameter> GetGraphNodeTag(GraphNode node)
        {
            return GetGraphNodeTag(node, true);
        }

        private Node<ExpressionBuilder, ExpressionBuilderParameter> GetGraphNodeTag(GraphNode node, bool throwOnError)
        {
            while (node.Value == null)
            {
                var edge = (GraphEdge)node.Tag;
                node = edge.Node;
            }

            var nodeTag = (Node<ExpressionBuilder, ExpressionBuilderParameter>)node.Tag;
            if (throwOnError) return workflow.First(ns => ns.Value == nodeTag.Value);
            else return workflow.FirstOrDefault(ns => ns.Value == nodeTag.Value);
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
                if (!workflow.Predecessors(target).Any()) connection = ExpressionBuilderParameter.Source;
                else
                {
                    var binaryCombinator = combinator as BinaryCombinatorExpressionBuilder;
                    if (binaryCombinator != null && workflow.Predecessors(target).SingleOrDefault() != null)
                    {
                        var combinatorBuilder = binaryCombinator as CombinatorBuilder;
                        if (combinatorBuilder == null ||
                            combinatorBuilder.Combinator.GetType().IsDefined(typeof(BinaryCombinatorAttribute), true))
                        {
                            connection = ExpressionBuilderParameter.Other;
                        }
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

        public void CreateGraphNode(TreeNode typeNode, GraphNode closestGraphViewNode, CreateGraphNodeType nodeType, bool branch)
        {
            var typeName = typeNode.Name;
            var elementType = (ElementCategory)typeNode.Tag;
            CreateGraphNode(typeName, elementType, closestGraphViewNode, nodeType, branch);
        }

        public void CreateGraphNode(string typeName, ElementCategory elementType, GraphNode closestGraphViewNode, CreateGraphNodeType nodeType, bool branch)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                ExpressionBuilder builder;
                if (!type.IsSubclassOf(typeof(ExpressionBuilder)))
                {
                    var element = Activator.CreateInstance(type);
                    builder = ExpressionBuilder.FromWorkflowElement(element, elementType);
                }
                else builder = (ExpressionBuilder)Activator.CreateInstance(type);
                CreateGraphNode(builder, elementType, closestGraphViewNode, nodeType, branch);
            }
        }

        public void CreateGraphNode(ExpressionBuilder builder, ElementCategory elementType, GraphNode closestGraphViewNode, CreateGraphNodeType nodeType, bool branch)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            var inspectBuilder = new InspectBuilder();
            var sourceNode = new Node<ExpressionBuilder, ExpressionBuilderParameter>(builder);
            var inspectNode = new Node<ExpressionBuilder, ExpressionBuilderParameter>(inspectBuilder);
            var inspectParameter = new ExpressionBuilderParameter();
            Action addNode = () => { workflow.Add(sourceNode); workflow.Add(inspectNode); workflow.AddEdge(sourceNode, inspectNode, inspectParameter); };
            Action removeNode = () => { workflow.Remove(inspectNode); workflow.Remove(sourceNode); };
            Action addConnection = () => { };
            Action removeConnection = () => { };

            var closestNode = closestGraphViewNode != null ? GetGraphNodeTag(closestGraphViewNode) : null;
            if (elementType == ElementCategory.Source)
            {
                if (closestNode != null && !(closestNode.Value is SourceBuilder) && !workflow.Predecessors(closestNode).Any())
                {
                    var parameter = new ExpressionBuilderParameter();
                    addConnection = () => workflow.AddEdge(inspectNode, closestNode, parameter);
                    removeConnection = () => workflow.RemoveEdge(inspectNode, closestNode, parameter);
                }
            }
            else if (closestNode != null)
            {
                var edgeIndex = 0;
                var closestInspectNode = closestNode.Successors.Single().Node;
                var parameter = new ExpressionBuilderParameter();
                if (nodeType == CreateGraphNodeType.Predecessor)
                {
                    var closestPredecessorNode = workflow.Predecessors(closestNode).FirstOrDefault();
                    if (closestPredecessorNode != null)
                    {
                        // If we have a predecessor, we need to insert the new node in the right branch
                        closestInspectNode = closestPredecessorNode;
                        foreach (var edge in closestInspectNode.Successors)
                        {
                            if (edge.Node == closestNode) break;
                            edgeIndex++;
                        }

                        var oldSuccessor = closestInspectNode.Successors[edgeIndex];
                        addConnection = () =>
                        {
                            workflow.SetEdge(closestInspectNode, edgeIndex, sourceNode, parameter);
                            workflow.AddEdge(inspectNode, oldSuccessor.Node, oldSuccessor.Label);
                        };

                        removeConnection = () =>
                        {
                            workflow.RemoveEdge(inspectNode, oldSuccessor.Node, oldSuccessor.Label);
                            workflow.SetEdge(closestInspectNode, edgeIndex, oldSuccessor.Node, oldSuccessor.Label);
                        };
                    }
                    else
                    {
                        // If there is no predecessor, we just create an edge to the selected node
                        addConnection = () => { workflow.AddEdge(inspectNode, closestNode, parameter); };
                        removeConnection = () => { workflow.RemoveEdge(inspectNode, closestNode, parameter); };
                    }
                }
                else
                {
                    if (!branch && closestInspectNode.Successors.Count > 0)
                    {
                        // If we are not creating a new branch, the new node will inherit all branches of selected node
                        var oldSuccessors = closestInspectNode.Successors.ToArray();
                        addConnection = () =>
                        {
                            foreach (var successor in oldSuccessors)
                            {
                                workflow.RemoveEdge(closestInspectNode, successor.Node, successor.Label);
                                workflow.AddEdge(inspectNode, successor.Node, successor.Label);
                            }
                            workflow.AddEdge(closestInspectNode, sourceNode, parameter);
                        };

                        removeConnection = () =>
                        {
                            foreach (var successor in oldSuccessors)
                            {
                                workflow.RemoveEdge(inspectNode, successor.Node, successor.Label);
                                workflow.AddEdge(closestInspectNode, successor.Node, successor.Label);
                            }
                            workflow.RemoveEdge(closestInspectNode, sourceNode, parameter);
                        };
                    }
                    else
                    {
                        // Otherwise, just create the new branch
                        addConnection = () => { workflow.AddEdge(closestInspectNode, sourceNode, parameter); };
                        removeConnection = () => { workflow.RemoveEdge(closestInspectNode, sourceNode, parameter); };
                    }
                }
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
                var sourcePredecessor = Array.Find(predecessorEdges, edge => edge.Item2.Label.Value == ExpressionBuilderParameter.Source);
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
                    workflow.AddEdge(workflowNode, inspectNode, new ExpressionBuilderParameter());
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

        public GraphNode FindGraphNode(object value)
        {
            return workflowGraphView.Nodes.SelectMany(layer => layer).FirstOrDefault(n => n.Value == value);
        }

        public void LaunchVisualizer(GraphNode node)
        {
            var visualizerDialog = GetVisualizerDialogLauncher(node);
            if (visualizerDialog != null)
            {
                visualizerDialog.Show(workflowGraphView, serviceProvider);
            }
        }

        public void LaunchWorkflowView(GraphNode node)
        {
            var editorLauncher = GetWorkflowEditorLauncher(node);
            if (editorLauncher != null)
            {
                editorLauncher.Show(workflowGraphView, serviceProvider);
            }
        }

        public VisualizerDialogLauncher GetVisualizerDialogLauncher(GraphNode node)
        {
            VisualizerDialogLauncher visualizerDialog = null;
            if (visualizerMapping != null && node != null)
            {
                visualizerMapping.TryGetValue(node, out visualizerDialog);
            }

            return visualizerDialog;
        }

        public WorkflowEditorLauncher GetWorkflowEditorLauncher(GraphNode node)
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

                    var mashupVisualizer = visualizerDialog.Visualizer as DialogMashupVisualizer;
                    if (mashupVisualizer != null)
                    {
                        foreach (var mashup in mashupVisualizer.Mashups)
                        {
                            var predecessorIndex = (from indexedNode in visualizerMapping.Select((node, index) => new { node, index })
                                                    where indexedNode.node.Value.Source.Output == mashup.Source
                                                    select indexedNode.index)
                                                   .FirstOrDefault();
                            dialogSettings.Mashups.Add(predecessorIndex);
                        }
                    }

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
                var node = GetGraphNodeTag(graphViewSource, false);
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
                var branch = (e.KeyState & AltModifier) != 0;
                var predecessor = (e.KeyState & ShiftModifier) != 0 ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
                var linkNode = workflowGraphView.GetNodeAt(dropLocation) ?? workflowGraphView.SelectedNode;
                if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                {
                    var path = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                    var workflowBuilder = editorService.LoadWorkflow(path[0]);
                    var workflowExpressionBuilder = new NestedWorkflowExpressionBuilder(workflowBuilder.Workflow);
                    CreateGraphNode(workflowExpressionBuilder, ElementCategory.Combinator, linkNode, predecessor, branch);
                }
                else
                {
                    var typeNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
                    CreateGraphNode(typeNode, linkNode, predecessor, branch);
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

            if (e.KeyCode == Keys.C && e.Modifiers.HasFlag(Keys.Control))
            {
                var node = workflowGraphView.SelectedNode;
                if (node != null)
                {
                    editorService.StoreWorkflowElement((ExpressionBuilder)node.Value);
                }
            }

            if (e.KeyCode == Keys.V && e.Modifiers.HasFlag(Keys.Control))
            {
                var expressionBuilder = editorService.RetrieveWorkflowElement();
                if (expressionBuilder != null)
                {
                    var branch = e.Modifiers.HasFlag(BranchModifier);
                    var predecessor = e.Modifiers.HasFlag(PredecessorModifier) ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
                    CreateGraphNode(expressionBuilder, expressionBuilder.GetType() == typeof(SourceBuilder) ? ElementCategory.Source : ElementCategory.Combinator, workflowGraphView.SelectedNode, predecessor, branch);
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

    public enum CreateGraphNodeType
    {
        Successor,
        Predecessor
    }
}
