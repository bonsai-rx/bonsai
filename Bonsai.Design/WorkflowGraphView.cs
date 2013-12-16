using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bonsai.Expressions;
using System.Xml.Linq;
using System.IO;
using Bonsai.Dag;
using System.Xml.Serialization;

namespace Bonsai.Design
{
    public partial class WorkflowGraphView : UserControl
    {
        static readonly XName XsdAttributeName = ((XNamespace)"http://www.w3.org/2000/xmlns/") + "xsd";
        static readonly XName XsiAttributeName = ((XNamespace)"http://www.w3.org/2000/xmlns/") + "xsi";
        const string XsdAttributeValue = "http://www.w3.org/2001/XMLSchema";
        const string XsiAttributeValue = "http://www.w3.org/2001/XMLSchema-instance";

        const int RightMouseButton = 0x2;
        const int ShiftModifier = 0x4;
        const int CtrlModifier = 0x8;
        const int AltModifier = 0x20;
        public const Keys BranchModifier = Keys.Alt;
        public const Keys PredecessorModifier = Keys.Shift;
        public const string BonsaiExtension = ".bonsai";

        int dragKeyState;
        GraphNode dragHighlight;
        IEnumerable<GraphNode> dragSelection;
        CommandExecutor commandExecutor;
        ExpressionBuilderGraph workflow;
        WorkflowSelectionModel selectionModel;
        IWorkflowEditorService editorService;
        Dictionary<ExpressionBuilder, VisualizerDialogLauncher> visualizerMapping;
        Dictionary<WorkflowExpressionBuilder, WorkflowEditorLauncher> workflowEditorMapping;
        ExpressionBuilderTypeConverter builderConverter;
        VisualizerLayout visualizerLayout;
        IServiceProvider serviceProvider;

        public WorkflowGraphView(IServiceProvider provider)
        {
            InitializeComponent();
            serviceProvider = provider;
            commandExecutor = (CommandExecutor)provider.GetService(typeof(CommandExecutor));
            selectionModel = (WorkflowSelectionModel)provider.GetService(typeof(WorkflowSelectionModel));
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
            builderConverter = new ExpressionBuilderTypeConverter();
            workflowEditorMapping = new Dictionary<WorkflowExpressionBuilder, WorkflowEditorLauncher>();

            graphView.HandleDestroyed += graphView_HandleDestroyed;
            editorService.WorkflowStarted += editorService_WorkflowStarted;
        }

