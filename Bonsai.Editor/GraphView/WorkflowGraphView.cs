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
using System.Linq.Expressions;
using System.Windows.Forms.Design;

namespace Bonsai.Design
{
    partial class WorkflowGraphView : UserControl
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
        Dictionary<InspectBuilder, VisualizerDialogLauncher> visualizerMapping;
        Dictionary<WorkflowExpressionBuilder, WorkflowEditorLauncher> workflowEditorMapping;
        ExpressionBuilderTypeConverter builderConverter;
        VisualizerLayout visualizerLayout;
        IServiceProvider serviceProvider;
        IUIService uiService;

        public WorkflowGraphView(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            InitializeComponent();
            serviceProvider = provider;
            uiService = (IUIService)provider.GetService(typeof(IUIService));
            commandExecutor = (CommandExecutor)provider.GetService(typeof(CommandExecutor));
            selectionModel = (WorkflowSelectionModel)provider.GetService(typeof(WorkflowSelectionModel));
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
            builderConverter = new ExpressionBuilderTypeConverter();
            workflowEditorMapping = new Dictionary<WorkflowExpressionBuilder, WorkflowEditorLauncher>();

            graphView.HandleDestroyed += graphView_HandleDestroyed;
            editorService.WorkflowStarted += editorService_WorkflowStarted;
        }

