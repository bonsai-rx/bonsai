﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Bonsai.Expressions;
using System.IO;
using Bonsai.Dag;
using System.Windows.Forms.Design;
using Bonsai.Editor.Properties;
using System.Reflection;
using System.Drawing.Design;
using System.Reactive.Disposables;
using Bonsai.Editor.Themes;
using Bonsai.Design;
using Bonsai.Editor.GraphModel;
using System.Xml;

namespace Bonsai.Editor.GraphView
{
    partial class WorkflowGraphView : UserControl
    {
        static readonly Action EmptyAction = () => { };
        static readonly Cursor InvalidSelectionCursor = Cursors.No;
        static readonly Cursor MoveSelectionCursor = Cursors.SizeAll;
        static readonly Cursor AlternateSelectionCursor = Cursors.UpArrow;

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
        GraphNode dragHighlight;
        IEnumerable<GraphNode> dragSelection;
        readonly CommandExecutor commandExecutor;
        readonly WorkflowSelectionModel selectionModel;
        readonly IWorkflowEditorState editorState;
        readonly IWorkflowEditorService editorService;
        readonly TypeVisualizerMap typeVisualizerMap;
        readonly Dictionary<IWorkflowExpressionBuilder, WorkflowEditorLauncher> workflowEditorMapping;
        readonly IServiceProvider serviceProvider;
        readonly IUIService uiService;
        readonly ThemeRenderer themeRenderer;
        readonly IDefinitionProvider definitionProvider;
        Dictionary<InspectBuilder, VisualizerDialogLauncher> visualizerMapping;
        VisualizerLayout visualizerLayout;
        ExpressionBuilderGraph workflow;

        public WorkflowGraphView(IServiceProvider provider, WorkflowEditorControl owner, bool readOnly)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            InitializeComponent();
            EditorControl = owner;
            serviceProvider = provider;
            ReadOnly = readOnly;
            Editor = new WorkflowEditor(provider, graphView);
            uiService = (IUIService)provider.GetService(typeof(IUIService));
            themeRenderer = (ThemeRenderer)provider.GetService(typeof(ThemeRenderer));
            definitionProvider = (IDefinitionProvider)provider.GetService(typeof(IDefinitionProvider));
            commandExecutor = (CommandExecutor)provider.GetService(typeof(CommandExecutor));
            graphView.IconRenderer = (SvgRendererFactory)provider.GetService(typeof(SvgRendererFactory));
            selectionModel = (WorkflowSelectionModel)provider.GetService(typeof(WorkflowSelectionModel));
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
            typeVisualizerMap = (TypeVisualizerMap)provider.GetService(typeof(TypeVisualizerMap));
            editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
            workflowEditorMapping = new Dictionary<IWorkflowExpressionBuilder, WorkflowEditorLauncher>();

            graphView.HandleDestroyed += graphView_HandleDestroyed;
            editorState.WorkflowStarted += editorService_WorkflowStarted;
            themeRenderer.ThemeChanged += themeRenderer_ThemeChanged;
            InitializeTheme();
            InitializeViewBindings();
        }

        internal WorkflowEditor Editor { get; }

        internal WorkflowEditorLauncher Launcher { get; set; }

        internal WorkflowEditorControl EditorControl { get; }

        internal bool ReadOnly { get; }

        internal bool CanEdit
        {
            get { return !ReadOnly && !editorState.WorkflowRunning; }
        }

