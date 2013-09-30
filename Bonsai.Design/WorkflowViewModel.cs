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
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace Bonsai.Design
{
    public class WorkflowViewModel
    {
        static readonly XName XsdAttributeName = ((XNamespace)"http://www.w3.org/2000/xmlns/") + "xsd";
        static readonly XName XsiAttributeName = ((XNamespace)"http://www.w3.org/2000/xmlns/") + "xsi";
        const string XsdAttributeValue = "http://www.w3.org/2001/XMLSchema";
        const string XsiAttributeValue = "http://www.w3.org/2001/XMLSchema-instance";

        const int ShiftModifier = 0x4;
        const int CtrlModifier = 0x8;
        const int AltModifier = 0x20;
        public const Keys BranchModifier = Keys.Alt;
        public const Keys PredecessorModifier = Keys.Shift;
        public const string BonsaiExtension = ".bonsai";

        GraphNode dragHighlight;
        IEnumerable<GraphNode> dragSelection;
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
            workflowGraphView.DragLeave += new EventHandler(workflowGraphView_DragLeave);
            workflowGraphView.ItemDrag += new ItemDragEventHandler(workflowGraphView_ItemDrag);
            workflowGraphView.KeyDown += new KeyEventHandler(workflowGraphView_KeyDown);
            workflowGraphView.SelectedNodeChanged += new EventHandler(workflowGraphView_SelectedNodeChanged);
            workflowGraphView.GotFocus += new EventHandler(workflowGraphView_SelectedNodeChanged);
            workflowGraphView.NodeMouseDoubleClick += new EventHandler<GraphNodeMouseEventArgs>(workflowGraphView_NodeMouseDoubleClick);
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

        private VisualizerDialogLauncher CreateVisualizerLauncher(ExpressionBuilder builder, InspectBuilder inspectBuilder, GraphNode key)
        {
            var workflowElementType = ExpressionBuilder.GetWorkflowElement(builder).GetType();
            var visualizerType = editorService.GetTypeVisualizer(workflowElementType) ??
                                 editorService.GetTypeVisualizer(inspectBuilder.ObservableType) ??
                                 editorService.GetTypeVisualizer(typeof(object));

            DialogTypeVisualizer visualizer = null;
            var layoutSettings = visualizerLayout != null ? visualizerLayout.DialogSettings.FirstOrDefault(xs => xs.Tag == key) : null;
            if (layoutSettings != null && layoutSettings.VisualizerSettings != null)
            {
                var root = layoutSettings.VisualizerSettings;
                root.SetAttributeValue(XsdAttributeName, XsdAttributeValue);
                root.SetAttributeValue(XsiAttributeName, XsiAttributeValue);
                var serializer = new XmlSerializer(visualizerType);
                using (var reader = layoutSettings.VisualizerSettings.CreateReader())
                {
                    if (serializer.CanDeserialize(reader))
                    {
                        visualizer = (DialogTypeVisualizer)serializer.Deserialize(reader);
                    }
                }
            }
            
            visualizer = visualizer ?? (DialogTypeVisualizer)Activator.CreateInstance(visualizerType);
            var launcher = new VisualizerDialogLauncher(inspectBuilder, visualizer, this);
            if (layoutSettings != null)
            {
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
                    launcher.Show(workflowGraphView, serviceProvider);
                }
            }

            launcher.Text = builderConverter.ConvertToString(builder);
            return launcher;
        }

        private void InitializeVisualizerMapping()
        {
            if (workflow == null) return;
            visualizerMapping = (from node in workflow
                                 where !(node.Value is InspectBuilder)
                                 let inspectBuilder = node.Successors.Single().Node.Value as InspectBuilder
                                 where inspectBuilder != null
                                 let key = workflowGraphView.Nodes.SelectMany(layer => layer).First(n => n.Value == node.Value)
                                 select new { node, inspectBuilder, key })
                                 .ToDictionary(mapping => mapping.key,
                                               mapping => CreateVisualizerLauncher(mapping.node.Value, mapping.inspectBuilder, mapping.key));
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

        Tuple<Action, Action> GetInsertGraphNodeCommands(
            Node<ExpressionBuilder, ExpressionBuilderParameter> sourceNode,
            Node<ExpressionBuilder, ExpressionBuilderParameter> sinkNode,
            ElementCategory elementType,
            Node<ExpressionBuilder, ExpressionBuilderParameter> closestNode,
            CreateGraphNodeType nodeType,
            bool branch)
        {
            Action addConnection = () => { };
            Action removeConnection = () => { };
            if (elementType == ElementCategory.Source)
            {
                if (closestNode != null && !(closestNode.Value is SourceBuilder) && !workflow.Predecessors(closestNode).Any())
                {
                    var parameter = new ExpressionBuilderParameter();
                    addConnection = () => workflow.AddEdge(sinkNode, closestNode, parameter);
                    removeConnection = () => workflow.RemoveEdge(sinkNode, closestNode, parameter);
                }
            }
            else if (closestNode != null)
            {
                var parameter = new ExpressionBuilderParameter();
                if (nodeType == CreateGraphNodeType.Predecessor)
                {
                    var predecessors = workflow.PredecessorEdges(closestNode).ToList();
                    if (predecessors.Count > 0)
                    {
                        // If we have predecessors, we need to connect the new node in the right branches
                        foreach (var predecessor in predecessors)
                        {
                            var edge = predecessor.Item2;
                            var predecessorNode = predecessor.Item1;
                            var edgeIndex = predecessor.Item3;
                            addConnection += () => { workflow.SetEdge(predecessorNode, edgeIndex, sourceNode, edge.Label); };
                            removeConnection += () => { workflow.SetEdge(predecessorNode, edgeIndex, edge.Node, edge.Label); };
                        }
                    }

                    // After dealing with predecessors, we just create an edge to the selected node
                    addConnection += () => { workflow.AddEdge(sinkNode, closestNode, parameter); };
                    removeConnection += () => { workflow.RemoveEdge(sinkNode, closestNode, parameter); };
                }
                else
                {
                    var closestInspectNode = closestNode.Successors.Single().Node;
                    if (!branch && closestInspectNode.Successors.Count > 0)
                    {
                        // If we are not creating a new branch, the new node will inherit all branches of selected node
                        var oldSuccessors = closestInspectNode.Successors.ToArray();
                        addConnection = () =>
                        {
                            foreach (var successor in oldSuccessors)
                            {
                                workflow.RemoveEdge(closestInspectNode, successor.Node, successor.Label);
                                workflow.AddEdge(sinkNode, successor.Node, successor.Label);
                            }
                            workflow.AddEdge(closestInspectNode, sourceNode, parameter);
                        };

                        removeConnection = () =>
                        {
                            foreach (var successor in oldSuccessors)
                            {
                                workflow.RemoveEdge(sinkNode, successor.Node, successor.Label);
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

            return Tuple.Create(addConnection, removeConnection);
        }

        bool ValidConnection(IEnumerable<GraphNode> graphViewSources, GraphNode graphViewTarget)
        {
            var reject = false;
            var target = GetGraphNodeTag(graphViewTarget, false);
            var connectionCount = workflow.Predecessors(target).Count();
            foreach (var sourceNode in graphViewSources)
            {
                if (graphViewTarget == sourceNode || sourceNode.Successors.Any(edge => edge.Node == graphViewTarget))
                {
                    reject = true;
                    break;
                }

                var node = GetGraphNodeTag(sourceNode, false);
                if (connectionCount++ >= target.Value.ArgumentRange.UpperBound ||
                    target.DepthFirstSearch().Contains(node))
                {
                    reject = true;
                    break;
                }
            }

            return !reject;
        }

        public void ConnectGraphNodes(IEnumerable<GraphNode> graphViewSources, GraphNode graphViewTarget)
        {
            Action addConnection = () => { };
            Action removeConnection = () => { };
            var target = GetGraphNodeTag(graphViewTarget);
            var connectionIndex = workflow.Predecessors(target).Count();
            foreach (var graphViewSource in graphViewSources)
            {
                var source = GetGraphNodeTag(graphViewSource).Successors.Single().Node;
                var connection = string.Empty;
                connection = ExpressionBuilderParameter.Source;
                if (connectionIndex > 0) connection += connectionIndex;

                var parameter = new ExpressionBuilderParameter(connection);
                addConnection += () => workflow.AddEdge(source, target, parameter);
                removeConnection += () => workflow.RemoveEdge(source, target, parameter);
                connectionIndex++;
            }

            commandExecutor.Execute(
            () =>
            {
                addConnection();
                UpdateGraphLayout();
            },
            () =>
            {
                removeConnection();
                UpdateGraphLayout();
            });
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

            var closestNode = closestGraphViewNode != null ? GetGraphNodeTag(closestGraphViewNode) : null;
            var insertCommands = GetInsertGraphNodeCommands(sourceNode, inspectNode, elementType, closestNode, nodeType, branch);
            var addConnection = insertCommands.Item1;
            var removeConnection = insertCommands.Item2;
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

        public void InsertGraphElements(ExpressionBuilderGraph elements, CreateGraphNodeType nodeType, bool branch)
        {
            if (elements == null)
            {
                throw new ArgumentNullException("elements");
            }

            var selection = selectionModel.SelectedNodes.FirstOrDefault();
            var inspectableGraph = elements.ToInspectableGraph();
            Action addConnection = () => { };
            Action removeConnection = () => { };
            if (selection != null)
            {
                var selectionNode = GetGraphNodeTag(selection);
                var source = inspectableGraph.Sources().FirstOrDefault();
                var sink = inspectableGraph.Sinks().FirstOrDefault();
                if (source != null && sink != null)
                {
                    var insertCommands = GetInsertGraphNodeCommands(source, sink, ElementCategory.Combinator, selectionNode, nodeType, branch);
                    addConnection = insertCommands.Item1;
                    removeConnection = insertCommands.Item2;
                }
            }

            commandExecutor.Execute(
            () =>
            {
                foreach (var node in inspectableGraph)
                {
                    workflow.Add(node);
                }
                addConnection();
                UpdateGraphLayout();
            },
            () =>
            {
                removeConnection();
                foreach (var node in inspectableGraph.TopologicalSort())
                {
                    workflow.Remove(node);
                }
                UpdateGraphLayout();
            });
        }

        int GetEdgeConnectionIndex(ExpressionBuilderParameter parameter)
        {
            var connectionIndexString = parameter.Value.Substring(ExpressionBuilderParameter.Source.Length);
            return string.IsNullOrEmpty(connectionIndexString) ? 0 : int.Parse(connectionIndexString);
        }

        void IncrementEdgeValue(ExpressionBuilderParameter parameter)
        {
            parameter.Value = ExpressionBuilderParameter.Source + (GetEdgeConnectionIndex(parameter) + 1);
        }

        void DecrementEdgeValue(ExpressionBuilderParameter parameter)
        {
            var connectionIndex = GetEdgeConnectionIndex(parameter) - 1;
            parameter.Value = ExpressionBuilderParameter.Source;
            if (connectionIndex > 0) parameter.Value += connectionIndex;
        }

        public void DeleteGraphNodes(IEnumerable<GraphNode> nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException("nodes");
            }

            Action command = () => { };
            Action undo = () => { };
            foreach (var node in nodes)
            {
                Action addEdge = () => { };
                Action removeEdge = () => { };

                var workflowNode = GetGraphNodeTag(node);
                var inspectNode = workflowNode.Successors.Single().Node;

                var predecessorEdges = workflow.PredecessorEdges(workflowNode).ToArray();
                var siblingEdgesAfter = (from edge in inspectNode.Successors
                                         from siblingEdge in workflow.PredecessorEdges(edge.Node)
                                         where siblingEdge.Item2.Label.Value.CompareTo(edge.Label.Value) > 0
                                         select siblingEdge.Item2)
                                         .ToArray();

                var simplePredecessor = (predecessorEdges.Length == 1 && predecessorEdges[0].Item1.Successors.Count == 1);
                var simpleSuccessor = (inspectNode.Successors.Count == 1 && workflow.Predecessors(inspectNode.Successors[0].Node).Count() == 1);
                if (simplePredecessor || simpleSuccessor)
                {
                    addEdge = () =>
                    {
                        foreach (var predecessor in predecessorEdges)
                        {
                            foreach (var successor in inspectNode.Successors)
                            {
                                if (workflow.Successors(predecessor.Item1).Contains(successor.Node)) continue;
                                if (simplePredecessor) workflow.AddEdge(predecessor.Item1, successor.Node, successor.Label);
                                else workflow.SetEdge(predecessor.Item1, predecessor.Item3, successor.Node, predecessor.Item2.Label);
                            }
                        }
                    };

                    removeEdge = () =>
                    {
                        foreach (var predecessor in predecessorEdges)
                        {
                            foreach (var successor in inspectNode.Successors)
                            {
                                if (simplePredecessor) workflow.RemoveEdge(predecessor.Item1, successor.Node, successor.Label);
                                else workflow.RemoveEdge(predecessor.Item1, successor.Node, predecessor.Item2.Label);
                            }
                        }
                    };
                }

                Action removeNode = () =>
                {
                    workflow.Remove(inspectNode);
                    workflow.Remove(workflowNode);
                    foreach (var sibling in siblingEdgesAfter)
                    {
                        DecrementEdgeValue(sibling.Label);
                    }
                };

                Action addNode = () =>
                {
                    workflow.Add(workflowNode);
                    workflow.Add(inspectNode);
                    workflow.AddEdge(workflowNode, inspectNode, new ExpressionBuilderParameter());
                    foreach (var edge in predecessorEdges)
                    {
                        edge.Item1.Successors.Insert(edge.Item3, edge.Item2);
                    }

                    foreach (var sibling in siblingEdgesAfter)
                    {
                        IncrementEdgeValue(sibling.Label);
                    }
                };

                var previousCommand = command;
                command = () =>
                {
                    previousCommand();
                    addEdge();
                    removeNode();
                };

                var previousUndo = undo;
                undo = () =>
                {
                    addNode();
                    removeEdge();
                    previousUndo();
                };
            }

            commandExecutor.Execute(
            () =>
            {
                command();
                UpdateGraphLayout();
            },
            () =>
            {
                undo();
                UpdateGraphLayout();
            });
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

                    var visualizer = visualizerDialog.Visualizer;
                    var visualizerSettings = new XDocument();
                    var serializer = new XmlSerializer(visualizer.GetType());
                    using (var writer = visualizerSettings.CreateWriter())
                    {
                        serializer.Serialize(writer, visualizer);
                    }
                    var root = visualizerSettings.Root;
                    root.Remove();
                    var xsdAttribute = root.Attribute(XsdAttributeName);
                    if (xsdAttribute != null) xsdAttribute.Remove();
                    var xsiAttribute = root.Attribute(XsiAttributeName);
                    if (xsiAttribute != null) xsiAttribute.Remove();
                    dialogSettings.VisualizerSettings = root;
                    visualizerLayout.DialogSettings.Add(dialogSettings);
                }
            }

            visualizerMapping = null;
        }

        private void UpdateGraphLayout()
        {
            workflowGraphView.Nodes = workflow.FromInspectableGraph(false)
                .LongestPathLayering()
                .EnsureLayerPriority()
                .SortLayeringByConnectionKey()
                .ToList();
            workflowGraphView.Invalidate();
        }

        #endregion

        #region Controller

        private void OnDragFileDrop(DragEventArgs e)
        {
            if (workflowGraphView.ParentForm.Owner == null &&
                        (e.KeyState & AltModifier) == 0)
            {
                e.Effect = DragDropEffects.Link;
            }
            else e.Effect = DragDropEffects.Copy;
        }

        private void ClearDragDrop()
        {
            dragSelection = null;
            dragHighlight = null;
        }

        private void workflowGraphView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var path = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                if (path != null && path.Length > 0 &&
                    Path.GetExtension(path[0]) == BonsaiExtension &&
                    File.Exists(path[0]))
                {
                    OnDragFileDrop(e);
                }
            }
            else if (e.Data.GetDataPresent(typeof(GraphNode)))
            {
                var graphViewNode = (GraphNode)e.Data.GetData(typeof(GraphNode));
                var node = GetGraphNodeTag(graphViewNode, false);
                if (node != null && workflow.Contains(node))
                {
                    dragSelection = workflowGraphView.SelectedNodes;
                    dragHighlight = graphViewNode;
                }
            }
        }

        void workflowGraphView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Effect != DragDropEffects.None && e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                OnDragFileDrop(e);
            }

            if (dragSelection != null)
            {
                var dragLocation = workflowGraphView.PointToClient(new Point(e.X, e.Y));
                var highlight = workflowGraphView.GetNodeAt(dragLocation);
                if (highlight != dragHighlight)
                {
                    if (highlight != null)
                    {
                        e.Effect = ValidConnection(dragSelection, highlight)
                            ? DragDropEffects.Link
                            : DragDropEffects.None;
                    }
                    else e.Effect = DragDropEffects.None;
                    dragHighlight = highlight;
                }
            }
        }

        void workflowGraphView_DragLeave(object sender, EventArgs e)
        {
            ClearDragDrop();
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
                        ConnectGraphNodes(dragSelection, linkNode);
                    }
                }
            }

            if (e.Effect != DragDropEffects.None)
            {
                var parentForm = workflowGraphView.ParentForm;
                if (parentForm != null && !parentForm.Focused) parentForm.Activate();
                workflowGraphView.Select();
            }

            ClearDragDrop();
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
                DeleteGraphNodes(selectionModel.SelectedNodes);
            }

            if (e.KeyCode == Keys.Return)
            {
                if (e.Modifiers == Keys.Control)
                {
                    LaunchWorkflowView(workflowGraphView.SelectedNode);
                }
                else if (editorService.WorkflowRunning)
                {
                    LaunchVisualizer(workflowGraphView.SelectedNode);
                }
                else if (selectionModel.SelectedNodes.Any() && workflowGraphView.CursorNode != null &&
                         ValidConnection(selectionModel.SelectedNodes, workflowGraphView.CursorNode))
                {
                    ConnectGraphNodes(selectionModel.SelectedNodes, workflowGraphView.CursorNode);
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

            if (e.KeyCode == Keys.Z && e.Modifiers.HasFlag(Keys.Control))
            {
                editorService.Undo();
            }

            if (e.KeyCode == Keys.Y && e.Modifiers.HasFlag(Keys.Control))
            {
                editorService.Redo();
            }

            if (e.KeyCode == Keys.X && e.Modifiers.HasFlag(Keys.Control))
            {
                editorService.StoreWorkflowElements(selectionModel.SelectedNodes.ToWorkflowBuilder());
                DeleteGraphNodes(selectionModel.SelectedNodes);
            }

            if (e.KeyCode == Keys.C && e.Modifiers.HasFlag(Keys.Control))
            {
                editorService.StoreWorkflowElements(selectionModel.SelectedNodes.ToWorkflowBuilder());
            }

            if (e.KeyCode == Keys.V && e.Modifiers.HasFlag(Keys.Control))
            {
                var builder = editorService.RetrieveWorkflowElements();
                if (builder.Workflow.Count > 0)
                {
                    var branch = e.Modifiers.HasFlag(BranchModifier);
                    var predecessor = e.Modifiers.HasFlag(PredecessorModifier) ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
                    InsertGraphElements(builder.Workflow, predecessor, branch);
                }
            }
        }

        private void workflowGraphView_SelectedNodeChanged(object sender, EventArgs e)
        {
            selectionModel.UpdateSelection(this);
        }

        private void workflowGraphView_NodeMouseDoubleClick(object sender, GraphNodeMouseEventArgs e)
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
