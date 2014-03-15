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
using Bonsai.Editor.Properties;
using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom;
using System.Drawing.Design;
using Bonsai.Editor;
using System.Reactive.Disposables;
using Bonsai.Expressions.Properties;

namespace Bonsai.Design
{
    partial class WorkflowGraphView : UserControl
    {
        static readonly Cursor InvalidSelectionCursor = Cursors.No;
        static readonly Cursor AlternateSelectionCursor = Cursors.UpArrow;
        static readonly XName XsdAttributeName = ((XNamespace)"http://www.w3.org/2000/xmlns/") + "xsd";
        static readonly XName XsiAttributeName = ((XNamespace)"http://www.w3.org/2000/xmlns/") + "xsi";
        const string XsdAttributeValue = "http://www.w3.org/2001/XMLSchema";
        const string XsiAttributeValue = "http://www.w3.org/2001/XMLSchema-instance";
        const string OutputMenuItemLabel = "Output";

        const int RightMouseButton = 0x2;
        const int ShiftModifier = 0x4;
        const int CtrlModifier = 0x8;
        const int AltModifier = 0x20;
        public const Keys BranchModifier = Keys.Alt;
        public const Keys PredecessorModifier = Keys.Shift;
        public const string BonsaiExtension = ".bonsai";

        int dragKeyState;
        bool isContextMenuSource;
        GraphNode dragHighlight;
        IEnumerable<GraphNode> dragSelection;
        CommandExecutor commandExecutor;
        ExpressionBuilderGraph workflow;
        WorkflowSelectionModel selectionModel;
        IWorkflowEditorState editorState;
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
            editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
            builderConverter = new ExpressionBuilderTypeConverter();
            workflowEditorMapping = new Dictionary<WorkflowExpressionBuilder, WorkflowEditorLauncher>();