        public GraphViewControl GraphView
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
                Editor.Workflow = value;
                UpdateEditorWorkflow();
            }
        }

        public static ElementCategory GetToolboxElementCategory(TreeNode typeNode)
        {
            var elementCategories = (ElementCategory[])typeNode.Tag;
            for (int i = 0; i < elementCategories.Length; i++)
            {
                if (elementCategories[i] == ElementCategory.Nested) continue;
                return elementCategories[i];
            }

            return ElementCategory.Combinator;
        }

        private Func<IWin32Window> CreateWindowOwnerSelectorDelegate()
        {
            return Launcher != null ? (Func<IWin32Window>)(() => Launcher.Owner) : () => graphView;
        }

        private Action CreateUpdateEditorMappingDelegate(Action<Dictionary<IWorkflowExpressionBuilder, WorkflowEditorLauncher>> action)
        {
            return Launcher != null
                ? (Action)(() => action(Launcher.WorkflowGraphView.workflowEditorMapping))
                : () => action(workflowEditorMapping);
        }

        #region Model

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

        private void UpdateEditorWorkflow()
        {
            UpdateGraphLayout(validateWorkflow: false);
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

        private void InitializeVisualizerMapping()
        {
            if (workflow == null) return;
            visualizerMapping = LayoutHelper.CreateVisualizerMapping(
                workflow,
                visualizerLayout,
                typeVisualizerMap,
                serviceProvider,
                graphView,
                this);
        }

        private void CloseWorkflowEditorLauncher(IWorkflowExpressionBuilder workflowExpressionBuilder)
        {
            CloseWorkflowEditorLauncher(workflowExpressionBuilder, true);
        }

        private void CloseWorkflowEditorLauncher(IWorkflowExpressionBuilder workflowExpressionBuilder, bool removeEditorMapping)
        {
            if (workflowEditorMapping.TryGetValue(workflowExpressionBuilder, out WorkflowEditorLauncher editorLauncher))
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
                    : EmptyAction;
                var addMapping = CreateUpdateEditorMappingDelegate(editorMapping => editorMapping[workflowExpressionBuilder] = editorLauncher);
                commandExecutor.Execute(removeMapping, addMapping);
            }
        }

        private void InsertWorkflow(ExpressionBuilderGraph workflow)
        {
            var branch = ModifierKeys.HasFlag(BranchModifier);
            var nodeType = ModifierKeys.HasFlag(PredecessorModifier) ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
            InsertWorkflow(workflow, nodeType, branch);
        }

        private void InsertWorkflow(ExpressionBuilderGraph workflow, CreateGraphNodeType nodeType, bool branch)
        {
            if (workflow.Count > 0)
            {
                commandExecutor.BeginCompositeCommand();
                Editor.InsertGraphElements(workflow, nodeType, branch);
                commandExecutor.EndCompositeCommand();
            }
        }

        private void StoreWorkflowElements()
        {
            var text = ElementStore.StoreWorkflowElements(selectionModel.SelectedNodes.ToWorkflowBuilder());
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text);
            }
        }

        private void ShowClipboardError(InvalidOperationException ex, string message)
        {
            if (ex == null)
            {
                throw new ArgumentNullException(nameof(ex));
            }

            // Unwrap XML exceptions when serializing individual workflow elements
            if (ex.InnerException is InvalidOperationException writerException) ex = writerException;
            var errorMessage = string.Format(message, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            uiService.ShowError(errorMessage);
        }

        public void CutToClipboard()
        {
            try
            {
                StoreWorkflowElements();
                Editor.DeleteGraphNodes(selectionModel.SelectedNodes);
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
                if (Clipboard.ContainsText())
                {
                    var builder = ElementStore.RetrieveWorkflowElements(Clipboard.GetText());
                    InsertWorkflow(builder.Workflow.ToInspectableGraph());
                }
            }
            catch (InvalidOperationException ex)
            {
                ShowClipboardError(ex, Resources.PasteFromClipboard_Error);
            }
        }

        public void CreateGraphNode(string name, string typeName, ElementCategory elementCategory, CreateGraphNodeType nodeType, bool branch, bool group)
        {
            CreateGraphNode(name, typeName, elementCategory, nodeType, branch, group, null);
        }

        public void CreateGraphNode(
            string name,
            string typeName,
            ElementCategory elementCategory,
            CreateGraphNodeType nodeType,
            bool branch,
            bool group,
            string arguments)
        {
            try { Editor.InsertGraphNode(typeName, elementCategory, nodeType, branch, group, arguments); }
            catch (TargetInvocationException e)
            {
                var message = string.Format(Resources.CreateTypeNode_Error, name, e.InnerException.Message);
                uiService.ShowError(message);
            }
            catch (SystemException e)
            {
                var message = string.Format(Resources.CreateTypeNode_Error, name, e.Message);
                uiService.ShowError(message);
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

        public GraphNode FindGraphNode(ExpressionBuilder value)
        {
            return graphView.Nodes.SelectMany(layer => layer).FirstOrDefault(n => n.Value == value);
        }

        internal void SelectBuilderNode(ExpressionBuilder builder)
        {
            var graphNode = FindGraphNode(builder);
            if (graphNode != null)
            {
                GraphView.SelectedNode = graphNode;
                EditorControl.SelectTab(this);
                GraphView.Select();
                UpdateSelection();
            }
        }

        private bool HasDefaultEditor(ExpressionBuilder builder)
        {
            if (builder is IWorkflowExpressionBuilder) return true;
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
            var builder = WorkflowEditor.GetGraphNodeBuilder(node);
            var disableBuilder = builder as DisableBuilder;
            var workflowBuilder = (disableBuilder != null ? disableBuilder.Builder : builder) as IWorkflowExpressionBuilder;
            if (workflowBuilder != null && workflowBuilder.Workflow != null)
            {
                if (workflowBuilder is IncludeWorkflowBuilder) return;
                LaunchWorkflowView(node);
            }
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

        private bool HasDefinition(ExpressionBuilder builder)
        {
            var disableBuilder = builder as DisableBuilder;
            var includeWorkflow = (disableBuilder != null ? disableBuilder.Builder : builder) as IncludeWorkflowBuilder;
            if (includeWorkflow != null && includeWorkflow.Workflow != null) return true;
            else if (builder != null)
            {
                var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
                return definitionProvider.HasDefinition(workflowElement);
            }

            return false;
        }

        private void LaunchDefinition(GraphNode node)
        {
            var builder = WorkflowEditor.GetGraphNodeBuilder(node);
            var disableBuilder = builder as DisableBuilder;
            var includeWorkflow = (disableBuilder != null ? disableBuilder.Builder : builder) as IncludeWorkflowBuilder;
            if (includeWorkflow != null && includeWorkflow.Workflow != null) LaunchWorkflowView(node);
            else if (builder != null)
            {
                var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
                try
                {
                    if (definitionProvider.HasDefinition(workflowElement))
                    {
                        definitionProvider.ShowDefinition(workflowElement);
                    }
                }
                catch (Exception ex)
                {
                    uiService.ShowError(ex);
                }
            }
        }

        public void LaunchWorkflowView(GraphNode node)
        {
            CreateWorkflowView(node, null, Rectangle.Empty, launch: true, activate: true);
        }

        private void CreateWorkflowView(GraphNode node, VisualizerLayout editorLayout, Rectangle bounds, bool launch, bool activate)
        {
            var builder = WorkflowEditor.GetGraphNodeBuilder(node);
            var disableBuilder = builder as DisableBuilder;
            var workflowExpressionBuilder = (disableBuilder != null ? disableBuilder.Builder : builder) as IWorkflowExpressionBuilder;
            if (workflowExpressionBuilder == null || editorLaunching) return;

            editorLaunching = true;
            var parentLaunching = Launcher != null && Launcher.ParentView.editorLaunching;
            var compositeExecutor = new Lazy<CommandExecutor>(() =>
            {
                if (!parentLaunching) commandExecutor.BeginCompositeCommand();
                return commandExecutor;
            }, false);

            try
            {
                if (!workflowEditorMapping.TryGetValue(workflowExpressionBuilder, out WorkflowEditorLauncher editorLauncher))
                {
                    Func<WorkflowGraphView> parentSelector;
                    Func<WorkflowEditorControl> containerSelector;
                    if (workflowExpressionBuilder is IncludeWorkflowBuilder ||
                        workflowExpressionBuilder is GroupWorkflowBuilder)
                    {
                        containerSelector = () => Launcher != null ? Launcher.WorkflowGraphView.EditorControl : EditorControl;
                    }
                    else containerSelector = () => null;
                    parentSelector = () => Launcher != null ? Launcher.WorkflowGraphView : this;

                    editorLauncher = new WorkflowEditorLauncher(workflowExpressionBuilder, parentSelector, containerSelector);
                    editorLauncher.VisualizerLayout = editorLayout;
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
            }
            finally
            {
                if (compositeExecutor.IsValueCreated && !parentLaunching)
                {
                    compositeExecutor.Value.EndCompositeCommand();
                }
                editorLaunching = false;
            }
        }

        internal void UpdateSelection()
        {
            if (selectionModel.SelectedView != this ||
                selectionModel.SelectedNodes != GraphView.SelectedNodes)
            {
                selectionModel.UpdateSelection(this);
            }
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
            if (visualizerMapping != null && node?.Value is InspectBuilder inspectBuilder)
            {
                visualizerMapping.TryGetValue(inspectBuilder, out visualizerDialog);
            }

            return visualizerDialog;
        }

        public WorkflowEditorLauncher GetWorkflowEditorLauncher(GraphNode node)
        {
            var builder = WorkflowEditor.GetGraphNodeBuilder(node);
            var disableBuilder = builder as DisableBuilder;
            if (disableBuilder != null) builder = disableBuilder.Builder;

            var workflowExpressionBuilder = builder as IWorkflowExpressionBuilder;
            if (workflowExpressionBuilder != null)
            {
                workflowEditorMapping.TryGetValue(workflowExpressionBuilder, out WorkflowEditorLauncher editorLauncher);
                return editorLauncher;
            }

            return null;
        }

        private VisualizerDialogSettings CreateLayoutSettings(ExpressionBuilder builder)
        {
            VisualizerDialogSettings dialogSettings;
            if (ExpressionBuilder.GetWorkflowElement(builder) is IWorkflowExpressionBuilder workflowExpressionBuilder &&
                workflowEditorMapping.TryGetValue(workflowExpressionBuilder, out WorkflowEditorLauncher editorLauncher))
            {
                if (editorLauncher.Visible) editorLauncher.UpdateEditorLayout();
                dialogSettings = new WorkflowEditorSettings
                {
                    EditorVisualizerLayout = editorLauncher.Visible ? editorLauncher.VisualizerLayout : null,
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

        private void SetVisualizerLayout(VisualizerLayout layout)
        {
            if (workflow == null)
            {
                throw new InvalidOperationException(Resources.VisualizerLayoutOnNullWorkflow_Error);
            }

            visualizerLayout = layout ?? new VisualizerLayout();
            foreach (var node in workflow)
            {
                var layoutSettings = visualizerLayout.GetLayoutSettings(node.Value);
                if (layoutSettings == null)
                {
                    layoutSettings = CreateLayoutSettings(node.Value);
                    visualizerLayout.DialogSettings.Add(layoutSettings);
                }
                else layoutSettings.Tag = node.Value;

                var graphNode = graphView.Nodes.SelectMany(layer => layer).First(n => n.Value == node.Value);
                if (layoutSettings is WorkflowEditorSettings workflowEditorSettings &&
                    workflowEditorSettings.EditorDialogSettings.Tag == null)
                {
                    var editorLayout = workflowEditorSettings.EditorVisualizerLayout;
                    var editorVisible = workflowEditorSettings.EditorDialogSettings.Visible;
                    var editorBounds = workflowEditorSettings.EditorDialogSettings.Bounds;
                    CreateWorkflowView(graphNode,
                                       editorLayout,
                                       editorBounds,
                                       launch: editorVisible,
                                       activate: false);
                }
            }
        }

        public void UpdateVisualizerLayout()
        {
            var updatedLayout = new VisualizerLayout();
            var topologicalOrder = workflow.TopologicalSort();
            foreach (var node in topologicalOrder)
            {
                var builder = node.Value;
                VisualizerDialogSettings dialogSettings;
                if (visualizerMapping != null &&
                    visualizerMapping.TryGetValue(builder as InspectBuilder, out VisualizerDialogLauncher visualizerDialog))
                {
                    var visible = visualizerDialog.Visible;
                    if (!editorState.WorkflowRunning)
                    {
                        visualizerDialog.Hide();
                    }

                    var visualizer = visualizerDialog.Visualizer;
                    dialogSettings = CreateLayoutSettings(builder);
                    dialogSettings.Visible = visible;
                    dialogSettings.Bounds = visualizerDialog.Bounds;
                    dialogSettings.WindowState = visualizerDialog.WindowState;

                    if (visualizer.IsValueCreated)
                    {
                        var visualizerType = visualizer.Value.GetType();
                        if (visualizerType.IsPublic)
                        {
                            dialogSettings.VisualizerTypeName = visualizerType.FullName;
                            dialogSettings.VisualizerSettings = LayoutHelper.SerializeVisualizerSettings(
                                visualizer.Value,
                                topologicalOrder);
                        }
                    }
                }
                else
                {
                    dialogSettings = visualizerLayout.GetLayoutSettings(builder);
                    if (dialogSettings == null) dialogSettings = CreateLayoutSettings(builder);
                    else
                    {
                        if (ExpressionBuilder.Unwrap(builder) is IWorkflowExpressionBuilder workflowExpressionBuilder)
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
            if (validateWorkflow) EditorControl.SelectTab(this);
            UpdateSelection();
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
                case DragDropEffects.Scroll:
                    if (isContextMenuSource) Cursor = AlternateSelectionCursor;
                    else Cursor.Current = AlternateSelectionCursor;
                    break;
                case DragDropEffects.Move:
                    if (isContextMenuSource) Cursor = MoveSelectionCursor;
                    else Cursor.Current = MoveSelectionCursor;
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
            if (!CanEdit) return;
            dragKeyState = e.KeyState;
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                if (files != null && Array.TrueForAll(files, path =>
                    Path.GetExtension(path) == BonsaiExtension))
                {
                    OnDragFileDrop(e);
                }
            }
            else if (e.Data.GetDataPresent(typeof(GraphNode)))
            {
                var graphViewNode = (GraphNode)e.Data.GetData(typeof(GraphNode));
                var node = WorkflowEditor.GetGraphNodeTag(workflow, graphViewNode, false);
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

        private bool ValidateConnection(int keyState, GraphNode target)
        {
            var branch = (keyState & AltModifier) != 0;
            var shift = (keyState & ShiftModifier) != 0;
            return Editor.ValidateConnection(branch, shift, dragSelection, target);
        }

        private void graphView_DragOver(object sender, DragEventArgs e)
        {
            EnsureDragVisible(e);
            if (!CanEdit) return;
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
                    if (highlight != null && !dragSelection.Contains(highlight))
                    {
                        var link = (e.KeyState & CtrlModifier) == 0;
                        if (link)
                        {
                            e.Effect = ValidateConnection(e.KeyState, highlight)
                                ? DragDropEffects.Link
                                : DragDropEffects.None;
                        }
                        else e.Effect = DragDropEffects.Move;
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
                        var files = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                        var elements = new ExpressionBuilderGraph();
                        foreach (var path in files)
                        {
                            WorkflowBuilder workflowBuilder;
                            try { workflowBuilder = editorService.LoadWorkflow(path); }
                            catch (SystemException ex) when (ex is InvalidOperationException || ex is XmlException)
                            {
                                var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                                errorMessage = string.Format(Resources.OpenWorkflow_Error, Path.GetFileName(path), errorMessage);
                                uiService.ShowError(errorMessage);
                                return;
                            }

                            var groupBuilder = new GroupWorkflowBuilder(workflowBuilder.Workflow);
                            groupBuilder.Name = Path.GetFileNameWithoutExtension(path);
                            groupBuilder.Description = workflowBuilder.Description;
                            elements.Add(groupBuilder);
                        }
                        InsertWorkflow(elements, nodeType, branch);
                    }
                    else
                    {
                        var typeNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
                        var elementCategory = GetToolboxElementCategory(typeNode);
                        CreateGraphNode(typeNode.Text, typeNode.Name, elementCategory, nodeType, branch, group);
                    }
                }

                if (e.Effect == DragDropEffects.Move)
                {
                    var target = graphView.GetNodeAt(dropLocation);
                    Editor.MoveGraphNodes(dragSelection, target, nodeType, branch);
                }

                if (e.Effect == DragDropEffects.Link)
                {
                    if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                    {
                        var files = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                        var elements = new ExpressionBuilderGraph();
                        foreach (var path in files)
                        {
                            var includeBuilder = new IncludeWorkflowBuilder { Path = PathConvert.GetProjectPath(path) };
                            elements.Add(includeBuilder);
                        }
                        InsertWorkflow(elements, nodeType, branch);
                    }
                    else
                    {
                        var linkNode = graphView.GetNodeAt(dropLocation);
                        if (linkNode != null)
                        {
                            if (branch) Editor.ReorderGraphNodes(dragSelection, linkNode);
                            else if (shift) Editor.DisconnectGraphNodes(dragSelection, linkNode);
                            else Editor.ConnectGraphNodes(dragSelection, linkNode);
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
            if (e.Item is GraphNode selectedNode)
            {
                graphView.DoDragDrop(selectedNode, DragDropEffects.Move | DragDropEffects.Link);
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
            if (e.KeyCode == goToDefinitionToolStripMenuItem.ShortcutKeys)
            {
                LaunchDefinition(graphView.SelectedNode);
            }

            if (e.KeyCode == Keys.Return)
            {
                if (graphView.SelectedNode != null && graphView.CursorNode != graphView.SelectedNode)
                {
                    var branch = (e.Modifiers & Keys.Alt) == Keys.Alt;
                    var shift = (e.Modifiers & Keys.Shift) == Keys.Shift;
                    var control = (e.Modifiers & Keys.Control) == Keys.Control;
                    if (branch)
                    {
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }

                    if (control)
                    {
                        var nodeType = shift ? CreateGraphNodeType.Predecessor : CreateGraphNodeType.Successor;
                        Editor.MoveGraphNodes(graphView.SelectedNodes, graphView.CursorNode, nodeType, branch);
                    }
                    else if (Editor.ValidateConnection(branch, shift, graphView.SelectedNodes, graphView.CursorNode))
                    {
                        if (branch) Editor.ReorderGraphNodes(graphView.SelectedNodes, graphView.CursorNode);
                        else if (shift) Editor.DisconnectGraphNodes(graphView.SelectedNodes, graphView.CursorNode);
                        else Editor.ConnectGraphNodes(graphView.SelectedNodes, graphView.CursorNode);
                    }
                }
                else if (e.Modifiers == Keys.Control)
                {
                    LaunchDefaultEditor(graphView.SelectedNode);
                }
                else if (editorState.WorkflowRunning)
                {
                    LaunchVisualizer(graphView.SelectedNode);
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
                if (Launcher != null && Launcher.ParentView != null)
                {
                    var parentView = Launcher.ParentView;
                    var parentEditor = parentView.EditorControl;
                    var parentEditorForm = parentEditor.ParentForm;
                    if (EditorControl.ParentForm != parentEditorForm)
                    {
                        parentEditorForm.Activate();
                    }

                    var parentNode = parentView.Workflow.FirstOrDefault(node => ExpressionBuilder.Unwrap(node.Value) == Launcher.Builder);
                    if (parentNode != null)
                    {
                        parentView.SelectBuilderNode(parentNode.Value);
                    }
                }
            }

            if (CanEdit)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    Editor.DeleteGraphNodes(selectionModel.SelectedNodes);
                }

                if (e.KeyCode == Keys.D && e.Modifiers.HasFlag(Keys.Control) && selectionModel.SelectedNodes.Any())
                {
                    if (e.Modifiers.HasFlag(Keys.Shift))
                    {
                        Editor.EnableGraphNodes(selectionModel.SelectedNodes);
                    }
                    else Editor.DisableGraphNodes(selectionModel.SelectedNodes);
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
                    if (e.Modifiers.HasFlag(Keys.Shift))
                    {
                        Editor.UngroupGraphNodes(selectionModel.SelectedNodes);
                    }
                    else Editor.GroupGraphNodes(selectionModel.SelectedNodes);
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
                    var targetNode = graphView.GetNodeAt(e.Location);
                    if (targetNode != null && !dragSelection.Contains(targetNode) && ValidateConnection(dragKeyState, targetNode))
                    {
                        var branch = (dragKeyState & AltModifier) != 0;
                        var shift = (dragKeyState & ShiftModifier) != 0;
                        if (branch) Editor.ReorderGraphNodes(dragSelection, targetNode);
                        else if (shift) Editor.DisconnectGraphNodes(dragSelection, targetNode);
                        else Editor.ConnectGraphNodes(dragSelection, targetNode);
                    }
                }

                ClearDragDrop();
                Cursor = Cursors.Default;
            }
        }

        private void graphView_NodeMouseEnter(object sender, GraphNodeMouseEventArgs e)
        {
            if (dragSelection != null && e.Node != null &&
               (dragSelection.Contains(e.Node) || !ValidateConnection(dragKeyState, e.Node)))
            {
                SetDragCursor(DragDropEffects.None);
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

        private void InitializeViewBindings()
        {
            Editor.CloseWorkflowEditor.Subscribe(CloseWorkflowEditorLauncher);
            Editor.Error.Subscribe(ex => uiService.ShowError(ex));
            Editor.UpdateLayout.Subscribe(validateWorkflow =>
            {
                if (Launcher != null) Launcher.WorkflowGraphView.UpdateGraphLayout();
                else UpdateGraphLayout();
            });

            Editor.UpdateParentLayout.Subscribe(validateWorkflow =>
            {
                if (Launcher != null)
                {
                    Launcher.ParentView.UpdateGraphLayout(validateWorkflow);
                }
            });

            Editor.UpdateSelection.Subscribe(selection =>
            {
                var activeView = Launcher != null ? Launcher.WorkflowGraphView.GraphView : graphView;
                activeView.SelectedNodes = activeView.Nodes.LayeredNodes()
                    .Where(node => selection.Contains(WorkflowEditor.GetGraphNodeBuilder(node)));
            });
        }

        #endregion

        #region Context Menu

        private void defaultEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LaunchDefaultEditor(graphView.SelectedNode);
        }

        private void goToDefinitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LaunchDefinition(graphView.SelectedNode);
        }

        private void saveAsWorkflowToolStripMenuItem_Click(object sender, EventArgs e)
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
            Editor.GroupGraphNodes(selectionModel.SelectedNodes);
            contextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
        }

        private void ungroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.UngroupGraphNodes(selectionModel.SelectedNodes);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.DeleteGraphNodes(selectionModel.SelectedNodes);
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

        private void reconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitializeConnectionSource();
            dragKeyState = AltModifier;
        }

        private void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.EnableGraphNodes(selectionModel.SelectedNodes);
        }

        private void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.DisableGraphNodes(selectionModel.SelectedNodes);
        }

        private void InitializeConnectionSource()
        {
            isContextMenuSource = true;
            dragSelection = graphView.SelectedNodes.ToArray();
            SetDragCursor(DragDropEffects.Link);
        }

        private void InitializeOutputMenuItem(ToolStripMenuItem menuItem, string memberSelector, Type memberType)
        {
            var typeName = TypeHelper.GetTypeName(memberType);
            menuItem.Text += string.Format(" ({0})", typeName);
            menuItem.Name = memberSelector;
            menuItem.Tag = memberType;
        }

        //TODO: Consider refactoring this method into the core API to avoid redundancy
        static IEnumerable<PropertyInfo> GetProperties(Type type, BindingFlags bindingAttr)
        {
            var properties = type.GetProperties(bindingAttr).Except(type.GetDefaultMembers().OfType<PropertyInfo>());
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
                                      .Where(member => !member.IsDefined(typeof(ObsoleteAttribute)))
                                      .OrderBy(member => member.MetadataToken))
            {
                var memberSelector = root ? field.Name : string.Join(ExpressionHelper.MemberSeparator, ownerItem.Name, field.Name);
                var menuItem = CreateOutputMenuItem(field.Name, memberSelector, field.FieldType, selectedNode);
                ownerItem.DropDownItems.Add(menuItem);
            }

            foreach (var property in GetProperties(type, BindingFlags.Instance | BindingFlags.Public)
                                         .Where(member => !member.IsDefined(typeof(ObsoleteAttribute)))
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
            var menuItem = new ToolStripMenuItem(memberName, null, CanEdit ? delegate
            {
                var builder = new MemberSelectorBuilder { Selector = memberSelector };
                var successor = selectedNode.Successors.Select(edge => WorkflowEditor.GetGraphNodeBuilder(edge.Node)).FirstOrDefault();
                var branch = Control.ModifierKeys.HasFlag(Keys.Alt) || successor != null && successor is MemberSelectorBuilder;
                Editor.CreateGraphNode(builder, selectedNode, CreateGraphNodeType.Successor, branch);
                contextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
            } : (EventHandler)null);

            InitializeOutputMenuItem(menuItem, memberSelector, memberType);
            return menuItem;
        }

        private void CreateSubjectTypeMenuItems(InspectBuilder inspectBuilder, ToolStripMenuItem ownerItem, Type memberType, GraphNode selectedNode)
        {
            if (inspectBuilder.Builder is SubscribeSubjectBuilder subscribeBuilder)
            {
                ownerItem.Text = Resources.SubjectTypeAction;
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
                }
            }
            else if (memberType != null)
            {
                var typeName = TypeHelper.GetTypeName(memberType);
                ownerItem.Text = string.Format("{0} ({1})", Resources.CreateSourceMenuItemLabel, typeName);
                var toolboxService = (IWorkflowToolboxService)serviceProvider.GetService(typeof(IWorkflowToolboxService));
                if (toolboxService != null)
                {
                    foreach (var element in from element in toolboxService.GetToolboxElements()
                                            where element.ElementTypes.Contains(~ElementCategory.Combinator)
                                            select element)
                    {
                        ToolStripMenuItem menuItem = null;
                        var name = string.Format("{0} ({1})", element.Name, toolboxService.GetPackageDisplayName(element.Namespace));
                        menuItem = new ToolStripMenuItem(name, null, (sender, e) => CreateGraphNode(
                            element.Name,
                            element.FullyQualifiedName,
                            ~ElementCategory.Combinator,
                            CreateGraphNodeType.Successor,
                            branch: true,
                            group: true));
                        ownerItem.DropDownItems.Add(menuItem);
                    }

                    var enabled = ownerItem.DropDownItems.Count > 0;
                    ownerItem.Enabled = enabled;
                }
            }
        }

        private ToolStripMenuItem CreateSubjectTypeMenuItem(
            Type memberType,
            SubscribeSubjectBuilder subscribeSubject,
            GraphNode selectedNode)
        {
            ToolStripMenuItem menuItem = null;
            var typeName = memberType == null ? Resources.ContextMenu_NoneMenuItemLabel : TypeHelper.GetTypeName(memberType);
            menuItem = new ToolStripMenuItem(typeName, null, delegate
            {
                if (!menuItem.Checked)
                {
                    var subscribeSubjectType = memberType != null
                        ? typeof(SubscribeSubject<>).MakeGenericType(memberType)
                        : typeof(SubscribeSubjectBuilder);
                    var builder = (SubscribeSubjectBuilder)Activator.CreateInstance(subscribeSubjectType);
                    builder.Name = subscribeSubject.Name;
                    Editor.ReplaceGraphNode(selectedNode, builder);
                    contextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
                }
            });

            return menuItem;
        }

        static readonly Attribute[] ExternalizableAttributes = new Attribute[]
        {
            ExternalizableAttribute.Default,
            DesignTimeVisibleAttribute.Yes
        };

        private HashSet<string> FindMappedProperties(GraphNode node)
        {
            var mappedProperties = new HashSet<string>();
            foreach (var predecessor in workflow.Predecessors(WorkflowEditor.GetGraphNodeTag(workflow, node)))
            {
                var builder = ExpressionBuilder.Unwrap(predecessor.Value);
                if (builder is ExternalizedProperty externalizedProperty)
                {
                    mappedProperties.Add(externalizedProperty.MemberName);
                    continue;
                }

                if (builder is PropertyMappingBuilder propertyMapping)
                {
                    mappedProperties.UnionWith(propertyMapping.PropertyMappings.Select(mapping => mapping.Name));
                    continue;
                }

                if (builder is ExternalizedMappingBuilder externalizedMapping)
                {
                    mappedProperties.UnionWith(externalizedMapping.ExternalizedProperties.Select(mapping => mapping.Name));
                    continue;
                }
            }

            return mappedProperties;
        }

        private void CreateExternalizeMenuItems(object workflowElement, ToolStripMenuItem ownerItem, GraphNode selectedNode)
        {
            var mappedProperties = FindMappedProperties(selectedNode);
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(workflowElement, ExternalizableAttributes))
            {
                if (!property.IsBrowsable || property.IsReadOnly && !ExpressionHelper.IsCollectionType(property.PropertyType)) continue;
                var externalizedName = workflowElement is PropertySource propertySource ? propertySource.MemberName : null;
                var menuItem = CreateExternalizeMenuItem(property.Name, externalizedName, property.PropertyType, selectedNode);
                menuItem.Enabled = !mappedProperties.Contains(property.Name);                
                ownerItem.DropDownItems.Add(menuItem);
            }
        }

        private ToolStripMenuItem CreateExternalizeMenuItem(
            string memberName,
            string externalizedName,
            Type memberType,
            GraphNode selectedNode)
        {
            var text = string.IsNullOrEmpty(externalizedName) ? memberName : externalizedName;
            var menuItem = new ToolStripMenuItem(text, null, delegate
            {
                var mapping = new ExternalizedMapping { Name = memberName, DisplayName = externalizedName };
                var mappingNode = (from predecessor in workflow.Predecessors(WorkflowEditor.GetGraphNodeTag(workflow, selectedNode))
                                   let builder = ExpressionBuilder.Unwrap(predecessor.Value) as ExternalizedMappingBuilder
                                   where builder != null && predecessor.Successors.Count == 1
                                   select new { node = FindGraphNode(predecessor.Value), builder })
                                   .FirstOrDefault();
                if (mappingNode == null || Control.ModifierKeys.HasFlag(Keys.Alt))
                {
                    var mappingBuilder = new ExternalizedMappingBuilder { ExternalizedProperties = { mapping } };
                    Editor.CreateGraphNode(mappingBuilder, selectedNode, CreateGraphNodeType.Predecessor, branch: true);
                }
                else
                {
                    mappingNode.builder.ExternalizedProperties.Add(mapping);
                    graphView.SelectedNode = mappingNode.node;
                    editorService.ValidateWorkflow();
                }
                contextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
            });

            InitializeOutputMenuItem(menuItem, memberName, memberType);
            return menuItem;
        }

        private void CreatePropertySourceMenuItems(object workflowElement, ToolStripMenuItem ownerItem)
        {
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(workflowElement, ExternalizableAttributes))
            {
                if (property.IsReadOnly || !property.IsBrowsable) continue;
                var elementType = workflowElement.GetType();
                var memberValue = property.GetValue(workflowElement);
                if (!property.ComponentType.IsAssignableFrom(elementType)) elementType = property.ComponentType;
                var menuItem = CreatePropertySourceMenuItem(elementType, property.Name, property.PropertyType, memberValue);
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
                while (elementType.IsSubclassOf(typeof(PropertySource)) && elementType.IsGenericType)
                {
                    elementType = elementType.GetGenericArguments()[0];
                }

                var propertySourceType = typeof(PropertySource<,>).MakeGenericType(elementType, memberType);
                var propertySource = (PropertySource)Activator.CreateInstance(propertySourceType);
                var valueProperty = propertySourceType.GetProperty("Value");
                valueProperty.SetValue(propertySource, memberValue);
                propertySource.MemberName = memberName;

                Editor.CreateGraphNode(propertySource, default(GraphNode), CreateGraphNodeType.Predecessor, branch: true);
                contextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
            });

            InitializeOutputMenuItem(menuItem, memberName, memberType);
            return menuItem;
        }

        private ToolStripMenuItem CreateVisualizerMenuItem(string typeName, VisualizerDialogSettings layoutSettings, GraphNode selectedNode)
        {
            ToolStripMenuItem menuItem = null;
            var emptyVisualizer = string.IsNullOrEmpty(typeName);
            var itemText = emptyVisualizer ? Resources.ContextMenu_NoneMenuItemLabel : TypeHelper.GetTypeName(typeName);
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
                        visualizerLauncher = LayoutHelper.CreateVisualizerLauncher(
                            inspectBuilder,
                            visualizerLayout,
                            typeVisualizerMap,
                            workflow,
                            visualizerLauncher.VisualizerFactory.MashupSources,
                            workflowGraphView: this);
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
                var workflowBuilder = selectedNode != null ? WorkflowEditor.GetGraphNodeBuilder(selectedNode) as WorkflowExpressionBuilder : null;
                foreach (var element in from element in toolboxService.GetToolboxElements()
                                        where element.ElementTypes.Contains(ElementCategory.Nested)
                                        select element)
                {
                    ToolStripMenuItem menuItem = null;
                    var name = string.Format("{0} ({1})", element.Name, toolboxService.GetPackageDisplayName(element.Namespace));
                    menuItem = new ToolStripMenuItem(name, null, (sender, e) =>
                    {
                        if (menuItem.Checked && element.FullyQualifiedName != typeof(GroupWorkflowBuilder).AssemblyQualifiedName) return;
                        var replace = workflowBuilder != null && workflowBuilder.GetType().AssemblyQualifiedName != element.FullyQualifiedName;
                        if (replace) Editor.ReplaceGroupNode(selectedNode, element.FullyQualifiedName);
                        else Editor.GroupGraphNodes(selectedNodes, element.FullyQualifiedName);
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
                    else
                    {
                        groupToolStripMenuItem.DropDownItems.Add(menuItem);
                    }
                }
            }
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            // Ensure that the current view is selected
            UpdateSelection();

            editorService.OnContextMenuOpening(e);
            var selectedNodes = selectionModel.SelectedNodes.ToArray();
            if (selectedNodes.Length > 0)
            {
                copyToolStripMenuItem.Enabled = true;
                saveAsWorkflowToolStripMenuItem.Enabled = true;
            }

            if (CanEdit)
            {
                pasteToolStripMenuItem.Enabled = true;
                if (selectedNodes.Length > 0)
                {
                    cutToolStripMenuItem.Enabled = true;
                    deleteToolStripMenuItem.Enabled = true;
                    connectToolStripMenuItem.Enabled = true;
                    disconnectToolStripMenuItem.Enabled = true;
                    reconnectToolStripMenuItem.Enabled = true;
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
                    outputToolStripMenuItem.Enabled = true;
                    InitializeOutputMenuItem(outputToolStripMenuItem, string.Empty, inspectBuilder.ObservableType);
                    outputToolStripMenuItem.Tag = CreateOutputMenuItems(inspectBuilder.ObservableType, outputToolStripMenuItem, selectedNode);
                }

                var builder = WorkflowEditor.GetGraphNodeBuilder(selectedNode);
                defaultEditorToolStripMenuItem.Enabled = HasDefaultEditor(builder);
                goToDefinitionToolStripMenuItem.Enabled = HasDefinition(builder);

                var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
                if (workflowElement != null)
                {
                    if (CanEdit)
                    {
                        CreateSubjectTypeMenuItems(inspectBuilder, subjectTypeToolStripMenuItem, inspectBuilder.ObservableType, selectedNode);
                        CreateExternalizeMenuItems(workflowElement, externalizeToolStripMenuItem, selectedNode);
                        CreatePropertySourceMenuItems(workflowElement, createPropertySourceToolStripMenuItem);
                    }

                    externalizeToolStripMenuItem.Enabled = externalizeToolStripMenuItem.DropDownItems.Count > 0;
                    createPropertySourceToolStripMenuItem.Enabled = createPropertySourceToolStripMenuItem.DropDownItems.Count > 0;
                }

                var layoutSettings = visualizerLayout.GetLayoutSettings(selectedNode.Value);
                if (layoutSettings != null)
                {
                    var activeVisualizer = layoutSettings.VisualizerTypeName;
                    if (editorState.WorkflowRunning)
                    {
                        if (visualizerMapping.TryGetValue(inspectBuilder, out VisualizerDialogLauncher visualizerLauncher))
                        {
                            var visualizer = visualizerLauncher.Visualizer;
                            if (visualizer.IsValueCreated)
                            {
                                activeVisualizer = visualizer.Value.GetType().FullName;
                            }
                        }
                    }

                    var visualizerElement = ExpressionBuilder.GetVisualizerElement(inspectBuilder);
                    if (visualizerElement.ObservableType != null &&
                        (!editorState.WorkflowRunning || visualizerElement.PublishNotifications))
                    {
                        var visualizerTypes = Enumerable.Repeat<Type>(null, 1);
                        visualizerTypes = visualizerTypes.Concat(typeVisualizerMap.GetTypeVisualizers(visualizerElement));
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
            foreach (ToolStripItem item in contextMenuStrip.Items)
            {
                item.Enabled = false;
            }

            if (outputToolStripMenuItem.Tag is IDisposable outputMenuItemDisposable)
            {
                outputMenuItemDisposable.Dispose();
                outputToolStripMenuItem.Tag = null;
            }

            outputToolStripMenuItem.Text = Resources.OutputMenuItemLabel;
            subjectTypeToolStripMenuItem.Text = Resources.CreateSourceMenuItemLabel;
            outputToolStripMenuItem.DropDownItems.Clear();
            subjectTypeToolStripMenuItem.DropDownItems.Clear();
            externalizeToolStripMenuItem.DropDownItems.Clear();
            createPropertySourceToolStripMenuItem.DropDownItems.Clear();
            visualizerToolStripMenuItem.DropDownItems.Clear();
            groupToolStripMenuItem.DropDownItems.Clear();
            editorService.OnContextMenuClosed(e);
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
}
