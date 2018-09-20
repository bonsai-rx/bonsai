﻿using System;
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
using Bonsai.Editor.Themes;

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
        public const Keys GroupModifier = Keys.Control;
        public const Keys BranchModifier = Keys.Alt;
        public const Keys PredecessorModifier = Keys.Shift;
        public const string BonsaiExtension = ".bonsai";

        int dragKeyState;
        bool editorLaunching;
        bool isContextMenuSource;
        readonly bool editorReadOnly;
        GraphNode dragHighlight;
        WorkflowEditorControl editorControl;
        IEnumerable<GraphNode> dragSelection;
        CommandExecutor commandExecutor;
        ExpressionBuilderGraph workflow;
        WorkflowSelectionModel selectionModel;
        IWorkflowEditorState editorState;
        IWorkflowEditorService editorService;
        Dictionary<InspectBuilder, VisualizerDialogLauncher> visualizerMapping;
        Dictionary<IWorkflowExpressionBuilder, WorkflowEditorLauncher> workflowEditorMapping;
        ExpressionBuilderTypeConverter builderConverter;
        VisualizerLayout visualizerLayout;
        IServiceProvider serviceProvider;
        IUIService uiService;
        ThemeRenderer themeRenderer;
        SizeF inverseScaleFactor;
        SizeF scaleFactor;

        public WorkflowGraphView(IServiceProvider provider, WorkflowEditorControl owner, bool readOnly)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            InitializeComponent();
            editorControl = owner;
            serviceProvider = provider;
            editorReadOnly = readOnly;
            uiService = (IUIService)provider.GetService(typeof(IUIService));
            themeRenderer = (ThemeRenderer)provider.GetService(typeof(ThemeRenderer));
            commandExecutor = (CommandExecutor)provider.GetService(typeof(CommandExecutor));
            graphView.IconRenderer = (SvgRendererFactory)provider.GetService(typeof(SvgRendererFactory));
            selectionModel = (WorkflowSelectionModel)provider.GetService(typeof(WorkflowSelectionModel));
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
            editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
            builderConverter = new ExpressionBuilderTypeConverter();
            workflowEditorMapping = new Dictionary<IWorkflowExpressionBuilder, WorkflowEditorLauncher>();

            graphView.HandleDestroyed += graphView_HandleDestroyed;
            editorState.WorkflowStarted += editorService_WorkflowStarted;
            themeRenderer.ThemeChanged += themeRenderer_ThemeChanged;
            InitializeTheme();
        }

        internal WorkflowEditorLauncher Launcher { get; set; }

        internal WorkflowEditorControl EditorControl
        {
            get { return editorControl; }
        }

        internal bool ReadOnly
        {
            get { return editorReadOnly; }
        }

        internal bool CanEdit
        {
            get { return ReadOnly || editorState.WorkflowRunning; }
        }

        public GraphView GraphView
        {
            get { return graphView; }
        }

        public VisualizerLayout VisualizerLayout
        {
            get { return visualizerLayout; }
            set { SetVisualizerLayout(value); }
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

        private static Node<ExpressionBuilder, ExpressionBuilderArgument> FindWorkflowValue(ExpressionBuilderGraph workflow, ExpressionBuilder value)
        {
            return workflow.Single(n => ExpressionBuilder.Unwrap(n.Value) == value);
        }

        private void AddWorkflowNode(ExpressionBuilderGraph workflow, Node<ExpressionBuilder, ExpressionBuilderArgument> node)
        {
            workflow.Add(node);
            var workflowInput = ExpressionBuilder.Unwrap(node.Value) as WorkflowInputBuilder;
            if (workflowInput != null)
            {
                foreach (var inputBuilder in workflow.Select(xs => ExpressionBuilder.Unwrap(xs.Value) as WorkflowInputBuilder)
                                                     .Where(xs => xs != null))
                {
                    if (inputBuilder != workflowInput && inputBuilder.Index >= workflowInput.Index)
                    {
                        inputBuilder.Index++;
                    }
                }

                var launcher = Launcher;
                if (launcher != null)
                {
                    launcher.ParentView.UpdateGraphLayout(false);
                }
            }
        }

        private void RemoveWorkflowNode(ExpressionBuilderGraph workflow, Node<ExpressionBuilder, ExpressionBuilderArgument> node)
        {
            workflow.Remove(node);
            var workflowInput = ExpressionBuilder.Unwrap(node.Value) as WorkflowInputBuilder;
            if (workflowInput != null)
            {
                foreach (var inputBuilder in workflow.Select(xs => ExpressionBuilder.Unwrap(xs.Value) as WorkflowInputBuilder)
                                                     .Where(xs => xs != null))
                {
                    if (inputBuilder.Index > workflowInput.Index)
                    {
                        inputBuilder.Index--;
                    }
                }

                var launcher = Launcher;
                if (launcher != null)
                {
                    launcher.ParentView.UpdateGraphLayout(false);
                }
            }
        }

        private Func<IWin32Window> CreateWindowOwnerSelectorDelegate()
        {
            var launcher = Launcher;
            return launcher != null ? (Func<IWin32Window>)(() => launcher.Owner) : () => graphView;
        }

        private Action CreateUpdateEditorMappingDelegate(Action<Dictionary<IWorkflowExpressionBuilder, WorkflowEditorLauncher>> action)
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

        private Action CreateUpdateSelectionDelegate()
        {
            return CreateUpdateSelectionDelegate(Enumerable.Empty<ExpressionBuilder>());
        }

        private Action CreateUpdateSelectionDelegate(GraphNode selection)
        {
            var selectedNodes = selection == null ? Enumerable.Empty<GraphNode>() : new[] { selection };
            return CreateUpdateSelectionDelegate(selectedNodes);
        }

        private Action CreateUpdateSelectionDelegate(IEnumerable<GraphNode> selection)
        {
            var nodes = selection.Select(node => GetGraphNodeTag(workflow, node));
            return CreateUpdateSelectionDelegate(nodes);
        }

        private Action CreateUpdateSelectionDelegate(Node<ExpressionBuilder, ExpressionBuilderArgument> selection)
        {
            var selectedNodes = selection == null ? Enumerable.Empty<Node<ExpressionBuilder, ExpressionBuilderArgument>>() : new[] { selection };
            return CreateUpdateSelectionDelegate(selectedNodes);
        }

        private Action CreateUpdateSelectionDelegate(IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> selection)
        {
            return CreateUpdateSelectionDelegate(selection.Select(node => node.Value));
        }

        private Action CreateUpdateSelectionDelegate(ExpressionBuilder selection)
        {
            return CreateUpdateSelectionDelegate(new[] { selection });
        }

        private Action CreateUpdateSelectionDelegate(IEnumerable<ExpressionBuilder> selection)
        {
            var builders = selection.Select(builder => ExpressionBuilder.Unwrap(builder)).ToArray();
            return CreateUpdateGraphViewDelegate(graphView =>
            {
                graphView.SelectedNodes = graphView.Nodes.LayeredNodes().Where(node => builders.Contains(GetGraphNodeBuilder(node)));
            });
        }

        private void HideWorkflowEditorLauncher(WorkflowEditorLauncher editorLauncher)
        {
            var visible = editorLauncher.Visible;
            var serviceProvider = this.serviceProvider;
            var windowSelector = CreateWindowOwnerSelectorDelegate();
            var activeTabClosing = editorLauncher.Container != null &&
                                   editorLauncher.Container.ActiveTab != null &&
                                   editorLauncher.Container.ActiveTab.WorkflowGraphView == editorLauncher.WorkflowGraphView;
            commandExecutor.Execute(
                editorLauncher.Hide,
                () =>
                {
                    if (visible && editorLauncher.Builder.Workflow != null)
                    {
                        editorLauncher.Show(windowSelector(), serviceProvider);
                        if (editorLauncher.Container != null && activeTabClosing)
                        {
                            editorLauncher.Container.SelectTab(editorLauncher.WorkflowGraphView);
                        }
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

        internal void HideEditorMapping()
        {
            foreach (var mapping in workflowEditorMapping)
            {
                mapping.Value.Hide();
            }
        }

        private void ClearEditorMapping()
        {
            HideEditorMapping();
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
            while (observableType != null)
            {
                foreach (var type in editorService.GetTypeVisualizers(observableType))
                {
                    yield return type;
                }

                if (!observableType.IsClass)
                {
                    foreach (var type in editorService.GetTypeVisualizers(typeof(object)))
                    {
                        yield return type;
                    }
                    break;
                }
                else observableType = observableType.BaseType;
            }
        }

        private VisualizerDialogLauncher CreateVisualizerLauncher(InspectBuilder inspectBuilder, GraphNode graphNode)
        {
            if (inspectBuilder.ObservableType == null || !inspectBuilder.PublishNotifications)
            {
                return null;
            }

            Type visualizerType = null;
            var deserializeVisualizer = false;
            Func<DialogTypeVisualizer> visualizerFactory = null;
            var layoutSettings = GetLayoutSettings(graphNode.Value);
            var visualizerTypes = GetTypeVisualizers(graphNode);
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

            visualizerType = visualizerType ?? visualizerTypes.FirstOrDefault();
            if (visualizerType == null)
            {
                return null;
            }

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

        private static Rectangle ScaleBounds(Rectangle bounds, SizeF scaleFactor)
        {
            bounds.Location = Point.Round(new PointF(bounds.X * scaleFactor.Width, bounds.Y * scaleFactor.Height));
            bounds.Size = Size.Round(new SizeF(bounds.Width * scaleFactor.Width, bounds.Height * scaleFactor.Height));
            return bounds;
        }

        private void InitializeVisualizerMapping()
        {
            if (workflow == null) return;
            visualizerMapping = (from node in workflow
                                 let key = (InspectBuilder)node.Value
                                 let graphNode = graphView.Nodes.SelectMany(layer => layer).First(n => n.Value == key)
                                 let visualizerLauncher = CreateVisualizerLauncher(key, graphNode) where visualizerLauncher != null
                                 select new { key, visualizerLauncher })
                                 .ToDictionary(mapping => mapping.key,
                                               mapping => mapping.visualizerLauncher);

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

                    visualizerLauncher.Bounds = ScaleBounds(layoutSettings.Bounds, scaleFactor);
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
            Node<ExpressionBuilder, ExpressionBuilderArgument> closestNode,
            CreateGraphNodeType nodeType,
            bool branch,
            bool validate = true)
        {
            var workflow = this.workflow;
            Action addConnection = () => { };
            Action removeConnection = () => { };
            if (closestNode != null)
            {
                var parameter = new ExpressionBuilderArgument();
                if (nodeType == CreateGraphNodeType.Predecessor)
                {
                    var predecessors = workflow.PredecessorEdges(closestNode).ToList();
                    // If the selected node has no predecessors or we are branching, we need to test if we can connect
                    if (!validate || (!branch && predecessors.Count > 0 || CanConnect(sinkNode, closestNode))
                        && (branch || CanConnect(predecessors.Select(p => p.Item1), sourceNode)))
                    {
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
                                removeConnection += () => { workflow.SetEdge(predecessorNode, edgeIndex, predecessorEdge); };
                            }
                        }

                        // After dealing with predecessors, we just create an edge to the selected node
                        var edge = Edge.Create(closestNode, parameter);
                        addConnection += () => { workflow.AddEdge(sinkNode, edge); };
                        removeConnection += () => { workflow.RemoveEdge(sinkNode, edge); };
                    }
                }
                else if (!validate || CanConnect(closestNode, sourceNode))
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
            var sources = graphViewSources.Select(sourceNode => GetGraphNodeTag(workflow, sourceNode, false));
            return CanConnect(sources, target);
        }

        bool CanConnect(
            Node<ExpressionBuilder, ExpressionBuilderArgument> source,
            Node<ExpressionBuilder, ExpressionBuilderArgument> target)
        {
            return CanConnect(Enumerable.Repeat(source, 1), target);
        }

        bool CanConnect(
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> sources,
            Node<ExpressionBuilder, ExpressionBuilderArgument> target)
        {
            var connectionCount = workflow.Contains(target)
                ? workflow.Predecessors(target).Count(node => !node.Value.IsBuildDependency())
                : 0;
            foreach (var source in sources)
            {
                if (source == null || target == source || source.Successors.Any(edge => edge.Target == target))
                {
                    return false;
                }

                if (connectionCount++ >= target.Value.ArgumentRange.UpperBound &&
                    !source.Value.IsBuildDependency() ||
                    target.DepthFirstSearch().Contains(source))
                {
                    return false;
                }
            }

            return true;
        }

        IEnumerable<PropertyMapping> GetEdgeMappings(Edge<ExpressionBuilder, ExpressionBuilderArgument> edge)
        {
            var mappingBuilder = ExpressionBuilder.Unwrap(edge.Target.Value) as IPropertyMappingBuilder;
            if (mappingBuilder != null)
            {
                foreach (var mapping in mappingBuilder.PropertyMappings)
                {
                    var memberPath = mapping.Selector.Split(new[] { ExpressionHelper.MemberSeparator }, 2, StringSplitOptions.None);
                    if (memberPath[0] == edge.Label.Name)
                    {
                        yield return mapping;
                    }
                }
            }
        }

        void AddPropertyMappings(ExpressionBuilder builder, IEnumerable<PropertyMapping> mappings)
        {
            var mappingBuilder = ExpressionBuilder.Unwrap(builder) as IPropertyMappingBuilder;
            if (mappingBuilder != null)
            {
                var propertyMappings = mappingBuilder.PropertyMappings;
                foreach (var mapping in mappings)
                {
                    if (propertyMappings.Contains(mapping.Name))
                    {
                        propertyMappings.Remove(mapping.Name);
                    }

                    propertyMappings.Add(mapping);
                }
            }
        }

        void RemovePropertyMappings(ExpressionBuilder builder, IEnumerable<PropertyMapping> mappings)
        {
            var mappingBuilder = ExpressionBuilder.Unwrap(builder) as IPropertyMappingBuilder;
            if (mappingBuilder != null)
            {
                foreach (var mapping in mappings)
                {
                    mappingBuilder.PropertyMappings.Remove(mapping);
                }
            }
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
            var restoreSelectedNodes = CreateUpdateSelectionDelegate(graphViewSources);
            var updateSelectedNode = CreateUpdateSelectionDelegate(graphViewTarget);
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
                updateSelectedNode();
            },
            () =>
            {
                removeConnection();
                updateGraphLayout();
                restoreSelectedNodes();
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
                var propertyMappings = GetEdgeMappings(edge).ToArray();
                var siblingEdgesAfter = (from siblingEdge in predecessorEdges
                                         where siblingEdge.Item2.Label.Index.CompareTo(edgeIndex) > 0
                                         select siblingEdge.Item2)
                                         .ToArray();

                addConnection += () =>
                {
                    predecessor.Item1.Successors.Insert(predecessor.Item3, edge);
                    AddPropertyMappings(edge.Target.Value, propertyMappings);
                    foreach (var sibling in siblingEdgesAfter)
                    {
                        sibling.Label.Index++;
                    }
                };

                removeConnection += () =>
                {
                    RemovePropertyMappings(edge.Target.Value, propertyMappings);
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

        public void InsertGraphNode(TreeNode typeNode, CreateGraphNodeType nodeType, bool branch, bool group)
        {
            InsertGraphNode(typeNode, nodeType, branch, group, null);
        }

        public void InsertGraphNode(TreeNode typeNode, CreateGraphNodeType nodeType, bool branch, bool group, string arguments)
        {
            if (typeNode == null)
            {
                throw new ArgumentNullException("typeNode");
            }

            var elementCategory = (ElementCategory)typeNode.Tag;
            if (elementCategory == ElementCategory.Workflow)
            {
                var includeBuilder = new IncludeWorkflowBuilder { Path = typeNode.Name };
                CreateGraphNode(includeBuilder, graphView.SelectedNodes.FirstOrDefault(), nodeType, branch);
            }
            else
            {
                var selectedNodes = graphView.SelectedNodes;
                var selectedNode = selectedNodes.FirstOrDefault();
                if (group && selectedNode != null) CreateOrReplaceGroupNode(selectedNodes.ToArray(), typeNode);
                else
                {
                    var builder = CreateBuilder(typeNode);
                    if (!string.IsNullOrEmpty(arguments))
                    {
                        //TODO: This special case for binary operator operands should be avoided in the future
                        var binaryOperator = builder as BinaryOperatorBuilder;
                        if (binaryOperator != null && selectedNode != null)
                        {
                            var inputBuilder = ((Node<ExpressionBuilder, ExpressionBuilderArgument>)selectedNode.Tag).Value as InspectBuilder;
                            if (inputBuilder != null && inputBuilder.ObservableType != null)
                            {
                                binaryOperator.Build(Expression.Parameter(typeof(IObservable<>).MakeGenericType(inputBuilder.ObservableType)));
                            }
                        }

                        var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
                        var defaultProperty = TypeDescriptor.GetDefaultProperty(workflowElement);
                        if (defaultProperty != null &&
                            !defaultProperty.IsReadOnly &&
                            defaultProperty.Converter != null &&
                            defaultProperty.Converter.CanConvertFrom(typeof(string)))
                        {
                            try
                            {
                                var context = new TypeDescriptorContext(workflowElement, defaultProperty, serviceProvider);
                                var propertyValue = defaultProperty.Converter.ConvertFromString(context, arguments);
                                defaultProperty.SetValue(workflowElement, propertyValue);
                            }
                            catch (Exception ex)
                            {
                                throw new SystemException(ex.Message, ex);
                            }
                        }
                    }
                    CreateGraphNode(builder, selectedNode, nodeType, branch);
                }
            }
        }

        public void CreateGraphNode(ExpressionBuilder builder, GraphNode selectedNode, CreateGraphNodeType nodeType, bool branch, bool validate = true)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            var workflow = this.workflow;
            var workflowInput = builder as WorkflowInputBuilder;
            if (workflowInput != null)
            {
                workflowInput.Index = workflow.Count(node => ExpressionBuilder.Unwrap(node.Value) is WorkflowInputBuilder);
            }

            var inspectBuilder = builder.AsInspectBuilder();
            var inspectNode = new Node<ExpressionBuilder, ExpressionBuilderArgument>(inspectBuilder);
            var inspectParameter = new ExpressionBuilderArgument();
            Action addNode = () => { AddWorkflowNode(workflow, inspectNode); };
            Action removeNode = () => { RemoveWorkflowNode(workflow, inspectNode); };
            builder = inspectBuilder.Builder;

            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            var updateSelectedNode = CreateUpdateSelectionDelegate(builder);

            var closestNode = selectedNode != null ? GetGraphNodeTag(workflow, selectedNode) : null;
            var restoreSelectedNode = CreateUpdateSelectionDelegate(closestNode);

            var workflowBuilder = builder as WorkflowExpressionBuilder;
            if (workflowBuilder != null && validate &&
               (workflowBuilder.ArgumentRange.UpperBound > 0 ||
                workflowBuilder is GroupWorkflowBuilder ||
                workflowBuilder is NestedWorkflowBuilder) &&
               (workflowBuilder.ArgumentRange.LowerBound > 0 ||
                closestNode != null && (nodeType == CreateGraphNodeType.Successor || workflow.PredecessorEdges(closestNode).Any())))
            {
                var nestedInput = new WorkflowInputBuilder();
                var nestedOutput = new WorkflowOutputBuilder();
                var nestedInputInspectBuilder = new InspectBuilder(nestedInput);
                var nestedOutputInspectBuilder = new InspectBuilder(nestedOutput);
                var nestedInputNode = workflowBuilder.Workflow.Add(nestedInputInspectBuilder);
                var nestedOutputNode = workflowBuilder.Workflow.Add(nestedOutputInspectBuilder);
                workflowBuilder.Workflow.AddEdge(nestedInputNode, nestedOutputNode, new ExpressionBuilderArgument());
            }

            var insertCommands = GetInsertGraphNodeCommands(inspectNode, inspectNode, closestNode, nodeType, branch, validate);
            var addConnection = insertCommands.Item1;
            var removeConnection = insertCommands.Item2;
            commandExecutor.Execute(
            () =>
            {
                addNode();
                addConnection();
                if (validate)
                {
                    updateGraphLayout();
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

            Action addConnection = () => { };
            Action removeConnection = () => { };
            var selectedNodes = selectionModel.SelectedNodes.ToArray();
            var updateSelectedNodes = CreateUpdateSelectionDelegate(elements.Sinks().FirstOrDefault());
            var restoreSelectedNodes = CreateUpdateSelectionDelegate(selectedNodes);
            if (selectedNodes.Length > 0)
            {
                var selectionNode = GetGraphNodeTag(workflow, selectedNodes[0]);
                var source = elements.Sources().FirstOrDefault();
                var sink = elements.Sinks().FirstOrDefault();
                if (source != null && sink != null)
                {
                    var insertCommands = GetInsertGraphNodeCommands(source, sink, selectionNode, nodeType, branch);
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
                    AddWorkflowNode(workflow, node);
                }
                addConnection();
                updateGraphLayout();
                updateSelectedNodes();
            },
            () =>
            {
                removeConnection();
                foreach (var node in elements.TopologicalSort())
                {
                    RemoveWorkflowNode(workflow, node);
                }
                updateGraphLayout();
                restoreSelectedNodes();
            });
        }

        void DeleteGraphNode(GraphNode node)
        {
            DeleteGraphNode(node, true);
        }

        void DeleteGraphNode(GraphNode node, bool replaceEdges)
        {
            var workflow = this.workflow;
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            Action addEdge = () => { };
            Action removeEdge = () => { };

            var workflowNode = GetGraphNodeTag(workflow, node);
            var edgeMappings = workflowNode.Successors.Select(edge => Tuple.Create(edge.Target.Value, GetEdgeMappings(edge).ToArray())).ToList();
            var predecessorEdges = workflow.PredecessorEdges(workflowNode).ToArray();
            var siblingEdgesAfter = (from edge in workflowNode.Successors
                                     from siblingEdge in workflow.PredecessorEdges(edge.Target)
                                     where siblingEdge.Item2.Label.Index.CompareTo(edge.Label.Index) > 0
                                     select siblingEdge.Item2)
                                     .ToArray();

            var simplePredecessor = predecessorEdges.Length == 1;
            var simpleSuccessor = (workflowNode.Successors.Count == 1 && workflow.Predecessors(workflowNode.Successors[0].Target).Count() == 1);
            replaceEdges &= simplePredecessor || simpleSuccessor;
            if (replaceEdges)
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

                edgeMappings.RemoveAll(mapping => replacedEdges.Any(edge => edge.edge.Target.Value == mapping.Item1));
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

            if (edgeMappings.Count > 0)
            {
                addEdge += () =>
                {
                    foreach (var mapping in edgeMappings)
                    {
                        RemovePropertyMappings(mapping.Item1, mapping.Item2);
                    };
                };

                removeEdge += () =>
                {
                    foreach (var mapping in edgeMappings)
                    {
                        AddPropertyMappings(mapping.Item1, mapping.Item2);
                    };
                };
            }

            Action removeNode = () =>
            {
                RemoveWorkflowNode(workflow, workflowNode);
                if (!replaceEdges)
                {
                    foreach (var sibling in siblingEdgesAfter)
                    {
                        sibling.Label.Index--;
                    }
                }
            };

            Action addNode = () =>
            {
                AddWorkflowNode(workflow, workflowNode);
                foreach (var edge in predecessorEdges)
                {
                    edge.Item1.Successors.Insert(edge.Item3, edge.Item2);
                }

                if (!replaceEdges)
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

            var builder = GetGraphNodeBuilder(node);
            var disableBuilder = builder as DisableBuilder;
            var workflowExpressionBuilder = (disableBuilder != null ? disableBuilder.Builder : builder) as IWorkflowExpressionBuilder;
            if (workflowExpressionBuilder != null)
            {
                CloseWorkflowEditorLauncher(workflowExpressionBuilder);
            }
        }

        private void CloseWorkflowEditorLauncher(IWorkflowExpressionBuilder workflowExpressionBuilder)
        {
            CloseWorkflowEditorLauncher(workflowExpressionBuilder, true);
        }

        private void CloseWorkflowEditorLauncher(IWorkflowExpressionBuilder workflowExpressionBuilder, bool removeEditorMapping)
        {
            WorkflowEditorLauncher editorLauncher;
            if (workflowEditorMapping.TryGetValue(workflowExpressionBuilder, out editorLauncher))
            {
                if (editorLauncher.Visible)
                {
                    var workflowGraphView = editorLauncher.WorkflowGraphView;
                    foreach (var node in workflowGraphView.workflow)
                    {
                        var nestedBuilder = ExpressionBuilder.Unwrap(node.Value) as IWorkflowExpressionBuilder;
                        if (nestedBuilder != null)
                        {
                            workflowGraphView.CloseWorkflowEditorLauncher(nestedBuilder, removeEditorMapping);
                        }
                    }
                }

                HideWorkflowEditorLauncher(editorLauncher);
                var removeMapping = removeEditorMapping
                    ? CreateUpdateEditorMappingDelegate(editorMapping => editorMapping.Remove(workflowExpressionBuilder))
                    : () => { };
                var addMapping = CreateUpdateEditorMappingDelegate(editorMapping => editorMapping[workflowExpressionBuilder] = editorLauncher);
                commandExecutor.Execute(removeMapping, addMapping);
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

        private void ReplaceNode(GraphNode node, ExpressionBuilder builder)
        {
            CreateGraphNode(builder, node, CreateGraphNodeType.Successor, branch: false, validate: false);
            DeleteGraphNode(node);
        }

        private bool CanGroup(IEnumerable<GraphNode> nodes, WorkflowBuilder groupBuilder)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException("nodes");
            }

            var workflow = this.workflow;
            var selectedNodes = nodes.Select(node => (Node<ExpressionBuilder, ExpressionBuilderArgument>)node.Tag);
            return !(from node in groupBuilder.Workflow.Sources()
                     let source = FindWorkflowValue(workflow, node.Value)
                     let connectivity = node.DepthFirstSearch()
                                            .Select(successor => FindWorkflowValue(workflow, successor.Value))
                                            .ToArray()
                     from successor in source.DepthFirstSearch()
                     where !connectivity.Contains(successor) && selectedNodes.Contains(successor)
                     select successor).Any();
        }

        public void CreateOrReplaceGroupNode(GraphNode[] selectedNodes, TreeNode typeNode)
        {
            CreateOrReplaceGroupNode(selectedNodes, typeNode.Name);
        }

        private void CreateOrReplaceGroupNode(GraphNode[] selectedNodes, string typeName)
        {
            var selectedNode = selectedNodes.Length == 1 ? selectedNodes[0] : null;
            var selectedNodeBuilder = selectedNode != null ? GetGraphNodeBuilder(selectedNode) : null;

            var includeBuilder = selectedNodeBuilder as IncludeWorkflowBuilder;
            if (includeBuilder != null && includeBuilder.Workflow != null &&
                typeName == typeof(GroupWorkflowBuilder).AssemblyQualifiedName)
            {
                var groupBuilder = new GroupWorkflowBuilder(includeBuilder.Workflow);
                groupBuilder.Name = includeBuilder.Name;
                groupBuilder.Description = includeBuilder.Description;
                ReplaceGroupNode(selectedNode, groupBuilder);
            }
            else if (selectedNodeBuilder == null || selectedNodeBuilder.GetType().AssemblyQualifiedName != typeName)
            {
                var workflowBuilder = selectedNodeBuilder as WorkflowExpressionBuilder;
                if (workflowBuilder != null) ReplaceGroupNode(selectedNode, typeName);
                else GroupGraphNodes(selectedNodes, typeName);
            }
        }

        public void GroupGraphNodes(IEnumerable<GraphNode> nodes)
        {
            var selectedNodes = nodes.ToArray();
            var selectedNode = selectedNodes.Length == 1 ? selectedNodes[0] : null;
            var selectedNodeBuilder = selectedNode != null ? GetGraphNodeBuilder(selectedNode) : null;

            var groupBuilder = selectedNodeBuilder as GroupWorkflowBuilder;
            if (groupBuilder != null) return;

            var includeBuilder = selectedNodeBuilder as IncludeWorkflowBuilder;
            if (includeBuilder != null && includeBuilder.Workflow != null)
            {
                CreateOrReplaceGroupNode(selectedNodes, typeof(GroupWorkflowBuilder).AssemblyQualifiedName);
            }
            else GroupGraphNodes(nodes, graph => new GroupWorkflowBuilder(graph));
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
            GraphNode replacementNode = null;
            var nodeType = CreateGraphNodeType.Successor;
            var workflowBuilder = nodes.ToWorkflowBuilder(recurse: false);
            if (!CanGroup(nodes, workflowBuilder))
            {
                uiService.ShowError("Unable to group broken branches.");
                return;
            }

            var inputIndex = 0;
            var predecessors = (from node in workflow
                                where !node.Value.IsBuildDependency()
                                let graphNode = FindGraphNode(node.Value)
                                where graphNode != null
                                orderby graphNode.Layer descending, graphNode.LayerIndex
                                let unwrapNode = ExpressionBuilder.Unwrap(node.Value)
                                where !workflowBuilder.Workflow.Any(n => n.Value == unwrapNode)
                                from successor in node.Successors
                                let unwrapSuccessor = ExpressionBuilder.Unwrap(successor.Target.Value)
                                let target = workflowBuilder.Workflow.FirstOrDefault(n => n.Value == unwrapSuccessor)
                                where target != null
                                group new { successor.Label.Index, target } by node).ToArray();
            var successors = (from node in workflowBuilder.Workflow
                              let workflowNode = workflow.Single(n => ExpressionBuilder.Unwrap(n.Value) == node.Value)
                              from successor in workflowNode.Successors
                              let unwrapSuccessor = ExpressionBuilder.Unwrap(successor.Target.Value)
                              where !workflowBuilder.Workflow.Any(n => n.Value == unwrapSuccessor)
                              group new { successor, node, workflowNode } by successor.Target).ToArray();

            foreach (var predecessor in predecessors)
            {
                var workflowInput = new WorkflowInputBuilder { Index = inputIndex++ };
                var inputNode = workflowBuilder.Workflow.Add(workflowInput);
                foreach (var edge in predecessor)
                {
                    workflowBuilder.Workflow.AddEdge(inputNode, edge.target, new ExpressionBuilderArgument(edge.Index));
                }
            }

            var sinks = workflowBuilder.Workflow.Sinks().ToArray();
            if (sinks.Length == 1)
            {
                var sink = sinks.First();
                var workflowOutput = new WorkflowOutputBuilder();
                var outputNode = workflowBuilder.Workflow.Add(workflowOutput);
                workflowBuilder.Workflow.AddEdge(sink, outputNode, new ExpressionBuilderArgument());

                var sinkNode = graphView.Nodes.LayeredNodes().Single(node => GetGraphNodeBuilder(node) == sink.Value);
                if (sinkNode.Successors.Count() > 0)
                {
                    replacementNode = sinkNode;
                    nodeType = CreateGraphNodeType.Predecessor;
                }
            }

            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            var workflowExpressionBuilder = groupFactory(workflowBuilder.Workflow.ToInspectableGraph(recurse: false));
            var updateSelectedNode = CreateUpdateSelectionDelegate(workflowExpressionBuilder);

            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(() => { }, updateGraphLayout);
            foreach (var node in nodes.Where(n => n != replacementNode))
            {
                DeleteGraphNode(node, replaceEdges: false);
            }

            CreateGraphNode(workflowExpressionBuilder,
                            replacementNode,
                            nodeType,
                            branch: false,
                            validate: false);

            // Connect grouped node predecessors and successors
            var predecessorEdges = new List<Tuple<Node<ExpressionBuilder, ExpressionBuilderArgument>, Edge<ExpressionBuilder, ExpressionBuilderArgument>>>();
            var successorEdges = new List<Tuple<Node<ExpressionBuilder, ExpressionBuilderArgument>, Edge<ExpressionBuilder, ExpressionBuilderArgument>>>();
            commandExecutor.Execute(() =>
            {
                var linkIndex = 0;
                var groupNode = workflow.Single(node => ExpressionBuilder.Unwrap(node.Value) == workflowExpressionBuilder);
                foreach (var predecessor in predecessors)
                {
                    var predecessorEdge = predecessor.Key.Successors
                        .FirstOrDefault(edge => edge.Target == groupNode && edge.Label.Index == linkIndex);
                    if (predecessorEdge == null)
                    {
                        var edge = workflow.AddEdge(predecessor.Key, groupNode, new ExpressionBuilderArgument { Index = linkIndex });
                        predecessorEdges.Add(Tuple.Create(predecessor.Key, edge));
                    }

                    linkIndex++;
                }

                foreach (var successor in successors)
                {
                    linkIndex = workflow.PredecessorEdges(successor.Key).Count();
                    var sinkNode = replacementNode == null ? groupNode : GetGraphNodeTag(workflow, replacementNode);
                    var successorEdge = sinkNode.Successors.FirstOrDefault(edge => edge.Target == successor.Key);
                    if (successorEdge == null)
                    {
                        var edge = workflow.AddEdge(sinkNode, successor.Key, new ExpressionBuilderArgument { Index = linkIndex });
                        successorEdges.Add(Tuple.Create(groupNode, edge));
                    }
                }
            },
            () =>
            {
                foreach (var edge in predecessorEdges)
                {
                    workflow.RemoveEdge(edge.Item1, edge.Item2);
                }

                foreach (var edge in successorEdges)
                {
                    workflow.RemoveEdge(edge.Item1, edge.Item2);
                }
            });

            if (replacementNode != null) DeleteGraphNode(replacementNode);
            commandExecutor.Execute(() =>
            {
                updateGraphLayout();
                updateSelectedNode();
            },
            () => { });
            commandExecutor.EndCompositeCommand();
        }

        private void ReplaceGroupNode(GraphNode node, string typeName)
        {
            var workflowBuilder = GetGraphNodeBuilder(node) as WorkflowExpressionBuilder;
            if (workflowBuilder == null)
            {
                throw new ArgumentException("Replaced node must be workflow expression builder.", "node");
            }

            var builder = CreateWorkflowBuilder(typeName, workflowBuilder.Workflow);
            builder.Name = workflowBuilder.Name;
            builder.Description = workflowBuilder.Description;
            ReplaceGroupNode(node, builder);
        }

        private void ReplaceGroupNode(GraphNode node, WorkflowExpressionBuilder builder)
        {
            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            var selectCreatedNode = CreateUpdateSelectionDelegate(builder);
            var selectDeletedNode = CreateUpdateSelectionDelegate(node);

            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(() => { }, () =>
            {
                updateGraphLayout();
                selectDeletedNode();
            });
            ReplaceNode(node, builder);
            commandExecutor.Execute(() =>
            {
                updateGraphLayout();
                selectCreatedNode();
            },
            () => { });
            commandExecutor.EndCompositeCommand();
        }

        private void UpdateGraphNodes(IEnumerable<GraphNode> nodes, Action<GraphNode> action)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException("nodes");
            }

            if (!nodes.Any()) return;
            var selectedNodes = nodes.ToArray();
            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            var updateSelectedNode = CreateUpdateSelectionDelegate();
            var restoreSelectedNodes = CreateUpdateSelectionDelegate(selectedNodes);

            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(() => { }, restoreSelectedNodes);
            commandExecutor.Execute(updateSelectedNode, updateGraphLayout);
            foreach (var node in selectedNodes)
            {
                action(node);
            }

            commandExecutor.Execute(updateGraphLayout, () => { });
            commandExecutor.EndCompositeCommand();
        }

        private void UngroupGraphNode(GraphNode node)
        {
            var workflow = this.workflow;
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            var workflowNode = GetGraphNodeTag(workflow, node);
            var workflowBuilder = ExpressionBuilder.Unwrap(workflowNode.Value) as WorkflowExpressionBuilder;
            if (workflowBuilder == null)
            {
                return;
            }

            var predecessors = workflow.PredecessorEdges(workflowNode).OrderBy(edge => edge.Item3).Select(xs => xs.Item1.Value).ToArray();
            var successors = workflowNode.Successors.Select(xs => xs.Target.Value).ToArray();
            var groupWorkflow = new ExpressionBuilderGraph();
            groupWorkflow.AddDescriptor(workflowBuilder.Workflow.ToDescriptor());

            var groupSources = (from n in groupWorkflow
                                let source = ExpressionBuilder.Unwrap(n.Value) as WorkflowInputBuilder
                                where source != null
                                orderby source.Index ascending
                                select n).ToArray();
            var groupSinks = (from n in groupWorkflow
                              let sink = ExpressionBuilder.Unwrap(n.Value) as WorkflowOutputBuilder
                              where sink != null
                              select n).ToArray();
            var groupOutputs = groupSinks.Take(1)
                .Select(groupWorkflow.PredecessorEdges)
                .SelectMany(edges => edges.OrderBy(edge => edge.Item3))
                .ToArray();
            foreach (var terminal in groupSources.Concat(groupSinks))
            {
                groupWorkflow.Remove(terminal);
            }

            DeleteGraphNode(node, replaceEdges: false);
            InsertGraphElements(groupWorkflow, CreateGraphNodeType.Successor, false);

            // Connect incoming nodes to internal targets
            var mainSink = groupSinks.FirstOrDefault();
            var inputConnections = predecessors
                .Select(xs => FindGraphNode(xs))
                .Zip(groupSources, (xs, ys) =>
                    ys.Successors.SelectMany(zs => zs.Target != mainSink
                                     ? Enumerable.Repeat(Tuple.Create(xs, FindGraphNode(zs.Target.Value)), 1)
                                     : successors.Select(ss => Tuple.Create(xs, FindGraphNode(ss)))));
            foreach (var input in inputConnections.SelectMany(xs => xs))
            {
                ConnectGraphNodes(Enumerable.Repeat(input.Item1, 1), input.Item2);
            }

            // Connect output sources to external targets
            var outputConnections = groupOutputs
                .Select(edge => FindGraphNode(edge.Item1.Value))
                .Where(xs => xs != null)
                .SelectMany(xs => successors.Select(edge =>
                    Tuple.Create(xs, FindGraphNode(edge))));
            foreach (var output in outputConnections)
            {
                ConnectGraphNodes(Enumerable.Repeat(output.Item1, 1), output.Item2);
            }
        }

        public void UngroupGraphNodes(IEnumerable<GraphNode> nodes)
        {
            UpdateGraphNodes(nodes, UngroupGraphNode);
        }

        void DisableGraphNode(GraphNode node)
        {
            var builder = GetGraphNodeBuilder(node);
            var disableBuilder = builder as DisableBuilder;
            if (builder != null && disableBuilder == null)
            {
                builder = new DisableBuilder(builder);
                ReplaceNode(node, builder);
            }
        }

        void EnableGraphNode(GraphNode node)
        {
            var builder = GetGraphNodeBuilder(node);
            var disableBuilder = builder as DisableBuilder;
            if (disableBuilder != null)
            {
                builder = disableBuilder.Builder;
                ReplaceNode(node, builder);
            }
        }

        public void DisableGraphNodes(IEnumerable<GraphNode> nodes)
        {
            UpdateGraphNodes(nodes, DisableGraphNode);
        }

        public void EnableGraphNodes(IEnumerable<GraphNode> nodes)
        {
            UpdateGraphNodes(nodes, EnableGraphNode);
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

        private void StoreWorkflowElements()
        {
            editorService.StoreWorkflowElements(selectionModel.SelectedNodes.ToWorkflowBuilder());
        }

        private void ShowClipboardError(InvalidOperationException ex, string message)
        {
            if (ex == null)
            {
                throw new ArgumentNullException("ex");
            }

            // Unwrap XML exceptions when serializing individual workflow elements
            var writerException = ex.InnerException as InvalidOperationException;
            if (writerException != null) ex = writerException;

            var errorMessage = string.Format(message, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            uiService.ShowError(errorMessage);
        }

        public void CutToClipboard()
        {
            try
            {
                StoreWorkflowElements();
                DeleteGraphNodes(selectionModel.SelectedNodes);
            }
            catch (InvalidOperationException ex)
            {
                ShowClipboardError(ex, Resources.CopyToClipboard_Error);
            }
        }

        public void CopyToClipboard()
        {
            try { StoreWorkflowElements(); }
            catch (InvalidOperationException ex)
            {
                ShowClipboardError(ex, Resources.CopyToClipboard_Error);
            }
        }

        public void PasteFromClipboard()
        {
            try
            {
                var builder = editorService.RetrieveWorkflowElements();
                InsertWorkflow(builder.Workflow.ToInspectableGraph());
            }
            catch (InvalidOperationException ex)
            {
                ShowClipboardError(ex, Resources.PasteFromClipboard_Error);
            }
        }

        public void SelectFirstGraphNode()
        {
            var layerCount = graphView.Nodes.Count();
            graphView.SelectedNode = graphView.Nodes
                .Where(layer => layerCount - layer.Key == 1)
                .SelectMany(layer => layer)
                .FirstOrDefault(node => node.Value != null);
        }

        public void SelectAllGraphNodes()
        {
            var graphNodes = graphView.Nodes
                .SelectMany(layer => layer)
                .Where(node => node.Value != null);
            graphView.SelectedNodes = graphNodes;
        }

        public GraphNode FindGraphNode(object value)
        {
            return graphView.Nodes.SelectMany(layer => layer).FirstOrDefault(n => n.Value == value);
        }

        private bool HasDefaultEditor(ExpressionBuilder builder)
        {
            var workflowExpressionBuilder = builder as IWorkflowExpressionBuilder;
            if (workflowExpressionBuilder != null) return true;
            else if (builder != null)
            {
                var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
                var componentEditor = (ComponentEditor)TypeDescriptor.GetEditor(workflowElement, typeof(ComponentEditor));
                if (componentEditor != null) return true;
                else
                {
                    var defaultProperty = TypeDescriptor.GetDefaultProperty(workflowElement);
                    if (defaultProperty != null)
                    {
                        var editor = (UITypeEditor)defaultProperty.GetEditor(typeof(UITypeEditor));
                        if (editor != null && editor.GetEditStyle() == UITypeEditorEditStyle.Modal)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void LaunchDefaultEditor(GraphNode node)
        {
            var builder = GetGraphNodeBuilder(node);
            var disableBuilder = builder as DisableBuilder;
            var workflowBuilder = (disableBuilder != null ? disableBuilder.Builder : builder) as IWorkflowExpressionBuilder;
            if (workflowBuilder != null && workflowBuilder.Workflow != null) LaunchWorkflowView(node);
            else if (builder != null)
            {
                var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
                try
                {
                    if (!uiService.CanShowComponentEditor(workflowElement) || !uiService.ShowComponentEditor(workflowElement, this))
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

                                RefreshEditorNode(node);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    uiService.ShowError(ex);
                }
            }
        }

        private void LaunchVisualizer(GraphNode node)
        {
            var visualizerLauncher = GetVisualizerDialogLauncher(node);
            if (visualizerLauncher != null)
            {
                visualizerLauncher.Show(graphView, serviceProvider);
            }
        }

        public void LaunchWorkflowView(GraphNode node)
        {
            CreateWorkflowView(node, null, Rectangle.Empty, launch: true, activate: true);
        }

        private void CreateWorkflowView(GraphNode node, VisualizerLayout editorLayout, Rectangle bounds, bool launch, bool activate)
        {
            var builder = GetGraphNodeBuilder(node);
            var disableBuilder = builder as DisableBuilder;
            var workflowExpressionBuilder = (disableBuilder != null ? disableBuilder.Builder : builder) as IWorkflowExpressionBuilder;
            if (workflowExpressionBuilder == null || editorLaunching) return;

            editorLaunching = true;
            var launcher = Launcher;
            var readOnly = ReadOnly || disableBuilder != null || workflowExpressionBuilder is IncludeWorkflowBuilder;
            var parentLaunching = launcher != null && launcher.ParentView.editorLaunching;
            var compositeExecutor = new Lazy<CommandExecutor>(() =>
            {
                if (!parentLaunching) commandExecutor.BeginCompositeCommand();
                return commandExecutor;
            }, false);

            WorkflowEditorLauncher editorLauncher;
            if (!workflowEditorMapping.TryGetValue(workflowExpressionBuilder, out editorLauncher))
            {
                Func<WorkflowGraphView> parentSelector;
                Func<WorkflowEditorControl> containerSelector;
                if (workflowExpressionBuilder is IncludeWorkflowBuilder ||
                    workflowExpressionBuilder is GroupWorkflowBuilder)
                {
                    containerSelector = () => launcher != null ? launcher.WorkflowGraphView.editorControl : editorControl;
                }
                else containerSelector = () => null;
                parentSelector = () => launcher != null ? launcher.WorkflowGraphView : this;

                editorLauncher = new WorkflowEditorLauncher(workflowExpressionBuilder, parentSelector, containerSelector);
                editorLauncher.VisualizerLayout = editorLayout;
                editorLauncher.ReadOnly = readOnly;
                editorLauncher.Bounds = bounds;
                var addEditorMapping = CreateUpdateEditorMappingDelegate(editorMapping => editorMapping.Add(workflowExpressionBuilder, editorLauncher));
                var removeEditorMapping = CreateUpdateEditorMappingDelegate(editorMapping => editorMapping.Remove(workflowExpressionBuilder));
                compositeExecutor.Value.Execute(addEditorMapping, removeEditorMapping);
            }

            if (launch && (!editorLauncher.Visible || activate))
            {
                var highlight = node.Highlight;
                var visible = editorLauncher.Visible;
                var editorService = this.editorService;
                var serviceProvider = this.serviceProvider;
                var windowSelector = CreateWindowOwnerSelectorDelegate();
                Action launchEditor = () =>
                {
                    if (editorLauncher.Builder.Workflow != null)
                    {
                        editorLauncher.Show(windowSelector(), serviceProvider);
                        if (editorLauncher.Container != null && !parentLaunching && activate)
                        {
                            editorLauncher.Container.SelectTab(editorLauncher.WorkflowGraphView);
                        }

                        if (highlight && !visible)
                        {
                            editorService.RefreshEditor();
                        }
                    }
                };

                if (visible) launchEditor();
                else compositeExecutor.Value.Execute(launchEditor, editorLauncher.Hide);
            }

            if (compositeExecutor.IsValueCreated && !parentLaunching)
            {
                compositeExecutor.Value.EndCompositeCommand();
            }
            editorLaunching = false;
        }

        internal void UpdateSelection()
        {
            selectionModel.UpdateSelection(this);
        }

        internal void CloseWorkflowView(IWorkflowExpressionBuilder workflowExpressionBuilder)
        {
            commandExecutor.BeginCompositeCommand();
            CloseWorkflowEditorLauncher(workflowExpressionBuilder, false);
            commandExecutor.EndCompositeCommand();
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
            var workflowExpressionBuilder = GetGraphNodeBuilder(node) as IWorkflowExpressionBuilder;
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

        private VisualizerDialogSettings CreateLayoutSettings(ExpressionBuilder builder)
        {
            VisualizerDialogSettings dialogSettings;
            WorkflowEditorLauncher editorLauncher;
            var workflowExpressionBuilder = ExpressionBuilder.GetWorkflowElement(builder) as IWorkflowExpressionBuilder;
            if (workflowExpressionBuilder != null &&
                workflowEditorMapping.TryGetValue(workflowExpressionBuilder, out editorLauncher))
            {
                if (editorLauncher.Visible) editorLauncher.UpdateEditorLayout();
                dialogSettings = new WorkflowEditorSettings
                {
                    EditorVisualizerLayout = editorLauncher.Visible ? editorLauncher.VisualizerLayout : null,
                    EditorDialogSettings = new VisualizerDialogSettings
                    {
                        Visible = editorLauncher.Visible,
                        Bounds = ScaleBounds(editorLauncher.Bounds, inverseScaleFactor),
                        Tag = editorLauncher
                    }
                };
            }
            else dialogSettings = new VisualizerDialogSettings();
            dialogSettings.Tag = builder;
            return dialogSettings;
        }

        private void SetVisualizerLayout(VisualizerLayout layout)
        {
            if (workflow == null)
            {
                throw new InvalidOperationException("Cannot set visualizer layout with a null workflow.");
            }

            visualizerLayout = layout ?? new VisualizerLayout();
            foreach (var node in workflow)
            {
                var layoutSettings = GetLayoutSettings(node.Value);
                if (layoutSettings == null) layoutSettings = CreateLayoutSettings(node.Value);
                else layoutSettings.Tag = node.Value;

                var graphNode = graphView.Nodes.SelectMany(layer => layer).First(n => n.Value == node.Value);
                var workflowEditorSettings = layoutSettings as WorkflowEditorSettings;
                if (workflowEditorSettings != null && workflowEditorSettings.EditorDialogSettings.Tag == null)
                {
                    var editorLayout = workflowEditorSettings.EditorVisualizerLayout;
                    var editorVisible = workflowEditorSettings.EditorDialogSettings.Visible;
                    var editorBounds = ScaleBounds(workflowEditorSettings.EditorDialogSettings.Bounds, scaleFactor);
                    CreateWorkflowView(graphNode,
                                       editorLayout,
                                       editorBounds,
                                       launch: editorVisible,
                                       activate: false);
                }

                visualizerLayout.DialogSettings.Add(layoutSettings);
            }
        }

        public void UpdateVisualizerLayout()
        {
            var updatedLayout = new VisualizerLayout();
            foreach (var node in workflow)
            {
                var builder = node.Value;
                VisualizerDialogSettings dialogSettings;
                VisualizerDialogLauncher visualizerDialog;
                if (visualizerMapping != null && visualizerMapping.TryGetValue(builder as InspectBuilder, out visualizerDialog))
                {
                    var visible = visualizerDialog.Visible;
                    if (!editorState.WorkflowRunning)
                    {
                        visualizerDialog.Hide();
                    }

                    var visualizer = visualizerDialog.Visualizer;
                    dialogSettings = CreateLayoutSettings(builder);
                    var mashupVisualizer = visualizer.IsValueCreated ? visualizer.Value as DialogMashupVisualizer : null;
                    if (mashupVisualizer != null)
                    {
                        foreach (var mashup in mashupVisualizer.Mashups)
                        {
                            var predecessorIndex = (from element in visualizerMapping.Select((mapping, index) => new { mapping, index })
                                                    where element.mapping.Value.Source.Output == mashup.Source
                                                    select element.index)
                                                   .FirstOrDefault();
                            dialogSettings.Mashups.Add(predecessorIndex);
                        }
                    }

                    dialogSettings.Visible = visible;
                    dialogSettings.Bounds = ScaleBounds(visualizerDialog.Bounds, inverseScaleFactor);
                    dialogSettings.WindowState = visualizerDialog.WindowState;

                    if (visualizer.IsValueCreated)
                    {
                        var visualizerType = visualizer.Value.GetType();
                        if (visualizerType.IsPublic)
                        {
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
                    }
                }
                else
                {
                    dialogSettings = GetLayoutSettings(builder);
                    if (dialogSettings == null) dialogSettings = CreateLayoutSettings(builder);
                    else
                    {
                        var workflowExpressionBuilder = ExpressionBuilder.Unwrap(builder) as IWorkflowExpressionBuilder;
                        if (workflowExpressionBuilder != null)
                        {
                            var updatedEditorSettings = CreateLayoutSettings(builder);
                            updatedEditorSettings.Bounds = dialogSettings.Bounds;
                            updatedEditorSettings.Visible = dialogSettings.Visible;
                            updatedEditorSettings.WindowState = dialogSettings.WindowState;
                            updatedEditorSettings.VisualizerTypeName = dialogSettings.VisualizerTypeName;
                            updatedEditorSettings.VisualizerSettings = dialogSettings.VisualizerSettings;
                            foreach (var mashup in dialogSettings.Mashups)
                            {
                                updatedEditorSettings.Mashups.Add(mashup);
                            }

                            dialogSettings = updatedEditorSettings;
                        }
                    }
                }

                updatedLayout.DialogSettings.Add(dialogSettings);
            }

            visualizerLayout = updatedLayout;
            if (!editorState.WorkflowRunning)
            {
                visualizerMapping = null;
            }
        }

        public void RefreshSelection()
        {
            foreach (var node in graphView.SelectedNodes)
            {
                RefreshEditorNode(node);
            }
        }

        void RefreshEditorNode(GraphNode node)
        {
            graphView.Invalidate(node);
            var editor = GetWorkflowEditorLauncher(node);
            if (editor != null && editor.Visible)
            {
                editor.UpdateEditorText();
            }
        }

        private void UpdateGraphLayout()
        {
            UpdateGraphLayout(validateWorkflow: true);
        }

        private void UpdateGraphLayout(bool validateWorkflow)
        {
            graphView.Nodes = workflow.ConnectedComponentLayering().ToList();
            graphView.Invalidate();
            if (validateWorkflow)
            {
                editorService.ValidateWorkflow();
            }

            UpdateVisualizerLayout();
            editorControl.SelectTab(this);
            UpdateSelection();
        }

        #endregion

        #region Controller

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            scaleFactor = factor;
            inverseScaleFactor = new SizeF(1f / factor.Width, 1f / factor.Height);
            base.ScaleControl(factor, specified);
        }

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
            if (CanEdit) return;
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

        private void EnsureDragVisible(DragEventArgs e)
        {
            const int DragOffset = 50;
            var halfWidth = graphView.Width / 2;
            var halfHeight = graphView.Height / 2;
            var location = new Point(e.X, e.Y);
            location = graphView.PointToClient(location);
            location.X += DragOffset * Math.Sign(location.X - halfWidth);
            location.Y += DragOffset * Math.Sign(location.Y - halfHeight);
            graphView.EnsureVisible(location);
        }

        private void graphView_DragOver(object sender, DragEventArgs e)
        {
            EnsureDragVisible(e);
            if (CanEdit) return;
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
                var branch = (e.KeyState & AltModifier) != 0;
                var shift = (e.KeyState & ShiftModifier) != 0;
                var nodeType = shift ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
                var dropLocation = graphView.PointToClient(new Point(e.X, e.Y));
                if (e.Effect == DragDropEffects.Copy)
                {
                    var group = (e.KeyState & CtrlModifier) != 0;
                    if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                    {
                        var path = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                        if (path.Length == 1)
                        {
                            WorkflowBuilder workflowBuilder;
                            try { workflowBuilder = editorService.LoadWorkflow(path[0]); }
                            catch (InvalidOperationException ex)
                            {
                                uiService.ShowError(ex.InnerException, Resources.OpenWorkflow_Error);
                                return;
                            }

                            var groupBuilder = new GroupWorkflowBuilder(workflowBuilder.Workflow);
                            groupBuilder.Name = Path.GetFileNameWithoutExtension(path[0]);
                            groupBuilder.Description = workflowBuilder.Description;
                            CreateGraphNode(groupBuilder, graphView.SelectedNodes.FirstOrDefault(), nodeType, branch);
                        }
                    }
                    else
                    {
                        var typeNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
                        InsertGraphNode(typeNode, nodeType, branch, group);
                    }
                }

                if (e.Effect == DragDropEffects.Link)
                {
                    if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                    {
                        var path = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                        var includeBuilder = new IncludeWorkflowBuilder { Path = PathConvert.GetProjectPath(path[0]) };
                        CreateGraphNode(includeBuilder, graphView.SelectedNodes.FirstOrDefault(), nodeType, branch);
                    }
                    else
                    {
                        var linkNode = graphView.GetNodeAt(dropLocation);
                        if (linkNode != null)
                        {
                            if (shift) DisconnectGraphNodes(dragSelection, linkNode);
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
                else editorService.StartWorkflow(!e.Control);
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
                else if (graphView.SelectedNode != null && graphView.CursorNode != graphView.SelectedNode)
                {
                    if (e.Modifiers == Keys.Shift && CanDisconnect(graphView.SelectedNodes, graphView.CursorNode))
                    {
                        DisconnectGraphNodes(graphView.SelectedNodes, graphView.CursorNode);
                    }
                    else if (CanConnect(graphView.SelectedNodes, graphView.CursorNode))
                    {
                        ConnectGraphNodes(graphView.SelectedNodes, graphView.CursorNode);
                    }
                }
                else
                {
                    LaunchDefaultEditor(graphView.SelectedNode);
                }
            }

            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                SelectAllGraphNodes();
            }

            if (e.KeyCode == Keys.C && e.Modifiers.HasFlag(Keys.Control))
            {
                CopyToClipboard();
            }

            if (e.KeyCode == Keys.Back && e.Modifiers == Keys.Control)
            {
                var launcher = Launcher;
                if (launcher != null && launcher.ParentView != null)
                {
                    var parentView = launcher.ParentView;
                    var parentEditor = parentView.EditorControl;
                    var parentEditorForm = parentEditor.ParentForm;
                    if (editorControl.ParentForm != parentEditorForm)
                    {
                        parentEditorForm.Activate();
                    }

                    var parentNode = parentView.Workflow.FirstOrDefault(node => ExpressionBuilder.Unwrap(node.Value) == launcher.Builder);
                    if (parentNode != null)
                    {
                        var graphNode = parentView.FindGraphNode(parentNode.Value);
                        parentView.GraphView.SelectedNode = graphNode;
                        parentEditor.SelectTab(parentView);
                        parentView.GraphView.Select();
                    }
                }
            }

            if (!CanEdit)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    DeleteGraphNodes(selectionModel.SelectedNodes);
                }

                if (e.KeyCode == Keys.D && e.Modifiers.HasFlag(Keys.Control) && selectionModel.SelectedNodes.Any())
                {
                    if (e.Modifiers.HasFlag(Keys.Shift))
                    {
                        EnableGraphNodes(selectionModel.SelectedNodes);
                    }
                    else DisableGraphNodes(selectionModel.SelectedNodes);
                }

                if (e.KeyCode == Keys.X && e.Modifiers.HasFlag(Keys.Control))
                {
                    CutToClipboard();
                }

                if (e.KeyCode == Keys.V && e.Modifiers.HasFlag(Keys.Control))
                {
                    PasteFromClipboard();
                }

                if (e.KeyCode == Keys.G && e.Modifiers.HasFlag(Keys.Control))
                {
                    if (e.Modifiers.HasFlag(Keys.Shift) && selectionModel.SelectedNodes.Any())
                    {
                        UngroupGraphNodes(selectionModel.SelectedNodes);
                    }
                    else GroupGraphNodes(selectionModel.SelectedNodes);
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
            UpdateSelection();
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
            themeRenderer.ThemeChanged -= themeRenderer_ThemeChanged;
        }

        private void editorService_WorkflowStarted(object sender, EventArgs e)
        {
            InitializeVisualizerMapping();
        }

        private void themeRenderer_ThemeChanged(object sender, EventArgs e)
        {
            InitializeTheme();
        }

        private void InitializeTheme()
        {
            BackColor = themeRenderer.ToolStripRenderer.ColorTable.WindowBackColor;
            graphView.BackColor = themeRenderer.ToolStripRenderer.ColorTable.WindowBackColor;
        }

        #endregion

        #region Context Menu

        private void defaultEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LaunchDefaultEditor(graphView.SelectedNode);
        }

        private void saveSnippetAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorService.OnKeyDown(new KeyEventArgs(Keys.Control | Keys.Shift | Keys.S));
        }

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

        private void ungroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UngroupGraphNodes(selectionModel.SelectedNodes);
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

        private void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EnableGraphNodes(selectionModel.SelectedNodes);
        }

        private void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisableGraphNodes(selectionModel.SelectedNodes);
        }

        private void InitializeConnectionSource()
        {
            isContextMenuSource = true;
            dragSelection = graphView.SelectedNodes.ToArray();
            SetDragCursor(DragDropEffects.Link);
        }

        private void InitializeOutputMenuItem(ToolStripMenuItem menuItem, string memberSelector, Type memberType)
        {
            var typeName = GetTypeName(memberType);
            menuItem.Text += string.Format(" ({0})", typeName);
            menuItem.Name = memberSelector;
            menuItem.Tag = memberType;
        }

        static string GetTypeName(Type type)
        {
            using (var provider = new CSharpCodeProvider())
            {
                var typeRef = new CodeTypeReference(type);
                return provider.GetTypeOutput(typeRef);
            }
        }

        //TODO: Consider refactoring this method into the core API to avoid redundancy
        static IEnumerable<PropertyInfo> GetProperties(Type type, BindingFlags bindingAttr)
        {
            IEnumerable<PropertyInfo> properties = type.GetProperties(bindingAttr);
            if (type.IsInterface)
            {
                properties = properties.Concat(type
                    .GetInterfaces()
                    .SelectMany(i => i.GetProperties(bindingAttr)));
            }
            return properties;
        }

        private IDisposable CreateOutputMenuItems(Type type, ToolStripMenuItem ownerItem, GraphNode selectedNode)
        {
            if (type.IsEnum) return Disposable.Empty;
            var root = string.IsNullOrEmpty(ownerItem.Name);

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                                      .OrderBy(member => member.MetadataToken))
            {
                var memberSelector = root ? field.Name : string.Join(ExpressionHelper.MemberSeparator, ownerItem.Name, field.Name);
                var menuItem = CreateOutputMenuItem(field.Name, memberSelector, field.FieldType, selectedNode);
                ownerItem.DropDownItems.Add(menuItem);
            }

            foreach (var property in GetProperties(type, BindingFlags.Instance | BindingFlags.Public)
                                         .Distinct(PropertyInfoComparer.Default)
                                         .OrderBy(member => member.MetadataToken))
            {
                var memberSelector = root ? property.Name : string.Join(ExpressionHelper.MemberSeparator, ownerItem.Name, property.Name);
                var menuItem = CreateOutputMenuItem(property.Name, memberSelector, property.PropertyType, selectedNode);
                ownerItem.DropDownItems.Add(menuItem);
            }

            var menuItemDisposable = new CompositeDisposable();
            EventHandler dropDownOpeningHandler = delegate
            {
                foreach (ToolStripMenuItem item in ownerItem.DropDownItems)
                {
                    var itemDisposable = CreateOutputMenuItems((Type)item.Tag, item, selectedNode);
                    menuItemDisposable.Add(itemDisposable);
                }
            };

            EventHandler dropDownClosedHandler = delegate
            {
                menuItemDisposable.Clear();
                foreach (ToolStripMenuItem item in ownerItem.DropDownItems)
                {
                    item.DropDownItems.Clear();
                }
            };

            ownerItem.DropDownOpening += dropDownOpeningHandler;
            ownerItem.DropDownClosed += dropDownClosedHandler;
            return new CompositeDisposable(
                Disposable.Create(() =>
                {
                    ownerItem.DropDownClosed -= dropDownClosedHandler;
                    ownerItem.DropDownOpening -= dropDownOpeningHandler;
                }),
                menuItemDisposable);
        }

        private ToolStripMenuItem CreateOutputMenuItem(string memberName, string memberSelector, Type memberType, GraphNode selectedNode)
        {
            var menuItem = new ToolStripMenuItem(memberName, null, delegate
            {
                var builder = new MemberSelectorBuilder { Selector = memberSelector };
                var successor = selectedNode.Successors.Select(edge => GetGraphNodeBuilder(edge.Node)).FirstOrDefault();
                CreateGraphNode(builder, selectedNode, CreateGraphNodeType.Successor, branch: true);
                contextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
            });

            InitializeOutputMenuItem(menuItem, memberSelector, memberType);
            return menuItem;
        }

        private void CreateSubjectTypeMenuItems(InspectBuilder inspectBuilder, ToolStripMenuItem ownerItem, GraphNode selectedNode)
        {
            var subscribeBuilder = inspectBuilder.Builder as SubscribeSubjectBuilder;
            if (subscribeBuilder != null)
            {
                var subscribeBuilderType = subscribeBuilder.GetType();
                var subjectType = inspectBuilder.ObservableType ?? subscribeBuilderType.GetGenericArguments().FirstOrDefault();
                if (subjectType != null)
                {
                    var noneMenuItem = CreateSubjectTypeMenuItem(null, subscribeBuilder, selectedNode);
                    var typeMenuItem = CreateSubjectTypeMenuItem(subjectType, subscribeBuilder, selectedNode);
                    typeMenuItem.Checked = subscribeBuilderType.IsGenericType;
                    noneMenuItem.Checked = !typeMenuItem.Checked;
                    ownerItem.DropDownItems.Add(noneMenuItem);
                    ownerItem.DropDownItems.Add(typeMenuItem);
                    ownerItem.Enabled = true;
                    ownerItem.Visible = true;
                }
            }
        }

        private ToolStripMenuItem CreateSubjectTypeMenuItem(
            Type memberType,
            SubscribeSubjectBuilder subscribeSubject,
            GraphNode selectedNode)
        {
            ToolStripMenuItem menuItem = null;
            var typeName = memberType == null ? Resources.ContextMenu_NoneMenuItemLabel : GetTypeName(memberType);
            menuItem = new ToolStripMenuItem(typeName, null, delegate
            {
                if (!menuItem.Checked)
                {
                    var subscribeSubjectType = memberType != null
                        ? typeof(SubscribeSubject<>).MakeGenericType(memberType)
                        : typeof(SubscribeSubjectBuilder);
                    var builder = (SubscribeSubjectBuilder)Activator.CreateInstance(subscribeSubjectType);
                    builder.Name = subscribeSubject.Name;
                    UpdateGraphNodes(new[] { selectedNode }, node => ReplaceNode(node, builder));
                    contextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
                }
            });

            return menuItem;
        }

        static readonly Attribute[] ExternalizableAttributes = new[] { ExternalizableAttribute.Default };

        private void CreateExternalizeMenuItems(object workflowElement, ToolStripMenuItem ownerItem, GraphNode selectedNode)
        {
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(workflowElement, ExternalizableAttributes))
            {
                if (!property.IsBrowsable || property.IsReadOnly && !ExpressionHelper.IsCollectionType(property.PropertyType)) continue;
                var propertySource = workflowElement as PropertySource;
                var externalizedName = propertySource != null ? propertySource.MemberName : property.Name;
                var menuItem = CreateExternalizeMenuItem(property.Name, externalizedName, property.PropertyType, selectedNode);
                ownerItem.DropDownItems.Add(menuItem);
            }
        }

        private ToolStripMenuItem CreateExternalizeMenuItem(
            string memberName,
            string externalizedName,
            Type memberType,
            GraphNode selectedNode)
        {
            var menuItem = new ToolStripMenuItem(externalizedName, null, delegate
            {
                var property = new ExternalizedProperty { MemberName = memberName, Name = externalizedName };
                CreateGraphNode(property, selectedNode, CreateGraphNodeType.Predecessor, branch: true);
                contextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
            });

            menuItem.Enabled = !workflow
                .Predecessors(GetGraphNodeTag(workflow, selectedNode))
                .Select(node => ExpressionBuilder.Unwrap(node.Value) as ExternalizedProperty)
                .Any(property => property != null && property.MemberName == memberName);
            InitializeOutputMenuItem(menuItem, memberName, memberType);
            return menuItem;
        }

        private void CreatePropertySourceMenuItems(object workflowElement, ToolStripMenuItem ownerItem)
        {
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(workflowElement, ExternalizableAttributes))
            {
                if (property.IsReadOnly || !property.IsBrowsable) continue;
                var memberValue = property.GetValue(workflowElement);
                var menuItem = CreatePropertySourceMenuItem(property.ComponentType, property.Name, property.PropertyType, memberValue);
                ownerItem.DropDownItems.Add(menuItem);
            }
        }

        private ToolStripMenuItem CreatePropertySourceMenuItem(
            Type elementType,
            string memberName,
            Type memberType,
            object memberValue)
        {
            var menuItem = new ToolStripMenuItem(memberName, null, delegate
            {
                var propertySourceType = typeof(PropertySource<,>).MakeGenericType(elementType, memberType);
                var propertySource = (PropertySource)Activator.CreateInstance(propertySourceType);
                var valueProperty = propertySourceType.GetProperty("Value");
                valueProperty.SetValue(propertySource, memberValue);
                propertySource.MemberName = memberName;

                CreateGraphNode(propertySource, null, CreateGraphNodeType.Predecessor, branch: true);
                contextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
            });

            InitializeOutputMenuItem(menuItem, memberName, memberType);
            return menuItem;
        }

        private ToolStripMenuItem CreateVisualizerMenuItem(string typeName, VisualizerDialogSettings layoutSettings, GraphNode selectedNode)
        {
            ToolStripMenuItem menuItem = null;
            var emptyVisualizer = string.IsNullOrEmpty(typeName);
            var itemText = emptyVisualizer ? Resources.ContextMenu_NoneMenuItemLabel : typeName;
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
                                              (element.ElementTypes.Contains(ElementCategory.Nested) ||
                                               element.FullyQualifiedName == typeof(ConditionBuilder).AssemblyQualifiedName ||
                                               element.FullyQualifiedName == typeof(SinkBuilder).AssemblyQualifiedName)
                                        select element)
                {
                    ToolStripMenuItem menuItem = null;
                    var name = string.Format("{0} ({1})", element.Name, toolboxService.GetPackageDisplayName(element.Namespace));
                    menuItem = new ToolStripMenuItem(name, null, (sender, e) =>
                    {
                        if (menuItem.Checked) return;
                        CreateOrReplaceGroupNode(selectedNodes, element.FullyQualifiedName);
                    });

                    if (workflowBuilder != null &&
                        workflowBuilder.GetType().AssemblyQualifiedName == element.FullyQualifiedName)
                    {
                        menuItem.Checked = true;
                    }

                    if (element.FullyQualifiedName == typeof(GroupWorkflowBuilder).AssemblyQualifiedName)
                    {
                        //make group workflow the first on the list and display shortcut key string
                        menuItem.ShortcutKeys = Keys.Control | Keys.G;
                        groupToolStripMenuItem.DropDownItems.Insert(0, menuItem);
                    }
                    else if (element.FullyQualifiedName != typeof(IncludeWorkflowBuilder).AssemblyQualifiedName)
                    {
                        groupToolStripMenuItem.DropDownItems.Add(menuItem);
                    }
                }
            }
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            // Ensure that the current view is selected 
            if (selectionModel.SelectedView != this)
            {
                selectionModel.UpdateSelection(this);
            }

            var selectedNodes = selectionModel.SelectedNodes.ToArray();
            if (selectedNodes.Length > 0)
            {
                copyToolStripMenuItem.Enabled = true;
                saveSnippetAsToolStripMenuItem.Enabled = true;
            }

            if (!CanEdit)
            {
                pasteToolStripMenuItem.Enabled = true;
                if (selectedNodes.Length > 0)
                {
                    cutToolStripMenuItem.Enabled = true;
                    deleteToolStripMenuItem.Enabled = true;
                    connectToolStripMenuItem.Enabled = true;
                    disconnectToolStripMenuItem.Enabled = true;
                    groupToolStripMenuItem.Enabled = true;
                    ungroupToolStripMenuItem.Enabled = true;
                    enableToolStripMenuItem.Enabled = true;
                    disableToolStripMenuItem.Enabled = true;
                    CreateGroupMenuItems(selectedNodes);
                }
            }

            if (selectedNodes.Length == 1)
            {
                var selectedNode = selectedNodes[0];
                var inspectBuilder = (InspectBuilder)selectedNode.Value;
                if (inspectBuilder != null && inspectBuilder.ObservableType != null)
                {
                    outputToolStripMenuItem.Enabled = !CanEdit;
                    InitializeOutputMenuItem(outputToolStripMenuItem, string.Empty, inspectBuilder.ObservableType);
                    if (outputToolStripMenuItem.Enabled)
                    {
                        outputToolStripMenuItem.Tag = CreateOutputMenuItems(inspectBuilder.ObservableType, outputToolStripMenuItem, selectedNode);
                    }
                }

                var builder = GetGraphNodeBuilder(selectedNode);
                defaultEditorToolStripMenuItem.Enabled = HasDefaultEditor(builder);

                var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
                if (workflowElement != null)
                {
                    if (!CanEdit)
                    {
                        CreateSubjectTypeMenuItems(inspectBuilder, subjectTypeToolStripMenuItem, selectedNode);
                        CreateExternalizeMenuItems(workflowElement, externalizeToolStripMenuItem, selectedNode);
                        CreatePropertySourceMenuItems(workflowElement, createPropertySourceToolStripMenuItem);
                    }

                    externalizeToolStripMenuItem.Enabled = externalizeToolStripMenuItem.DropDownItems.Count > 0;
                    createPropertySourceToolStripMenuItem.Enabled = createPropertySourceToolStripMenuItem.DropDownItems.Count > 0;
                }

                var layoutSettings = GetLayoutSettings(selectedNode.Value);
                if (layoutSettings != null)
                {
                    var activeVisualizer = layoutSettings.VisualizerTypeName;
                    if (editorState.WorkflowRunning)
                    {
                        VisualizerDialogLauncher visualizerLauncher;
                        if (visualizerMapping.TryGetValue(inspectBuilder, out visualizerLauncher))
                        {
                            var visualizer = visualizerLauncher.Visualizer;
                            if (visualizer.IsValueCreated)
                            {
                                activeVisualizer = visualizer.Value.GetType().FullName;
                            }
                        }
                    }

                    if (inspectBuilder != null && inspectBuilder.ObservableType != null &&
                        (!editorState.WorkflowRunning || inspectBuilder.PublishNotifications))
                    {
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
        }

        private void contextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            subjectTypeToolStripMenuItem.Visible = false;
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
            subjectTypeToolStripMenuItem.DropDownItems.Clear();
            externalizeToolStripMenuItem.DropDownItems.Clear();
            createPropertySourceToolStripMenuItem.DropDownItems.Clear();
            visualizerToolStripMenuItem.DropDownItems.Clear();
            groupToolStripMenuItem.DropDownItems.Clear();
        }

        #endregion

        #region PropertyInfoComparer Class

        class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
        {
            public static readonly PropertyInfoComparer Default = new PropertyInfoComparer();

            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(PropertyInfo obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        #endregion
    }

    enum CreateGraphNodeType
    {
        Successor,
        Predecessor
    }
}