            graphView.HandleDestroyed += graphView_HandleDestroyed;
            editorState.WorkflowStarted += editorService_WorkflowStarted;
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
            if (editorState.WorkflowRunning)
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
            if (observableType != null)
            {
                foreach (var type in editorService.GetTypeVisualizers(observableType))
                {
                    yield return type;
                }
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
                    visualizerLauncher.WindowState = layoutSettings.WindowState;
                    if (layoutSettings.Visible)
                    {
                        visualizerLauncher.Show(graphView, serviceProvider);
                    }
                }
            }
        }

        private static ExpressionBuilder GetGraphNodeBuilder(GraphNode node)
        {
            if (node != null && node.Value != null)
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
            if (!branch &&
                (elementType == ElementCategory.Source ||
                elementType == ElementCategory.Property))
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
                    if (branch) parameter.Index = predecessors.Count;
                    else if (predecessors.Count > 0)
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

        bool CanConnect(IEnumerable<GraphNode> graphViewSources, GraphNode graphViewTarget)
        {
            var target = GetGraphNodeTag(workflow, graphViewTarget, false);
            var connectionCount = workflow.Predecessors(target).Count();
            foreach (var sourceNode in graphViewSources)
            {
                var node = GetGraphNodeTag(workflow, sourceNode, false);
                if (target == node || node.Successors.Any(edge => edge.Target == target))
                {
                    return false;
                }

                var builder = GetGraphNodeBuilder(graphViewTarget);
                if (connectionCount++ >= target.Value.ArgumentRange.UpperBound &&
                    !(builder is IPropertyMappingBuilder) ||
                    target.DepthFirstSearch().Contains(node))
                {
                    return false;
                }
            }

            return true;
        }

        bool CanDisconnect(IEnumerable<GraphNode> graphViewSources, GraphNode graphViewTarget)
        {
            var target = GetGraphNodeTag(workflow, graphViewTarget, false);
            foreach (var sourceNode in graphViewSources)
            {
                var node = GetGraphNodeTag(workflow, sourceNode, false);
                if (node == null) return false;

                if (!node.Successors.Any(edge => edge.Target == target))
                {
                    return false;
                }
            }

            return true;
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

        public void DisconnectGraphNodes(IEnumerable<GraphNode> graphViewSources, GraphNode graphViewTarget)
        {
            var workflow = this.workflow;
            Action addConnection = () => { };
            Action removeConnection = () => { };
            var target = GetGraphNodeTag(workflow, graphViewTarget);
            var predecessorEdges = workflow.PredecessorEdges(target).ToArray();
            foreach (var graphViewSource in graphViewSources)
            {
                var source = GetGraphNodeTag(workflow, graphViewSource);
                var predecessor = predecessorEdges.Where(xs => xs.Item1 == source).FirstOrDefault();
                if (predecessor == null) continue;
                var edge = predecessor.Item2;
                var edgeIndex = edge.Label.Index;
                var siblingEdgesAfter = (from siblingEdge in predecessorEdges
                                         where siblingEdge.Item2.Label.Index.CompareTo(edgeIndex) > 0
                                         select siblingEdge.Item2)
                                         .ToArray();

                addConnection += () =>
                {
                    predecessor.Item1.Successors.Insert(predecessor.Item3, edge);
                    foreach (var sibling in siblingEdgesAfter)
                    {
                        sibling.Label.Index++;
                    }
                };

                removeConnection += () =>
                {
                    predecessor.Item1.Successors.RemoveAt(predecessor.Item3);
                    foreach (var sibling in siblingEdgesAfter)
                    {
                        sibling.Label.Index--;
                    }
                };
            }

            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            commandExecutor.Execute(
            () =>
            {
                removeConnection();
                updateGraphLayout();
            },
            () =>
            {
                addConnection();
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

        WorkflowExpressionBuilder CreateWorkflowBuilder(string typeName, ExpressionBuilderGraph graph)
        {
            var type = Type.GetType(typeName);
            if (!typeof(WorkflowExpressionBuilder).IsAssignableFrom(type))
            {
                throw new ArgumentException("The specified type is not a workflow expression builder.", "typeName");
            }

            return (WorkflowExpressionBuilder)Activator.CreateInstance(type, graph);
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

        public void CreateGraphNode(ExpressionBuilder builder, ElementCategory elementCategory, GraphNode closestGraphViewNode, CreateGraphNodeType nodeType, bool branch, bool validate = true)
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
                var validation = validate && updateGraphLayoutValidation();
                if (validation)
                {
                    updateSelectedNode();
                }
            },
            () =>
            {
                removeConnection();
                removeNode();
                if (validate)
                {
                    updateGraphLayout();
                    restoreSelectedNode();
                }
            });
        }

        public void InsertGraphElements(ExpressionBuilderGraph elements, CreateGraphNodeType nodeType, bool branch)
        {
            if (elements == null)
            {
                throw new ArgumentNullException("elements");
            }

            var selection = selectionModel.SelectedNodes.FirstOrDefault();
            Action addConnection = () => { };
            Action removeConnection = () => { };
            if (selection != null)
            {
                var selectionNode = GetGraphNodeTag(workflow, selection);
                var source = elements.Sources().FirstOrDefault();
                var sink = elements.Sinks().FirstOrDefault();
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
                foreach (var node in elements)
                {
                    workflow.Add(node);
                }
                addConnection();
                updateGraphLayout();
            },
            () =>
            {
                removeConnection();
                foreach (var node in elements.TopologicalSort())
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

            var simplePredecessor = predecessorEdges.Length == 1;
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
                                     .Reverse()
                                     .ToArray();

                addEdge = () =>
                {
                    Array.ForEach(replacedEdges, replacedEdge =>
                    {
                        if (simplePredecessor) workflow.InsertEdge(replacedEdge.predecessor, replacedEdge.edgeIndex, replacedEdge.edge);
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
            GroupGraphNodes(nodes, graph => new NestedWorkflowBuilder(graph));
        }

        private void GroupGraphNodes(IEnumerable<GraphNode> nodes, string typeName)
        {
            GroupGraphNodes(nodes, graph => CreateWorkflowBuilder(typeName, graph));
        }

        private void GroupGraphNodes(IEnumerable<GraphNode> nodes, Func<ExpressionBuilderGraph, WorkflowExpressionBuilder> groupFactory)
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
            var workflowBuilder = nodes.ToWorkflowBuilder(recurse: false);
            var sources = workflowBuilder.Workflow.Sources().ToArray();
            var sinks = workflowBuilder.Workflow.Sinks().ToArray();

            var source = sources.First();
            var layeredNodes = graphView.Nodes.LayeredNodes();
            var sourceNode = layeredNodes.Single(node => GetGraphNodeBuilder(node) == source.Value);
            var predecessors = layeredNodes
                .Where(node => node.Successors.Any(edge => edge.Node.Value == sourceNode.Value))
                .ToArray();
            if (predecessors.Length == 1)
            {
                var workflowInput = new WorkflowInputBuilder();
                var inputNode = workflowBuilder.Workflow.Add(workflowInput);
                workflowBuilder.Workflow.AddEdge(inputNode, source, new ExpressionBuilderArgument());
                linkNode = predecessors[0];
                replacementNode = sourceNode;
            }

            var sink = sinks.First();
            var sinkNode = layeredNodes.Single(node => GetGraphNodeBuilder(node) == sink.Value);
            var successors = sinkNode.Successors.Select(edge => edge.Node).ToArray();
            if (sinks.Length == 1)
            {
                var workflowOutput = new WorkflowOutputBuilder();
                var outputNode = workflowBuilder.Workflow.Add(workflowOutput);
                workflowBuilder.Workflow.AddEdge(sink, outputNode, new ExpressionBuilderArgument());
                if (linkNode == null && successors.Length > 0)
                {
                    linkNode = successors[0];
                    replacementNode = sinkNode;
                    nodeType = CreateGraphNodeType.Predecessor;
                }
            }

            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            var workflowExpressionBuilder = groupFactory(workflowBuilder.Workflow.ToInspectableGraph(recurse: false));
            var updateSelectedNode = CreateUpdateGraphViewDelegate(localGraphView =>
            {
                localGraphView.SelectedNode = localGraphView.Nodes.LayeredNodes().First(n => GetGraphNodeBuilder(n) == workflowExpressionBuilder);
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
                            branch: false,
                            validate: false);
            if (replacementNode != null) DeleteGraphNode(replacementNode);
            commandExecutor.Execute(() =>
            {
                updateGraphLayout();
                updateSelectedNode();
            },
            () => { });
            commandExecutor.EndCompositeCommand();
        }

        private void InsertWorkflow(ExpressionBuilderGraph workflow)
        {
            if (workflow.Count > 0)
            {
                var branch = Control.ModifierKeys.HasFlag(BranchModifier);
                var predecessor = Control.ModifierKeys.HasFlag(PredecessorModifier) ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
                InsertGraphElements(workflow, predecessor, branch);
            }
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
            InsertWorkflow(builder.Workflow.ToInspectableGraph());
        }

        public GraphNode FindGraphNode(object value)
        {
            return graphView.Nodes.SelectMany(layer => layer).FirstOrDefault(n => n.Value == value);
        }

        public void LaunchDefaultEditor(GraphNode node)
        {
            var builder = GetGraphNodeBuilder(node);
            var workflowExpressionBuilder = builder as WorkflowExpressionBuilder;
            if (workflowExpressionBuilder != null) LaunchWorkflowView(node);
            else if (builder != null)
            {
                var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
                var componentEditor = (ComponentEditor)TypeDescriptor.GetEditor(workflowElement, typeof(ComponentEditor));
                if (componentEditor != null)
                {
                    uiService.ShowComponentEditor(workflowElement, this);
                }
                else
                {
                    var defaultProperty = TypeDescriptor.GetDefaultProperty(workflowElement);
                    if (defaultProperty != null)
                    {
                        var editor = (UITypeEditor)defaultProperty.GetEditor(typeof(UITypeEditor));
                        if (editor != null && editor.GetEditStyle() == UITypeEditorEditStyle.Modal)
                        {
                            var graphViewEditorService = new WorkflowGraphViewEditorService(this, serviceProvider);
                            var context = new TypeDescriptorContext(workflowElement, defaultProperty, graphViewEditorService);
                            var currentValue = defaultProperty.GetValue(workflowElement);
                            var value = editor.EditValue(context, graphViewEditorService, currentValue);
                            if (value != currentValue && !defaultProperty.IsReadOnly)
                            {
                                defaultProperty.SetValue(workflowElement, value);
                            }

                            if (!editorState.WorkflowRunning)
                            {
                                editorService.ValidateWorkflow();
                            }
                        }
                    }
                }
            }
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
                    if (highlight && !visible)
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
                    dialogSettings.WindowState = visualizerDialog.WindowState;

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
                .EnsureEdgeLabelPriority()
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
            isContextMenuSource = false;
        }

        private void SetDragCursor(DragDropEffects effect)
        {
            switch (effect)
            {
                case DragDropEffects.All:
                case DragDropEffects.Copy:
                case DragDropEffects.Link:
                case DragDropEffects.Move:
                case DragDropEffects.Scroll:
                    if (isContextMenuSource) Cursor = AlternateSelectionCursor;
                    else Cursor.Current = AlternateSelectionCursor;
                    break;
                case DragDropEffects.None:
                default:
                    if (isContextMenuSource) Cursor = InvalidSelectionCursor;
                    else Cursor.Current = InvalidSelectionCursor;
                    break;
            }
        }

        private void graphView_DragEnter(object sender, DragEventArgs e)
        {
            if (editorState.WorkflowRunning) return;
            dragKeyState = e.KeyState;
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var path = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                if (path != null && path.Length == 1 &&
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
            if (editorState.WorkflowRunning) return;
            if (e.Effect != DragDropEffects.None && e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                OnDragFileDrop(e);
            }

            if (dragSelection != null)
            {
                var dragLocation = graphView.PointToClient(new Point(e.X, e.Y));
                var highlight = graphView.GetNodeAt(dragLocation);
                if (highlight != dragHighlight || e.KeyState != dragKeyState)
                {
                    if (highlight != null)
                    {
                        var validation = (e.KeyState & ShiftModifier) != 0
                            ? CanDisconnect(dragSelection, highlight)
                            : CanConnect(dragSelection, highlight);
                        e.Effect = validation ? DragDropEffects.Link : DragDropEffects.None;
                    }
                    else e.Effect = DragDropEffects.None;
                    SetDragCursor(e.Effect);
                    dragHighlight = highlight;
                }
            }

            dragKeyState = e.KeyState;
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
                    var nodeType = (e.KeyState & ShiftModifier) != 0 ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
                    var linkNode = graphView.GetNodeAt(dropLocation) ?? graphView.SelectedNode;
                    if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                    {
                        var path = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                        if (path.Length == 1)
                        {
                            var workflowBuilder = editorService.LoadWorkflow(path[0]);
                            InsertWorkflow(workflowBuilder.Workflow);
                        }
                    }
                    else
                    {
                        var typeNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
                        CreateGraphNode(typeNode, linkNode, nodeType, branch);
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
                            if ((e.KeyState & ShiftModifier) != 0) DisconnectGraphNodes(dragSelection, linkNode);
                            else ConnectGraphNodes(dragSelection, linkNode);
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

        private void graphView_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (dragSelection != null)
            {
                e.UseDefaultCursors = false;
                SetDragCursor(e.Effect);
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
                    LaunchDefaultEditor(graphView.SelectedNode);
                }
                else if (editorState.WorkflowRunning)
                {
                    LaunchVisualizer(graphView.SelectedNode);
                }
                else if (selectionModel.SelectedNodes.Any() && graphView.CursorNode != null)
                {
                    if (e.Modifiers == Keys.Shift && CanDisconnect(selectionModel.SelectedNodes, graphView.CursorNode))
                    {
                        DisconnectGraphNodes(selectionModel.SelectedNodes, graphView.CursorNode);
                    }
                    else if (CanConnect(selectionModel.SelectedNodes, graphView.CursorNode))
                    {
                        ConnectGraphNodes(selectionModel.SelectedNodes, graphView.CursorNode);
                    }
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

            if (!editorState.WorkflowRunning)
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

            editorService.OnKeyDown(e);
        }

        private void graphView_KeyPress(object sender, KeyPressEventArgs e)
        {
            editorService.OnKeyPress(e);
        }

        private void graphView_SelectedNodeChanged(object sender, EventArgs e)
        {
            selectionModel.UpdateSelection(this);
        }

        private void graphView_NodeMouseDoubleClick(object sender, GraphNodeMouseEventArgs e)
        {
            if (!editorState.WorkflowRunning || Control.ModifierKeys == Keys.Control)
            {
                LaunchDefaultEditor(e.Node);
            }
            else
            {
                LaunchVisualizer(e.Node);
            }
        }

        private void graphView_MouseDown(object sender, MouseEventArgs e)
        {
            if (dragSelection != null)
            {
                if (e.Button == MouseButtons.Left)
                {
                    var linkNode = graphView.GetNodeAt(e.Location);
                    if (linkNode != null)
                    {
                        var disconnect = (dragKeyState & ShiftModifier) != 0;
                        if (disconnect && CanDisconnect(dragSelection, linkNode))
                        {
                            DisconnectGraphNodes(dragSelection, linkNode);
                        }
                        else if (!disconnect && CanConnect(dragSelection, linkNode))
                        {
                            ConnectGraphNodes(dragSelection, linkNode);
                        }
                    }
                }

                ClearDragDrop();
                Cursor = Cursors.Default;
            }
        }

        private void graphView_NodeMouseEnter(object sender, GraphNodeMouseEventArgs e)
        {
            if (dragSelection != null && e.Node != null)
            {
                var disconnect = (dragKeyState & ShiftModifier) != 0;
                if (disconnect && !CanDisconnect(dragSelection, e.Node)) SetDragCursor(DragDropEffects.None);
                else if (!disconnect && !CanConnect(dragSelection, e.Node)) SetDragCursor(DragDropEffects.None);
            }
        }

        private void graphView_NodeMouseLeave(object sender, GraphNodeMouseEventArgs e)
        {
            if (dragSelection != null)
            {
                SetDragCursor(DragDropEffects.Link);
            }
        }

        private void graphView_HandleDestroyed(object sender, EventArgs e)
        {
            editorState.WorkflowStarted -= editorService_WorkflowStarted;
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
            contextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteGraphNodes(selectionModel.SelectedNodes);
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitializeConnectionSource();
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitializeConnectionSource();
            dragKeyState = ShiftModifier;
        }

        private void InitializeConnectionSource()
        {
            isContextMenuSource = true;
            dragSelection = graphView.SelectedNodes.ToArray();
            SetDragCursor(DragDropEffects.Link);
        }

        private void InitializeOutputMenuItem(ToolStripMenuItem menuItem, string memberSelector, Type memberType)
        {
            string typeName;
            using (var provider = new CSharpCodeProvider())
            {
                var typeRef = new CodeTypeReference(memberType);
                typeName = provider.GetTypeOutput(typeRef);
            }

            menuItem.Text += string.Format(" ({0})", typeName);
            menuItem.Name = memberSelector;
            menuItem.Tag = memberType;
        }

        private IDisposable CreateOutputMenuItems(Type type, ToolStripMenuItem ownerItem, GraphNode selectedNode)
        {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var memberSelector = string.Join(ExpressionHelper.MemberSeparator, ownerItem.Name, field.Name);
                var menuItem = CreateOutputMenuItem(field.Name, memberSelector, field.FieldType, selectedNode);
                ownerItem.DropDownItems.Add(menuItem);
            }

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var memberSelector = string.Join(ExpressionHelper.MemberSeparator, ownerItem.Name, property.Name);
                var menuItem = CreateOutputMenuItem(property.Name, memberSelector, property.PropertyType, selectedNode);
                ownerItem.DropDownItems.Add(menuItem);
            }

            var menuItemDisposable = new CompositeDisposable();
            EventHandler dropDownHandler = delegate
            {
                foreach (ToolStripMenuItem item in ownerItem.DropDownItems)
                {
                    var itemDisposable = CreateOutputMenuItems((Type)item.Tag, item, selectedNode);
                    menuItemDisposable.Add(itemDisposable);
                }
            };

            ownerItem.DropDownOpening += dropDownHandler;
            menuItemDisposable.Add(Disposable.Create(() => ownerItem.DropDownOpening -= dropDownHandler));
            return menuItemDisposable;
        }

        private ToolStripMenuItem CreateOutputMenuItem(string memberName, string memberSelector, Type memberType, GraphNode selectedNode)
        {
            var menuItem = new ToolStripMenuItem(memberName, null, delegate
            {
                var builder = new MemberSelectorBuilder { Selector = memberSelector };
                var successor = selectedNode.Successors.Select(edge => GetGraphNodeBuilder(edge.Node)).FirstOrDefault();
                var branch = successor != null && successor.GetType() == typeof(MemberSelectorBuilder);
                CreateGraphNode(builder, ElementCategory.Transform, selectedNode, CreateGraphNodeType.Successor, branch);
                contextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
            });

            InitializeOutputMenuItem(menuItem, memberSelector, memberType);
            return menuItem;
        }

        private void CreateExternalizeMenuItems(object workflowElement, ToolStripMenuItem ownerItem, GraphNode selectedNode)
        {
            var propertyMappingBuilder = GetGraphNodeBuilder(selectedNode) as IPropertyMappingBuilder;
            if (propertyMappingBuilder == null) return;

            var elementType = workflowElement.GetType();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(elementType))
            {
                if (property.IsReadOnly || !property.IsBrowsable) continue;
                var memberSelector = string.Join(ExpressionHelper.MemberSeparator, ownerItem.Name, property.Name);
                var memberValue = property.GetValue(workflowElement);
                var menuItem = CreateExternalizeMenuItem(
                    property.Name,
                    elementType,
                    property.Name,
                    property.PropertyType,
                    memberValue,
                    selectedNode,
                    propertyMappingBuilder);
                ownerItem.DropDownItems.Add(menuItem);
            }

            var workflowBuilder = workflowElement as WorkflowExpressionBuilder;
            if (workflowBuilder != null)
            {
                foreach (var property in from node in workflowBuilder.Workflow
                                         let workflowProperty = ExpressionBuilder.GetWorkflowElement(node.Value) as WorkflowProperty
                                         where workflowProperty != null
                                         let name = workflowProperty.Name
                                         where !string.IsNullOrEmpty(name)
                                         select workflowProperty)
                {
                    ToolStripMenuItem menuItem;
                    var propertyType = property.GetType();
                    var valueProperty = propertyType.GetProperty("Value");
                    var propertyValue = valueProperty.GetValue(property);
                    var externalizedProperty = property as IExternalizedProperty;
                    if (externalizedProperty != null)
                    {
                        menuItem = CreateExternalizeMenuItem(
                            property.Name,
                            externalizedProperty.ElementType,
                            externalizedProperty.MemberName,
                            valueProperty.PropertyType,
                            propertyValue,
                            selectedNode,
                            propertyMappingBuilder);
                    }
                    else
                    {
                        menuItem = CreateExternalizeMenuItem(
                            property.Name,
                            propertyType,
                            valueProperty.Name,
                            valueProperty.PropertyType,
                            propertyValue,
                            selectedNode,
                            propertyMappingBuilder);
                    }
                    ownerItem.DropDownItems.Add(menuItem);
                }
            }
        }

        private ToolStripMenuItem CreateExternalizeMenuItem(
            string name,
            Type elementType,
            string memberName,
            Type memberType,
            object memberValue,
            GraphNode selectedNode,
            IPropertyMappingBuilder propertyMappingBuilder)
        {
            var menuItem = new ToolStripMenuItem(name, null, delegate
            {
                var propertyType = typeof(ExternalizedProperty<,>).MakeGenericType(memberType, elementType);
                var property = (WorkflowProperty)Activator.CreateInstance(propertyType);
                var memberNameProperty = propertyType.GetProperty("MemberName");
                var valueProperty = propertyType.GetProperty("Value");
                memberNameProperty.SetValue(property, memberName);
                valueProperty.SetValue(property, memberValue);
                property.Name = name;

                var closestNode = GetGraphNodeTag(workflow, selectedNode);
                var predecessors = workflow.PredecessorEdges(closestNode).ToList();
                var edgeLabel = new ExpressionBuilderArgument(predecessors.Count);
                var builder = ExpressionBuilder.FromWorkflowElement(property, ElementCategory.Property);
                var selectedBuilderMappings = propertyMappingBuilder.PropertyMappings;
                var propertyMapping = new PropertyMapping(name, edgeLabel.Name);

                commandExecutor.BeginCompositeCommand();
                CreateGraphNode(builder, ElementCategory.Property, selectedNode, CreateGraphNodeType.Predecessor, true);
                commandExecutor.Execute(
                    () =>
                    {
                        selectedBuilderMappings.Remove(propertyMapping.Name);
                        selectedBuilderMappings.Add(propertyMapping);
                    },
                    () => selectedBuilderMappings.Remove(propertyMapping));
                commandExecutor.EndCompositeCommand();
                contextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
            });

            menuItem.Enabled = !propertyMappingBuilder.PropertyMappings.Contains(name);
            InitializeOutputMenuItem(menuItem, memberName, memberType);
            return menuItem;
        }

        private ToolStripMenuItem CreateVisualizerMenuItem(string typeName, VisualizerDialogSettings layoutSettings, GraphNode selectedNode)
        {
            ToolStripMenuItem menuItem = null;
            var emptyVisualizer = string.IsNullOrEmpty(typeName);
            var itemText = emptyVisualizer ? Resources.ContextMenu_NoneVisualizerItemLabel : typeName;
            menuItem = new ToolStripMenuItem(itemText, null, delegate
            {
                if (!menuItem.Checked)
                {
                    layoutSettings.VisualizerTypeName = typeName;
                    layoutSettings.VisualizerSettings = null;
                    layoutSettings.Visible = !emptyVisualizer;
                    if (!editorState.WorkflowRunning)
                    {
                        layoutSettings.Size = Size.Empty;
                    }
                    else
                    {
                        var inspectBuilder = (InspectBuilder)selectedNode.Value;
                        var visualizerLauncher = visualizerMapping[inspectBuilder];
                        var visualizerVisible = visualizerLauncher.Visible;
                        if (visualizerVisible)
                        {
                            visualizerLauncher.Hide();
                        }

                        var visualizerBounds = visualizerLauncher.Bounds;
                        visualizerLauncher = CreateVisualizerLauncher(inspectBuilder, selectedNode);
                        visualizerLauncher.Bounds = new Rectangle(visualizerBounds.Location, Size.Empty);
                        visualizerMapping[inspectBuilder] = visualizerLauncher;
                        if (layoutSettings.Visible)
                        {
                            visualizerLauncher.Show(graphView, serviceProvider);
                        }
                    }
                }
            });
            return menuItem;
        }

        private void CreateGroupMenuItems(GraphNode[] selectedNodes)
        {
            var toolboxService = (IWorkflowToolboxService)serviceProvider.GetService(typeof(IWorkflowToolboxService));
            if (toolboxService != null)
            {
                var selectedNode = selectedNodes.Length == 1 ? selectedNodes[0] : null;
                var workflowBuilder = selectedNode != null ? GetGraphNodeBuilder(selectedNode) as WorkflowExpressionBuilder : null;
                foreach (var element in from element in toolboxService.GetToolboxElements()
                                        where element.ElementTypes.Length == 1 &&
                                              (element.ElementTypes.Contains(ElementCategory.Nested) || element.AssemblyQualifiedName == typeof(ConditionBuilder).AssemblyQualifiedName)
                                        select element)
                {
                    ToolStripMenuItem menuItem = null;
                    var name = string.Format("{0} ({1})", element.Name, toolboxService.GetPackageDisplayName(element.Namespace));
                    menuItem = new ToolStripMenuItem(name, null, (sender, e) =>
                    {
                        if (workflowBuilder != null)
                        {
                            if (menuItem.Checked) return;
                            var builder = CreateWorkflowBuilder(element.AssemblyQualifiedName, workflowBuilder.Workflow);
                            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
                            var updateSelectedNode = CreateUpdateGraphViewDelegate(localGraphView =>
                            {
                                localGraphView.SelectedNode = localGraphView.Nodes.LayeredNodes().First(n => GetGraphNodeBuilder(n) == builder);
                            });

                            builder.Name = workflowBuilder.Name;
                            commandExecutor.BeginCompositeCommand();
                            commandExecutor.Execute(() => { }, updateGraphLayout);
                            CreateGraphNode(builder, element.ElementTypes[0], selectedNode, CreateGraphNodeType.Successor, false);
                            DeleteGraphNode(selectedNode);
                            commandExecutor.Execute(() =>
                            {
                                updateGraphLayout();
                                updateSelectedNode();
                            },
                            () => { });
                            commandExecutor.EndCompositeCommand();
                        }
                        else GroupGraphNodes(selectedNodes, element.AssemblyQualifiedName);
                    });

                    if (workflowBuilder != null &&
                        workflowBuilder.GetType().AssemblyQualifiedName == element.AssemblyQualifiedName)
                    {
                        menuItem.Checked = true;
                    }

                    groupToolStripMenuItem.DropDownItems.Add(menuItem);
                }
            }
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            var selectedNodes = selectionModel.SelectedNodes.ToArray();
            if (selectedNodes.Length > 0) copyToolStripMenuItem.Enabled = true;
            if (!editorState.WorkflowRunning)
            {
                pasteToolStripMenuItem.Enabled = true;
                if (selectedNodes.Length > 0)
                {
                    cutToolStripMenuItem.Enabled = true;
                    deleteToolStripMenuItem.Enabled = true;
                    connectToolStripMenuItem.Enabled = true;
                    disconnectToolStripMenuItem.Enabled = true;
                    groupToolStripMenuItem.Enabled = true;
                    CreateGroupMenuItems(selectedNodes);
                }
            }

            if (selectedNodes.Length == 1)
            {
                var selectedNode = selectedNodes[0];
                var inspectBuilder = (InspectBuilder)selectedNode.Value;
                if (inspectBuilder != null && inspectBuilder.ObservableType != null)
                {
                    outputToolStripMenuItem.Enabled = !editorState.WorkflowRunning;
                    InitializeOutputMenuItem(outputToolStripMenuItem, ExpressionBuilderArgument.ArgumentNamePrefix, inspectBuilder.ObservableType);
                    if (outputToolStripMenuItem.Enabled)
                    {
                        outputToolStripMenuItem.Tag = CreateOutputMenuItems(inspectBuilder.ObservableType, outputToolStripMenuItem, selectedNode);
                    }
                }

                var workflowElement = ExpressionBuilder.GetWorkflowElement(GetGraphNodeBuilder(selectedNode));
                if (workflowElement != null)
                {
                    if (!editorState.WorkflowRunning)
                    {
                        CreateExternalizeMenuItems(workflowElement, externalizeToolStripMenuItem, selectedNode);
                    }

                    externalizeToolStripMenuItem.Enabled = externalizeToolStripMenuItem.DropDownItems.Count > 0;
                }

                var layoutSettings = GetLayoutSettings(selectedNode.Value);
                if (layoutSettings != null)
                {
                    var activeVisualizer = layoutSettings.VisualizerTypeName;
                    if (editorState.WorkflowRunning)
                    {
                        var visualizerLauncher = visualizerMapping[inspectBuilder];
                        var visualizer = visualizerLauncher.Visualizer;
                        if (visualizer.IsValueCreated)
                        {
                            activeVisualizer = visualizer.Value.GetType().FullName;
                        }
                    }

                    var visualizerTypes = Enumerable.Repeat<Type>(null, 1);
                    visualizerTypes = visualizerTypes.Concat(GetTypeVisualizers(selectedNode));
                    visualizerToolStripMenuItem.Enabled = true;
                    foreach (var type in visualizerTypes)
                    {
                        var typeName = type != null ? type.FullName : string.Empty;
                        var menuItem = CreateVisualizerMenuItem(typeName, layoutSettings, selectedNode);
                        visualizerToolStripMenuItem.DropDownItems.Add(menuItem);
                        menuItem.Checked = type == null
                            ? string.IsNullOrEmpty(activeVisualizer)
                            : typeName == activeVisualizer;
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

            var outputMenuItemDisposable = outputToolStripMenuItem.Tag as IDisposable;
            if (outputMenuItemDisposable != null)
            {
                outputMenuItemDisposable.Dispose();
                outputToolStripMenuItem.Tag = null;
            }

            outputToolStripMenuItem.Text = OutputMenuItemLabel;
            outputToolStripMenuItem.DropDownItems.Clear();
            externalizeToolStripMenuItem.DropDownItems.Clear();
            visualizerToolStripMenuItem.DropDownItems.Clear();
            groupToolStripMenuItem.DropDownItems.Clear();
        }

        #endregion
    }

    public enum CreateGraphNodeType
    {
        Successor,
        Predecessor
    }
}