        public GraphView GraphView
        {
            get { return graphView; }
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

        private void UpdateEditorMapping()
        {
            var missingEditors = (from mapping in workflowEditorMapping
                                  where !workflow.Any(node => node.Value == mapping.Key)
                                  select mapping)
                                  .ToArray();
            foreach (var mapping in missingEditors)
            {
                mapping.Value.Hide();
                workflowEditorMapping.Remove(mapping.Key);
            }
        }

        private void ClearEditorMapping()
        {
            foreach (var mapping in workflowEditorMapping)
            {
                mapping.Value.Hide();
            }

            workflowEditorMapping.Clear();
        }

        private VisualizerDialogLauncher CreateVisualizerLauncher(ExpressionBuilder builder, InspectBuilder inspectBuilder, GraphNode graphNode)
        {
            var workflowElementType = ExpressionBuilder.GetWorkflowElement(builder).GetType();
            var visualizerType = editorService.GetTypeVisualizer(workflowElementType) ??
                                 editorService.GetTypeVisualizer(inspectBuilder.ObservableType) ??
                                 editorService.GetTypeVisualizer(typeof(object));

            DialogTypeVisualizer visualizer = null;
            var layoutSettings = visualizerLayout != null
                ? visualizerLayout.DialogSettings.FirstOrDefault(xs => xs.Tag == graphNode.Value)
                : null;
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
            launcher.Text = builderConverter.ConvertToString(builder);
            if (layoutSettings != null)
            {
                var mashupVisualizer = launcher.Visualizer as DialogMashupVisualizer;
                if (mashupVisualizer != null)
                {
                    foreach (var mashup in layoutSettings.Mashups)
                    {
                        if (mashup < 0 || mashup >= visualizerLayout.DialogSettings.Count) continue;
                        var mashupNode = graphView.Nodes
                            .SelectMany(xs => xs)
                            .FirstOrDefault(node => node.Value == visualizerLayout.DialogSettings[mashup].Tag);
                        if (mashupNode != null)
                        {
                            launcher.CreateMashup(mashupNode, editorService);
                        }
                    }
                }

                launcher.Bounds = layoutSettings.Bounds;
                if (layoutSettings.Visible)
                {
                    launcher.Show(graphView, serviceProvider);
                }
            }

            return launcher;
        }

        private void InitializeVisualizerMapping()
        {
            if (workflow == null) return;
            visualizerMapping = (from node in workflow
                                 let inspectBuilder = (InspectBuilder)node.Value
                                 let graphNode = graphView.Nodes.SelectMany(layer => layer).First(n => n.Value == inspectBuilder)
                                 let key = (ExpressionBuilder)graphNode.Value
                                 select new { key, inspectBuilder, graphNode })
                                 .ToDictionary(mapping => mapping.key,
                                               mapping => CreateVisualizerLauncher(mapping.key, mapping.inspectBuilder, mapping.graphNode));
        }

        private ExpressionBuilder GetGraphNodeBuilder(GraphNode node)
        {
            return ((InspectBuilder)node.Value).Builder;
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
            if (elementType == ElementCategory.Source ||
                elementType == ElementCategory.Property)
            {
                if (closestNode != null &&
                    !(((InspectBuilder)closestNode.Value).Builder is SourceBuilder) &&
                    !workflow.Predecessors(closestNode).Any())
                {
                    var parameter = new ExpressionBuilderParameter();
                    var edge = Edge.Create(closestNode, parameter);
                    addConnection = () => workflow.AddEdge(sinkNode, edge);
                    removeConnection = () => workflow.RemoveEdge(sinkNode, edge);
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
                            var predecessorEdge = predecessor.Item2;
                            var predecessorNode = predecessor.Item1;
                            var edgeIndex = predecessor.Item3;
                            addConnection += () => { workflow.SetEdge(predecessorNode, edgeIndex, sourceNode, predecessorEdge.Label); };
                            removeConnection += () => { workflow.SetEdge(predecessorNode, edgeIndex, predecessorEdge.Target, predecessorEdge.Label); };
                        }
                    }

                    // After dealing with predecessors, we just create an edge to the selected node
                    var edge = Edge.Create(closestNode, parameter);
                    addConnection += () => { workflow.AddEdge(sinkNode, edge); };
                    removeConnection += () => { workflow.RemoveEdge(sinkNode, edge); };
                }
                else
                {
                    if (!branch && closestNode.Successors.Count > 0)
                    {
                        // If we are not creating a new branch, the new node will inherit all branches of selected node
                        var edge = Edge.Create(sourceNode, parameter);
                        var oldSuccessors = closestNode.Successors.ToArray();
                        addConnection = () =>
                        {
                            foreach (var successor in oldSuccessors)
                            {
                                workflow.RemoveEdge(closestNode, successor);
                                workflow.AddEdge(sinkNode, successor);
                            }
                            workflow.AddEdge(closestNode, edge);
                        };

                        removeConnection = () =>
                        {
                            foreach (var successor in oldSuccessors)
                            {
                                workflow.RemoveEdge(sinkNode, successor);
                                workflow.AddEdge(closestNode, successor);
                            }
                            workflow.RemoveEdge(closestNode, edge);
                        };
                    }
                    else
                    {
                        // Otherwise, just create the new branch
                        var edge = Edge.Create(sourceNode, parameter);
                        addConnection = () => { workflow.AddEdge(closestNode, edge); };
                        removeConnection = () => { workflow.RemoveEdge(closestNode, edge); };
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
                var node = GetGraphNodeTag(sourceNode, false);
                if (target == node || node.Successors.Any(edge => edge.Target == target))
                {
                    reject = true;
                    break;
                }

                var builder = GetGraphNodeBuilder(graphViewTarget);
                if (connectionCount++ >= target.Value.ArgumentRange.UpperBound &&
                    !Attribute.IsDefined(builder.GetType(), typeof(PropertyMappingAttribute), true) ||
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
                var source = GetGraphNodeTag(graphViewSource);
                var connection = string.Empty;
                connection = ExpressionBuilderParameter.Source;
                connection += connectionIndex + 1;

                var parameter = new ExpressionBuilderParameter(connection);
                var edge = Edge.Create(target, parameter);
                addConnection += () => workflow.AddEdge(source, edge);
                removeConnection += () => workflow.RemoveEdge(source, edge);
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

        ExpressionBuilder CreateBuilder(TreeNode typeNode)
        {
            var typeName = typeNode.Name;
            var elementCategory = (ElementCategory)typeNode.Tag;
            return CreateBuilder(typeName, elementCategory);
        }

        ExpressionBuilder CreateBuilder(string typeName, ElementCategory elementCategory)
        {
            var type = Type.GetType(typeName);
            if (type == null)
            {
                throw new ArgumentException("The specified type could not be found.", "typeName");
            }

            ExpressionBuilder builder;
            if (!type.IsSubclassOf(typeof(ExpressionBuilder)))
            {
                var element = Activator.CreateInstance(type);
                builder = ExpressionBuilder.FromWorkflowElement(element, elementCategory);
            }
            else builder = (ExpressionBuilder)Activator.CreateInstance(type);
            return builder;
        }

        public void CreateGraphNode(TreeNode typeNode, GraphNode closestGraphViewNode, CreateGraphNodeType nodeType, bool branch)
        {
            var builder = CreateBuilder(typeNode);
            var elementCategory = (ElementCategory)typeNode.Tag;
            CreateGraphNode(builder, elementCategory, closestGraphViewNode, nodeType, branch);
        }

        public void CreateGraphNode(string typeName, ElementCategory elementCategory, GraphNode closestGraphViewNode, CreateGraphNodeType nodeType, bool branch)
        {
            var builder = CreateBuilder(typeName, elementCategory);
            CreateGraphNode(builder, elementCategory, closestGraphViewNode, nodeType, branch);
        }

        public void CreateGraphNode(ExpressionBuilder builder, ElementCategory elementCategory, GraphNode closestGraphViewNode, CreateGraphNodeType nodeType, bool branch)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            var inspectBuilder = new InspectBuilder(builder);
            var inspectNode = new Node<ExpressionBuilder, ExpressionBuilderParameter>(inspectBuilder);
            var inspectParameter = new ExpressionBuilderParameter();
            Action addNode = () => { workflow.Add(inspectNode); };
            Action removeNode = () => { workflow.Remove(inspectNode); };

            var closestNode = closestGraphViewNode != null ? GetGraphNodeTag(closestGraphViewNode) : null;
            var insertCommands = GetInsertGraphNodeCommands(inspectNode, inspectNode, elementCategory, closestNode, nodeType, branch);
            var addConnection = insertCommands.Item1;
            var removeConnection = insertCommands.Item2;
            commandExecutor.Execute(
            () =>
            {
                addNode();
                addConnection();
                UpdateGraphLayout();
                graphView.SelectedNode = graphView.Nodes.SelectMany(layer => layer).First(n => GetGraphNodeTag(n).Value == inspectBuilder);
            },
            () =>
            {
                removeConnection();
                removeNode();
                UpdateGraphLayout();
                graphView.SelectedNode = graphView.Nodes.SelectMany(layer => layer).FirstOrDefault(n => closestNode != null ? GetGraphNodeTag(n).Value == closestNode.Value : false);
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

        void DeleteGraphNode(GraphNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            Action addEdge = () => { };
            Action removeEdge = () => { };

            var workflowNode = GetGraphNodeTag(node);
            var predecessorEdges = workflow.PredecessorEdges(workflowNode).ToArray();
            var siblingEdgesAfter = (from edge in workflowNode.Successors
                                     from siblingEdge in workflow.PredecessorEdges(edge.Target)
                                     where siblingEdge.Item2.Label.Value.CompareTo(edge.Label.Value) > 0
                                     select siblingEdge.Item2)
                                     .ToArray();

            var simplePredecessor = (predecessorEdges.Length == 1 && predecessorEdges[0].Item1.Successors.Count == 1);
            var simpleSuccessor = (workflowNode.Successors.Count == 1 && workflow.Predecessors(workflowNode.Successors[0].Target).Count() == 1);
            var replaceEdge = simplePredecessor || simpleSuccessor;
            if (replaceEdge)
            {
                var replacedEdges = (from predecessor in predecessorEdges
                                     from successor in workflowNode.Successors
                                     where !workflow.Successors(predecessor.Item1).Contains(successor.Target)
                                     select new
                                     {
                                         predecessor = predecessor.Item1,
                                         edgeIndex = predecessor.Item3,
                                         edge = simplePredecessor
                                            ? successor
                                            : Edge.Create(successor.Target, predecessor.Item2.Label)
                                     })
                                     .ToArray();

                addEdge = () =>
                {
                    Array.ForEach(replacedEdges, replacedEdge =>
                    {
                        if (simplePredecessor) workflow.AddEdge(replacedEdge.predecessor, replacedEdge.edge);
                        else workflow.SetEdge(replacedEdge.predecessor, replacedEdge.edgeIndex, replacedEdge.edge);
                    });
                };

                removeEdge = () =>
                {
                    Array.ForEach(replacedEdges, replacedEdge =>
                    {
                        workflow.RemoveEdge(replacedEdge.predecessor, replacedEdge.edge);
                    });
                };
            }

            Action removeNode = () =>
            {
                workflow.Remove(workflowNode);
                if (!replaceEdge)
                {
                    foreach (var sibling in siblingEdgesAfter)
                    {
                        sibling.Label.DecrementEdgeValue();
                    }
                }
            };

            Action addNode = () =>
            {
                workflow.Add(workflowNode);
                foreach (var edge in predecessorEdges)
                {
                    edge.Item1.Successors.Insert(edge.Item3, edge.Item2);
                }

                if (!replaceEdge)
                {
                    foreach (var sibling in siblingEdgesAfter)
                    {
                        sibling.Label.IncrementEdgeValue();
                    }
                }
            };

            commandExecutor.Execute(() =>
            {
                addEdge();
                removeNode();
            },
            () =>
            {
                addNode();
                removeEdge();
            });
        }

        public void DeleteGraphNodes(IEnumerable<GraphNode> nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException("nodes");
            }

            if (!nodes.Any()) return;
            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(() => { }, UpdateGraphLayout);
            foreach (var node in nodes)
            {
                DeleteGraphNode(node);
            }

            commandExecutor.Execute(UpdateGraphLayout, () => { });
            commandExecutor.EndCompositeCommand();
        }

        public void GroupGraphNodes(IEnumerable<GraphNode> nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException("nodes");
            }

            if (!nodes.Any()) return;
            GraphNode linkNode = null;
            GraphNode replacementNode = null;
            var nodeType = CreateGraphNodeType.Successor;
            var workflowBuilder = nodes.ToWorkflowBuilder();
            var source = workflowBuilder.Workflow.Sources().First();
            var sourceNode = graphView.Nodes.SelectMany(layer => layer).Single(node => GetGraphNodeBuilder(node) == source.Value);
            var predecessors = graphView.Nodes
                .SelectMany(layer => layer)
                .Where(node => node.Value != null && node.Successors.Any(edge => edge.Node.Value == sourceNode.Value))
                .ToArray();
            if (predecessors.Length == 1)
            {
                var workflowInput = new WorkflowInputBuilder();
                var inputNode = workflowBuilder.Workflow.Add(workflowInput);
                workflowBuilder.Workflow.AddEdge(inputNode, source, new ExpressionBuilderParameter());
                linkNode = predecessors[0];
                replacementNode = sourceNode;
            }

            var sink = workflowBuilder.Workflow.Sinks().First();
            var sinkNode = graphView.Nodes.SelectMany(layer => layer).Single(node => GetGraphNodeBuilder(node) == sink.Value);
            var successors = sinkNode.Successors.Select(edge => edge.Node).ToArray();
            if (successors.Length == 1)
            {
                var workflowOutput = new WorkflowOutputBuilder();
                var outputNode = workflowBuilder.Workflow.Add(workflowOutput);
                workflowBuilder.Workflow.AddEdge(sink, outputNode, new ExpressionBuilderParameter());
                if (linkNode == null)
                {
                    linkNode = successors[0];
                    replacementNode = sinkNode;
                    nodeType = CreateGraphNodeType.Predecessor;
                }
            }

            var workflowExpressionBuilder = new NestedWorkflowExpressionBuilder(workflowBuilder.Workflow.ToInspectableGraph());
            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(() => { }, UpdateGraphLayout);
            foreach (var node in nodes.Where(n => n != replacementNode))
            {
                DeleteGraphNode(node);
            }

            CreateGraphNode(workflowExpressionBuilder,
                            ElementCategory.Nested,
                            replacementNode,
                            nodeType,
                            branch: false);
            if (replacementNode != null) DeleteGraphNode(replacementNode);
            commandExecutor.Execute(() =>
            {
                UpdateGraphLayout();
                graphView.SelectedNode = graphView.Nodes.SelectMany(layer => layer).First(n => GetGraphNodeBuilder(n) == workflowExpressionBuilder);
            },
            () => { });
            commandExecutor.EndCompositeCommand();
        }

        public void CutToClipboard()
        {
            CopyToClipboard();
            DeleteGraphNodes(selectionModel.SelectedNodes);
        }

        public void CopyToClipboard()
        {
            editorService.StoreWorkflowElements(selectionModel.SelectedNodes.ToWorkflowBuilder());
        }

        public void PasteFromClipboard()
        {
            var builder = editorService.RetrieveWorkflowElements();
            if (builder.Workflow.Count > 0)
            {
                var branch = Control.ModifierKeys.HasFlag(BranchModifier);
                var predecessor = Control.ModifierKeys.HasFlag(PredecessorModifier) ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
                InsertGraphElements(builder.Workflow, predecessor, branch);
            }
        }

        public void SetWorkflowProperty(string name, string value)
        {
            var property = (from node in workflow
                            let workflowProperty = ExpressionBuilder.GetWorkflowElement(node.Value) as WorkflowProperty
                            where workflowProperty != null && workflowProperty.Name == name
                            select workflowProperty)
                            .FirstOrDefault();
            if (property != null)
            {
                var propertyDescriptor = TypeDescriptor.GetProperties(property).Find("Value", false);
                if (propertyDescriptor != null)
                {
                    var propertyValue = propertyDescriptor.Converter.ConvertFromString(value);
                    propertyDescriptor.SetValue(property, propertyValue);
                }
            }
        }

        public GraphNode FindGraphNode(object value)
        {
            return graphView.Nodes.SelectMany(layer => layer).FirstOrDefault(n => n.Value == value);
        }

        public void LaunchVisualizer(GraphNode node)
        {
            var visualizerDialog = GetVisualizerDialogLauncher(node);
            if (visualizerDialog != null)
            {
                visualizerDialog.Show(graphView, serviceProvider);
            }
        }

        public void LaunchWorkflowView(GraphNode node)
        {
            var editorLauncher = GetWorkflowEditorLauncher(node);
            if (editorLauncher != null)
            {
                editorLauncher.Show(graphView, serviceProvider);
            }
        }

        public VisualizerDialogLauncher GetVisualizerDialogLauncher(GraphNode node)
        {
            VisualizerDialogLauncher visualizerDialog = null;
            if (visualizerMapping != null && node != null)
            {
                var expressionBuilder = node.Value as ExpressionBuilder;
                if (expressionBuilder != null)
                {
                    visualizerMapping.TryGetValue(expressionBuilder, out visualizerDialog);
                }
            }

            return visualizerDialog;
        }

        public WorkflowEditorLauncher GetWorkflowEditorLauncher(GraphNode node)
        {
            var workflowExpressionBuilder = GetGraphNodeBuilder(node) as WorkflowExpressionBuilder;
            if (workflowExpressionBuilder != null)
            {
                WorkflowEditorLauncher editorLauncher;
                if (!workflowEditorMapping.TryGetValue(workflowExpressionBuilder, out editorLauncher))
                {
                    editorLauncher = new WorkflowEditorLauncher(workflow, workflowExpressionBuilder);
                    workflowEditorMapping.Add(workflowExpressionBuilder, editorLauncher);
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
                var graphNode = graphView.Nodes.SelectMany(layer => layer).First(n => n.Value == node.Value);
                layoutSettings.Current.Tag = graphNode.Value;

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
                    var workflowExpressionBuilder = ExpressionBuilder.GetWorkflowElement(mapping.Key) as WorkflowExpressionBuilder;
                    if (workflowExpressionBuilder != null &&
                        workflowEditorMapping.TryGetValue(workflowExpressionBuilder, out editorLauncher))
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
            graphView.Nodes = workflow
                .LongestPathLayering()
                .EnsureLayerPriority()
                .SortLayeringByConnectionKey()
                .ToList();
            graphView.Invalidate();
            UpdateEditorMapping();
        }

        #endregion

        #region Controller

        private void OnDragFileDrop(DragEventArgs e)
        {
            if (graphView.ParentForm.Owner == null &&
                (e.KeyState & CtrlModifier) != 0)
            {
                e.Effect = DragDropEffects.Link;
            }
            else e.Effect = DragDropEffects.Copy;
        }

        private void ClearDragDrop()
        {
            dragKeyState = 0;
            dragSelection = null;
            dragHighlight = null;
        }

        private void graphView_DragEnter(object sender, DragEventArgs e)
        {
            dragKeyState = e.KeyState;
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
                    dragSelection = graphView.SelectedNodes;
                    dragHighlight = graphViewNode;
                }
            }
        }

        private void graphView_DragOver(object sender, DragEventArgs e)
        {
            dragKeyState = e.KeyState;
            if (e.Effect != DragDropEffects.None && e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                OnDragFileDrop(e);
            }

            if (dragSelection != null)
            {
                var dragLocation = graphView.PointToClient(new Point(e.X, e.Y));
                var highlight = graphView.GetNodeAt(dragLocation);
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

        private void graphView_DragLeave(object sender, EventArgs e)
        {
            ClearDragDrop();
        }

        private void graphView_DragDrop(object sender, DragEventArgs e)
        {
            if (((dragKeyState ^ e.KeyState) & RightMouseButton) != 0)
            {
                contextMenuStrip.Show(e.X, e.Y);
            }
            else
            {
                var dropLocation = graphView.PointToClient(new Point(e.X, e.Y));
                if (e.Effect == DragDropEffects.Copy)
                {
                    var branch = (e.KeyState & AltModifier) != 0;
                    var predecessor = (e.KeyState & ShiftModifier) != 0 ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
                    var linkNode = graphView.GetNodeAt(dropLocation) ?? graphView.SelectedNode;
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
                        var linkNode = graphView.GetNodeAt(dropLocation);
                        if (linkNode != null)
                        {
                            ConnectGraphNodes(dragSelection, linkNode);
                        }
                    }
                }
            }

            if (e.Effect != DragDropEffects.None)
            {
                var parentForm = graphView.ParentForm;
                if (parentForm != null && !parentForm.Focused) parentForm.Activate();
                graphView.Select();
            }

            ClearDragDrop();
        }

        private void graphView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var selectedNode = e.Item as GraphNode;
            if (selectedNode != null)
            {
                graphView.DoDragDrop(selectedNode, DragDropEffects.Link);
            }
        }

        private void graphView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteGraphNodes(selectionModel.SelectedNodes);
            }

            if (e.KeyCode == Keys.Return)
            {
                if (e.Modifiers == Keys.Control)
                {
                    LaunchWorkflowView(graphView.SelectedNode);
                }
                else if (editorService.WorkflowRunning)
                {
                    LaunchVisualizer(graphView.SelectedNode);
                }
                else if (selectionModel.SelectedNodes.Any() && graphView.CursorNode != null &&
                         ValidConnection(selectionModel.SelectedNodes, graphView.CursorNode))
                {
                    ConnectGraphNodes(selectionModel.SelectedNodes, graphView.CursorNode);
                }
            }

            if (e.KeyCode == Keys.Back && e.Modifiers == Keys.Control)
            {
                var owner = graphView.ParentForm.Owner;
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
                CutToClipboard();
            }

            if (e.KeyCode == Keys.C && e.Modifiers.HasFlag(Keys.Control))
            {
                CopyToClipboard();
            }

            if (e.KeyCode == Keys.V && e.Modifiers.HasFlag(Keys.Control))
            {
                PasteFromClipboard();
            }

            if (e.KeyCode == Keys.G && e.Modifiers.HasFlag(Keys.Control))
            {
                GroupGraphNodes(selectionModel.SelectedNodes);
            }

            if (e.KeyCode == Keys.R && e.Control)
            {
                editorService.StartWorkflow();
            }

            if (e.KeyCode == Keys.T && e.Control)
            {
                editorService.StopWorkflow();
            }
        }

        private void graphView_SelectedNodeChanged(object sender, EventArgs e)
        {
            selectionModel.UpdateSelection(this);
        }

        private void graphView_NodeMouseDoubleClick(object sender, GraphNodeMouseEventArgs e)
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

        private void graphView_HandleDestroyed(object sender, EventArgs e)
        {
            editorService.WorkflowStarted -= editorService_WorkflowStarted;
        }

        private void editorService_WorkflowStarted(object sender, EventArgs e)
        {
            InitializeVisualizerMapping();
        }

        #endregion

        #region Context Menu

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CutToClipboard();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyToClipboard();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteFromClipboard();
        }

        private void groupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GroupGraphNodes(selectionModel.SelectedNodes);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteGraphNodes(selectionModel.SelectedNodes);
        }

        #endregion
    }

    public enum CreateGraphNodeType
    {
        Successor,
        Predecessor
    }
}