        internal WorkflowEditorLauncher Launcher { get; set; }

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
                UpdateEditorWorkflow(validateWorkflow: false);
            }
        }

        #region Model

        private Func<IWin32Window> CreateWindowOwnerSelectorDelegate()
        {
            var launcher = Launcher;
            return launcher != null ? (Func<IWin32Window>)(() => launcher.Owner) : () => graphView;
        }

        private Action CreateUpdateEditorMappingDelegate(Action<Dictionary<WorkflowExpressionBuilder, WorkflowEditorLauncher>> action)
        {
            var launcher = Launcher;
            return launcher != null
                ? (Action)(() => action(launcher.WorkflowGraphView.workflowEditorMapping))
                : () => action(workflowEditorMapping);
        }

        private Action CreateUpdateGraphViewDelegate(Action<GraphView> action)
        {
            var launcher = Launcher;
            return launcher != null
                ? (Action)(() => action(launcher.WorkflowGraphView.GraphView))
                : () => action(graphView);
        }

        private Action CreateUpdateGraphLayoutDelegate()
        {
            var launcher = Launcher;
            return launcher != null
                ? (Action)(() => launcher.WorkflowGraphView.UpdateGraphLayout())
                : UpdateGraphLayout;
        }

        private Func<bool> CreateUpdateGraphLayoutValidationDelegate()
        {
            var launcher = Launcher;
            return launcher != null
                ? (Func<bool>)(() => launcher.WorkflowGraphView.UpdateGraphLayout(validateWorkflow: true))
                : () => UpdateGraphLayout(validateWorkflow: true);
        }

        internal void CloseWorkflowEditorLauncher(WorkflowEditorLauncher editorLauncher)
        {
            var visible = editorLauncher.Visible;
            var serviceProvider = this.serviceProvider;
            var windowSelector = CreateWindowOwnerSelectorDelegate();
            commandExecutor.Execute(
                editorLauncher.Hide,
                () =>
                {
                    if (visible)
                    {
                        editorLauncher.Show(windowSelector(), serviceProvider);
                    }
                });
        }

        private void UpdateEditorWorkflow(bool validateWorkflow)
        {
            UpdateGraphLayout(validateWorkflow);
            if (editorService.WorkflowRunning)
            {
                InitializeVisualizerMapping();
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

        private IEnumerable<Type> GetTypeVisualizers(GraphNode graphNode)
        {
            var inspectBuilder = (InspectBuilder)graphNode.Value;
            var workflowElementType = ExpressionBuilder.GetWorkflowElement(inspectBuilder).GetType();
            foreach (var type in editorService.GetTypeVisualizers(workflowElementType))
            {
                yield return type;
            }

            var observableType = inspectBuilder.ObservableType;
            foreach (var type in editorService.GetTypeVisualizers(observableType))
            {
                yield return type;
            }

            foreach (var type in editorService.GetTypeVisualizers(typeof(object)))
            {
                yield return type;
            }
        }

        private VisualizerDialogLauncher CreateVisualizerLauncher(InspectBuilder inspectBuilder, GraphNode graphNode)
        {
            var workflowElementType = ExpressionBuilder.GetWorkflowElement(inspectBuilder).GetType();
            var visualizerTypes = GetTypeVisualizers(graphNode);

            Type visualizerType = null;
            var deserializeVisualizer = false;
            Func<DialogTypeVisualizer> visualizerFactory = null;
            var layoutSettings = GetLayoutSettings(graphNode.Value);
            if (layoutSettings != null && !string.IsNullOrEmpty(layoutSettings.VisualizerTypeName))
            {
                visualizerType = visualizerTypes.FirstOrDefault(type => type.FullName == layoutSettings.VisualizerTypeName);
                if (visualizerType != null && layoutSettings.VisualizerSettings != null)
                {
                    var root = layoutSettings.VisualizerSettings;
                    root.SetAttributeValue(XsdAttributeName, XsdAttributeValue);
                    root.SetAttributeValue(XsiAttributeName, XsiAttributeValue);
                    var serializer = new XmlSerializer(visualizerType);
                    using (var reader = layoutSettings.VisualizerSettings.CreateReader())
                    {
                        if (serializer.CanDeserialize(reader))
                        {
                            var visualizer = (DialogTypeVisualizer)serializer.Deserialize(reader);
                            visualizerFactory = () => visualizer;
                            deserializeVisualizer = true;
                        }
                    }
                }
            }

            visualizerType = visualizerType ?? visualizerTypes.First();
            if (visualizerFactory == null)
            {
                var visualizerActivation = Expression.New(visualizerType);
                visualizerFactory = Expression.Lambda<Func<DialogTypeVisualizer>>(visualizerActivation).Compile();
            }

            var launcher = new VisualizerDialogLauncher(inspectBuilder, visualizerFactory, this);
            launcher.Text = builderConverter.ConvertToString(inspectBuilder);
            if (deserializeVisualizer)
            {
                launcher = launcher.Visualizer.Value != null ? launcher : null;
            }
            return launcher;
        }

        private void InitializeVisualizerMapping()
        {
            if (workflow == null) return;
            visualizerMapping = (from node in workflow
                                 let key = (InspectBuilder)node.Value
                                 let graphNode = graphView.Nodes.SelectMany(layer => layer).First(n => n.Value == key)
                                 select new { key, graphNode })
                                 .ToDictionary(mapping => mapping.key,
                                               mapping => CreateVisualizerLauncher(mapping.key, mapping.graphNode));

            foreach (var mapping in visualizerMapping)
            {
                var key = mapping.Key;
                var visualizerLauncher = mapping.Value;
                var layoutSettings = GetLayoutSettings(key);
                if (layoutSettings != null)
                {
                    var visualizer = visualizerLauncher.Visualizer;
                    var mashupVisualizer = visualizer.IsValueCreated ? visualizer.Value as DialogMashupVisualizer : null;
                    if (mashupVisualizer != null)
                    {
                        foreach (var mashup in layoutSettings.Mashups)
                        {
                            if (mashup < 0 || mashup >= visualizerLayout.DialogSettings.Count) continue;
                            var dialogSettings = visualizerLayout.DialogSettings[mashup];
                            var mashupNode = graphView.Nodes
                                .SelectMany(xs => xs)
                                .FirstOrDefault(node => node.Value == dialogSettings.Tag);
                            if (mashupNode != null)
                            {
                                visualizerLauncher.CreateMashup(mashupNode, editorService);
                            }
                        }
                    }

                    visualizerLauncher.Bounds = layoutSettings.Bounds;
                    if (layoutSettings.Visible)
                    {
                        visualizerLauncher.Show(graphView, serviceProvider);
                    }
                }
            }
        }

        private static ExpressionBuilder GetGraphNodeBuilder(GraphNode node)
        {
            if (node != null)
            {
                return ExpressionBuilder.Unwrap((ExpressionBuilder)node.Value);
            }

            return null;
        }

        private static Node<ExpressionBuilder, ExpressionBuilderArgument> GetGraphNodeTag(ExpressionBuilderGraph workflow, GraphNode node)
        {
            return GetGraphNodeTag(workflow, node, true);
        }

        private static Node<ExpressionBuilder, ExpressionBuilderArgument> GetGraphNodeTag(ExpressionBuilderGraph workflow, GraphNode node, bool throwOnError)
        {
            while (node.Value == null)
            {
                var edge = (GraphEdge)node.Tag;
                node = edge.Node;
            }

            var nodeTag = (Node<ExpressionBuilder, ExpressionBuilderArgument>)node.Tag;
            if (throwOnError) return workflow.First(ns => ns.Value == nodeTag.Value);
            else return workflow.FirstOrDefault(ns => ns.Value == nodeTag.Value);
        }

        Tuple<Action, Action> GetInsertGraphNodeCommands(
            Node<ExpressionBuilder, ExpressionBuilderArgument> sourceNode,
            Node<ExpressionBuilder, ExpressionBuilderArgument> sinkNode,
            ElementCategory elementType,
            Node<ExpressionBuilder, ExpressionBuilderArgument> closestNode,
            CreateGraphNodeType nodeType,
            bool branch)
        {
            var workflow = this.workflow;
            Action addConnection = () => { };
            Action removeConnection = () => { };
            if (elementType == ElementCategory.Source ||
                elementType == ElementCategory.Property)
            {
                if (closestNode != null &&
                    !(ExpressionBuilder.Unwrap(closestNode.Value) is SourceBuilder) &&
                    !workflow.Predecessors(closestNode).Any())
                {
                    var parameter = new ExpressionBuilderArgument();
                    var edge = Edge.Create(closestNode, parameter);
                    addConnection = () => workflow.AddEdge(sinkNode, edge);
                    removeConnection = () => workflow.RemoveEdge(sinkNode, edge);
                }
            }
            else if (closestNode != null)
            {
                var parameter = new ExpressionBuilderArgument();
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
            var target = GetGraphNodeTag(workflow, graphViewTarget, false);
            var connectionCount = workflow.Predecessors(target).Count();
            foreach (var sourceNode in graphViewSources)
            {
                var node = GetGraphNodeTag(workflow, sourceNode, false);
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
            var workflow = this.workflow;
            Action addConnection = () => { };
            Action removeConnection = () => { };
            var target = GetGraphNodeTag(workflow, graphViewTarget);
            var connectionIndex = workflow.Predecessors(target).Count();
            foreach (var graphViewSource in graphViewSources)
            {
                var source = GetGraphNodeTag(workflow, graphViewSource);
                var parameter = new ExpressionBuilderArgument(connectionIndex);
                var edge = Edge.Create(target, parameter);
                addConnection += () => workflow.AddEdge(source, edge);
                removeConnection += () => workflow.RemoveEdge(source, edge);
                connectionIndex++;
            }

            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            commandExecutor.Execute(
            () =>
            {
                addConnection();
                updateGraphLayout();
            },
            () =>
            {
                removeConnection();
                updateGraphLayout();
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

            var workflow = this.workflow;
            var inspectBuilder = new InspectBuilder(builder);
            var inspectNode = new Node<ExpressionBuilder, ExpressionBuilderArgument>(inspectBuilder);
            var inspectParameter = new ExpressionBuilderArgument();
            Action addNode = () => { workflow.Add(inspectNode); };
            Action removeNode = () => { workflow.Remove(inspectNode); };

            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            var updateGraphLayoutValidation = CreateUpdateGraphLayoutValidationDelegate();
            var updateSelectedNode = CreateUpdateGraphViewDelegate(graphView =>
            {
                graphView.SelectedNode = graphView.Nodes.SelectMany(layer => layer).First(n => GetGraphNodeTag(workflow, n).Value == inspectBuilder);
            });

            var closestNode = closestGraphViewNode != null ? GetGraphNodeTag(workflow, closestGraphViewNode) : null;
            var restoreSelectedNode = CreateUpdateGraphViewDelegate(graphView =>
            {
                graphView.SelectedNode = graphView.Nodes.SelectMany(layer => layer).FirstOrDefault(n => closestNode != null ? GetGraphNodeTag(workflow, n).Value == closestNode.Value : false);
            });

            var insertCommands = GetInsertGraphNodeCommands(inspectNode, inspectNode, elementCategory, closestNode, nodeType, branch);
            var addConnection = insertCommands.Item1;
            var removeConnection = insertCommands.Item2;
            commandExecutor.Execute(
            () =>
            {
                addNode();
                addConnection();
                var validation = updateGraphLayoutValidation();
                if (validation)
                {
                    updateSelectedNode();
                }
            },
            () =>
            {
                removeConnection();
                removeNode();
                updateGraphLayout();
                restoreSelectedNode();
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
                var selectionNode = GetGraphNodeTag(workflow, selection);
                var source = inspectableGraph.Sources().FirstOrDefault();
                var sink = inspectableGraph.Sinks().FirstOrDefault();
                if (source != null && sink != null)
                {
                    var insertCommands = GetInsertGraphNodeCommands(source, sink, ElementCategory.Combinator, selectionNode, nodeType, branch);
                    addConnection = insertCommands.Item1;
                    removeConnection = insertCommands.Item2;
                }
            }

            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            commandExecutor.Execute(
            () =>
            {
                foreach (var node in inspectableGraph)
                {
                    workflow.Add(node);
                }
                addConnection();
                updateGraphLayout();
            },
            () =>
            {
                removeConnection();
                foreach (var node in inspectableGraph.TopologicalSort())
                {
                    workflow.Remove(node);
                }
                updateGraphLayout();
            });
        }

        void DeleteGraphNode(GraphNode node)
        {
            var workflow = this.workflow;
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            Action addEdge = () => { };
            Action removeEdge = () => { };

            var workflowNode = GetGraphNodeTag(workflow, node);
            var predecessorEdges = workflow.PredecessorEdges(workflowNode).ToArray();
            var siblingEdgesAfter = (from edge in workflowNode.Successors
                                     from siblingEdge in workflow.PredecessorEdges(edge.Target)
                                     where siblingEdge.Item2.Label.Index.CompareTo(edge.Label.Index) > 0
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
                        sibling.Label.Index--;
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
                        sibling.Label.Index++;
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

            var workflowExpressionBuilder = GetGraphNodeBuilder(node) as WorkflowExpressionBuilder;
            if (workflowExpressionBuilder != null)
            {
                RemoveEditorMapping(workflowExpressionBuilder);
            }
        }

        private void RemoveEditorMapping(WorkflowExpressionBuilder workflowExpressionBuilder)
        {
            WorkflowEditorLauncher editorLauncher;
            if (workflowEditorMapping.TryGetValue(workflowExpressionBuilder, out editorLauncher))
            {
                if (editorLauncher.Visible)
                {
                    var workflowGraphView = editorLauncher.WorkflowGraphView;
                    foreach (var node in workflowGraphView.workflow)
                    {
                        var nestedBuilder = ExpressionBuilder.Unwrap(node.Value) as WorkflowExpressionBuilder;
                        if (nestedBuilder != null)
                        {
                            workflowGraphView.RemoveEditorMapping(nestedBuilder);
                        }
                    }
                }

                CloseWorkflowEditorLauncher(editorLauncher);
                var removeEditorMapping = CreateUpdateEditorMappingDelegate(editorMapping => editorMapping.Remove(workflowExpressionBuilder));
                var addEditorMapping = CreateUpdateEditorMappingDelegate(editorMapping => editorMapping.Add(workflowExpressionBuilder, editorLauncher));
                commandExecutor.Execute(removeEditorMapping, addEditorMapping);
            }
        }

        public void DeleteGraphNodes(IEnumerable<GraphNode> nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException("nodes");
            }

            if (!nodes.Any()) return;
            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(() => { }, updateGraphLayout);
            foreach (var node in nodes)
            {
                DeleteGraphNode(node);
            }

            commandExecutor.Execute(updateGraphLayout, () => { });
            commandExecutor.EndCompositeCommand();
        }

        public void GroupGraphNodes(IEnumerable<GraphNode> nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException("nodes");
            }

            if (!nodes.Any()) return;
            var workflow = this.workflow;
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
                workflowBuilder.Workflow.AddEdge(inputNode, source, new ExpressionBuilderArgument());
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
                workflowBuilder.Workflow.AddEdge(sink, outputNode, new ExpressionBuilderArgument());
                if (linkNode == null)
                {
                    linkNode = successors[0];
                    replacementNode = sinkNode;
                    nodeType = CreateGraphNodeType.Predecessor;
                }
            }

            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            var workflowExpressionBuilder = new NestedWorkflowExpressionBuilder(workflowBuilder.Workflow.ToInspectableGraph());
            var updateSelectedNode = CreateUpdateGraphViewDelegate(localGraphView =>
            {
                localGraphView.SelectedNode = localGraphView.Nodes.SelectMany(layer => layer).First(n => GetGraphNodeBuilder(n) == workflowExpressionBuilder);
            });

            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(() => { }, updateGraphLayout);
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
                updateGraphLayout();
                updateSelectedNode();
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
            var visualizerLauncher = GetVisualizerDialogLauncher(node);
            if (visualizerLauncher != null)
            {
                visualizerLauncher.Show(graphView, serviceProvider);
            }
        }

        public void LaunchWorkflowView(GraphNode node, bool activate = true)
        {
            LaunchWorkflowView(node, null, Rectangle.Empty, activate);
        }

        public void LaunchWorkflowView(GraphNode node, VisualizerLayout editorLayout, Rectangle bounds, bool activate)
        {
            var workflowExpressionBuilder = GetGraphNodeBuilder(node) as WorkflowExpressionBuilder;
            if (workflowExpressionBuilder == null) return;

            bool composite = false;
            WorkflowEditorLauncher editorLauncher;
            if (!workflowEditorMapping.TryGetValue(workflowExpressionBuilder, out editorLauncher))
            {
                composite = true;
                editorLauncher = new WorkflowEditorLauncher(workflowExpressionBuilder);
                editorLauncher.VisualizerLayout = editorLayout;
                editorLauncher.Bounds = bounds;
                var addEditorMapping = CreateUpdateEditorMappingDelegate(editorMapping => editorMapping.Add(workflowExpressionBuilder, editorLauncher));
                var removeEditorMapping = CreateUpdateEditorMappingDelegate(editorMapping => editorMapping.Remove(workflowExpressionBuilder));
                commandExecutor.BeginCompositeCommand();
                commandExecutor.Execute(addEditorMapping, removeEditorMapping);
            }

            if (!editorLauncher.Visible || activate)
            {
                var launcher = Launcher;
                var highlight = node.Highlight;
                var visible = editorLauncher.Visible;
                var editorService = this.editorService;
                var serviceProvider = this.serviceProvider;
                var windowSelector = CreateWindowOwnerSelectorDelegate();
                Action launchEditor = () =>
                {
                    editorLauncher.Show(windowSelector(), serviceProvider);
                    if (highlight)
                    {
                        editorService.RefreshEditor();
                    }
                };

                if (visible) launchEditor();
                else commandExecutor.Execute(launchEditor, editorLauncher.Hide);
                if (composite) commandExecutor.EndCompositeCommand();
            }
        }

        public VisualizerDialogLauncher GetVisualizerDialogLauncher(GraphNode node)
        {
            VisualizerDialogLauncher visualizerDialog = null;
            if (visualizerMapping != null && node != null)
            {
                var expressionBuilder = node.Value as InspectBuilder;
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
                workflowEditorMapping.TryGetValue(workflowExpressionBuilder, out editorLauncher);
                return editorLauncher;
            }

            return null;
        }

        private VisualizerDialogSettings GetLayoutSettings(object key)
        {
            return visualizerLayout != null
                ? visualizerLayout.DialogSettings.FirstOrDefault(xs => xs.Tag == key || xs.Tag == null)
                : null;
        }

        public VisualizerDialogSettings CreateLayoutSettings(ExpressionBuilder builder)
        {
            VisualizerDialogSettings dialogSettings;
            WorkflowEditorLauncher editorLauncher;
            var workflowExpressionBuilder = ExpressionBuilder.GetWorkflowElement(builder) as WorkflowExpressionBuilder;
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
            dialogSettings.Tag = builder;
            return dialogSettings;
        }

        public void UpdateVisualizerLayout()
        {
            if (visualizerLayout == null)
            {
                visualizerLayout = new VisualizerLayout();
            }

            if (visualizerMapping == null)
            {
                var updatedLayout = new VisualizerLayout();
                foreach (var node in workflow)
                {
                    var layoutSettings = GetLayoutSettings(node.Value);
                    if (layoutSettings == null) layoutSettings = CreateLayoutSettings(node.Value);
                    else layoutSettings.Tag = node.Value;

                    var graphNode = graphView.Nodes.SelectMany(layer => layer).First(n => n.Value == node.Value);
                    var workflowEditorSettings = layoutSettings as WorkflowEditorSettings;
                    if (workflowEditorSettings != null)
                    {
                        var editorLauncher = GetWorkflowEditorLauncher(graphNode);
                        if (workflowEditorSettings.EditorDialogSettings.Visible)
                        {
                            LaunchWorkflowView(graphNode,
                                               workflowEditorSettings.EditorVisualizerLayout,
                                               workflowEditorSettings.EditorDialogSettings.Bounds,
                                               activate: false);
                        }
                    }

                    updatedLayout.DialogSettings.Add(layoutSettings);
                }

                visualizerLayout = updatedLayout;
            }
            else
            {
                visualizerLayout.DialogSettings.Clear();

                foreach (var mapping in visualizerMapping)
                {
                    var visualizerDialog = mapping.Value;
                    var visible = visualizerDialog.Visible;
                    visualizerDialog.Hide();

                    var visualizer = visualizerDialog.Visualizer;
                    var dialogSettings = CreateLayoutSettings(mapping.Key);
                    var mashupVisualizer = visualizer.IsValueCreated ? visualizer.Value as DialogMashupVisualizer : null;
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

                    if (visualizer.IsValueCreated)
                    {
                        var visualizerType = visualizer.Value.GetType();
                        var visualizerSettings = new XDocument();
                        var serializer = new XmlSerializer(visualizerType);
                        using (var writer = visualizerSettings.CreateWriter())
                        {
                            serializer.Serialize(writer, visualizer.Value);
                        }
                        var root = visualizerSettings.Root;
                        root.Remove();
                        var xsdAttribute = root.Attribute(XsdAttributeName);
                        if (xsdAttribute != null) xsdAttribute.Remove();
                        var xsiAttribute = root.Attribute(XsiAttributeName);
                        if (xsiAttribute != null) xsiAttribute.Remove();
                        dialogSettings.VisualizerTypeName = visualizerType.FullName;
                        dialogSettings.VisualizerSettings = root;
                    }

                    visualizerLayout.DialogSettings.Add(dialogSettings);
                }
            }

            visualizerMapping = null;
        }

        private void UpdateGraphLayout()
        {
            UpdateGraphLayout(validateWorkflow: true);
        }

        private bool UpdateGraphLayout(bool validateWorkflow)
        {
            var result = true;
            graphView.Nodes = workflow
                .LongestPathLayering()
                .EnsureLayerPriority()
                .SortLayeringByConnectionKey()
                .ToList();
            graphView.Invalidate();
            if (validateWorkflow)
            {
                result = editorService.ValidateWorkflow();
            }

            UpdateVisualizerLayout();
            return result;
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
            if (editorService.WorkflowRunning) return;
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
                var node = GetGraphNodeTag(workflow, graphViewNode, false);
                if (node != null && workflow.Contains(node))
                {
                    dragSelection = graphView.SelectedNodes;
                    dragHighlight = graphViewNode;
                }
            }
        }

        private void graphView_DragOver(object sender, DragEventArgs e)
        {
            if (editorService.WorkflowRunning) return;
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
            if (e.KeyCode == Keys.F5)
            {
                if (e.Shift)
                {
                    if (e.Control) editorService.RestartWorkflow();
                    else editorService.StopWorkflow();
                }
                else editorService.StartWorkflow();
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

            if (!editorService.WorkflowRunning)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    DeleteGraphNodes(selectionModel.SelectedNodes);
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

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            var selectedNodes = selectionModel.SelectedNodes.ToArray();
            if (selectedNodes.Length > 0) copyToolStripMenuItem.Enabled = true;
            if (!editorService.WorkflowRunning)
            {
                pasteToolStripMenuItem.Enabled = true;
                if (selectedNodes.Length > 0)
                {
                    cutToolStripMenuItem.Enabled = true;
                    deleteToolStripMenuItem.Enabled = true;
                    groupToolStripMenuItem.Enabled = true;
                }
            }

            if (selectedNodes.Length == 1)
            {
                var selectedNode = selectedNodes[0];
                var layoutSettings = GetLayoutSettings(selectedNode.Value);
                if (layoutSettings != null)
                {
                    var visualizerTypes = GetTypeVisualizers(selectedNode);
                    visualizerToolStripMenuItem.Enabled = true;
                    foreach (var type in visualizerTypes)
                    {
                        var typeName = type.FullName;
                        ToolStripMenuItem menuItem = null;
                        menuItem = new ToolStripMenuItem(typeName, null, delegate
                        {
                            layoutSettings.VisualizerTypeName = typeName;
                            layoutSettings.VisualizerSettings = null;
                            if (!menuItem.Checked)
                            {
                                if (!editorService.WorkflowRunning)
                                {
                                    layoutSettings.Size = Size.Empty;
                                }
                                else
                                {
                                    var inspectBuilder = (InspectBuilder)selectedNode.Value;
                                    var visualizerLauncher = visualizerMapping[inspectBuilder];
                                    var visualizerVisible = visualizerLauncher.Visible;
                                    var visualizerBounds = visualizerLauncher.Bounds;
                                    if (visualizerVisible)
                                    {
                                        visualizerLauncher.Hide();
                                    }

                                    visualizerLauncher = CreateVisualizerLauncher(inspectBuilder, selectedNode);
                                    visualizerLauncher.Bounds = new Rectangle(visualizerBounds.Location, Size.Empty);
                                    visualizerMapping[inspectBuilder] = visualizerLauncher;
                                    if (visualizerVisible)
                                    {
                                        visualizerLauncher.Show(graphView, serviceProvider);
                                    }
                                }
                            }
                        });
                        var index = visualizerToolStripMenuItem.DropDownItems.Add(menuItem);
                        menuItem.Checked = string.IsNullOrEmpty(layoutSettings.VisualizerTypeName)
                            ? index == 0
                            : typeName == layoutSettings.VisualizerTypeName;
                    }
                }
            }
        }

        private void contextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            foreach (ToolStripItem item in contextMenuStrip.Items)
            {
                item.Enabled = false;
            }

            visualizerToolStripMenuItem.DropDownItems.Clear();
        }

        #endregion
    }

    public enum CreateGraphNodeType
    {
        Successor,
        Predecessor
    }
}
