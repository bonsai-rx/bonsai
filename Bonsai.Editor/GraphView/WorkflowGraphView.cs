using System;
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
        static readonly Cursor InvalidSelectionCursor = Cursors.No;
        static readonly Cursor MoveSelectionCursor = Cursors.SizeAll;
        static readonly Cursor AlternateSelectionCursor = Cursors.UpArrow;
        static readonly object WorkflowPathChangedEvent = new();

        const int RightMouseButton = 0x2;
        const int ShiftModifier = 0x4;
        const int CtrlModifier = 0x8;
        const int AltModifier = 0x20;
        public const Keys GroupModifier = Keys.Control;
        public const Keys BranchModifier = Keys.Alt;
        public const Keys PredecessorModifier = Keys.Shift;
        public const string BonsaiExtension = ".bonsai";

        int dragKeyState;
        bool isContextMenuSource;
        GraphNode dragHighlight;
        IEnumerable<GraphNode> dragSelection;
        readonly CommandExecutor commandExecutor;
        readonly WorkflowSelectionModel selectionModel;
        readonly IWorkflowEditorState editorState;
        readonly IWorkflowEditorService editorService;
        readonly TypeVisualizerMap typeVisualizerMap;
        readonly VisualizerLayoutMap visualizerSettings;
        readonly IServiceProvider serviceProvider;
        readonly IUIService uiService;
        readonly ThemeRenderer themeRenderer;
        readonly IDefinitionProvider definitionProvider;

        public WorkflowGraphView(IServiceProvider provider, WorkflowEditorControl owner)
        {
            EditorControl = owner ?? throw new ArgumentNullException(nameof(owner));
            serviceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            InitializeComponent();
            Editor = new WorkflowEditor(provider, graphView);
            uiService = (IUIService)provider.GetService(typeof(IUIService));
            themeRenderer = (ThemeRenderer)provider.GetService(typeof(ThemeRenderer));
            definitionProvider = (IDefinitionProvider)provider.GetService(typeof(IDefinitionProvider));
            commandExecutor = (CommandExecutor)provider.GetService(typeof(CommandExecutor));
            graphView.IconRenderer = (SvgRendererFactory)provider.GetService(typeof(SvgRendererFactory));
            selectionModel = (WorkflowSelectionModel)provider.GetService(typeof(WorkflowSelectionModel));
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
            typeVisualizerMap = (TypeVisualizerMap)provider.GetService(typeof(TypeVisualizerMap));
            visualizerSettings = (VisualizerLayoutMap)provider.GetService(typeof(VisualizerLayoutMap));
            editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));

            graphView.HandleDestroyed += graphView_HandleDestroyed;
            themeRenderer.ThemeChanged += themeRenderer_ThemeChanged;
            InitializeTheme();
            InitializeViewBindings();
        }

        internal WorkflowEditor Editor { get; }

        internal WorkflowEditorControl EditorControl { get; }

        internal bool IsReadOnly
        {
            get { return (Editor.WorkflowPathFlags & WorkflowPathFlags.ReadOnly) != 0; }
        }

        internal bool CanEdit
        {
            get { return !IsReadOnly && !editorState.WorkflowRunning; }
        }

        public GraphViewControl GraphView
        {
            get { return graphView; }
        }

        public WorkflowEditorPath WorkflowPath
        {
            get { return Editor.WorkflowPath; }
            set { Editor.NavigateTo(value); }
        }

        public event EventHandler WorkflowPathChanged
        {
            add { Events.AddHandler(WorkflowPathChangedEvent, value); }
            remove { Events.RemoveHandler(WorkflowPathChangedEvent, value); }
        }

        public ExpressionBuilderGraph Workflow
        {
            get { return Editor.Workflow; }
        }

        private void OnWorkflowPathChanged(EventArgs e)
        {
            (Events[WorkflowPathChangedEvent] as EventHandler)?.Invoke(this, e);
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

        #region Model

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
            var selection = selectionModel.SelectedNodes.SortSelection(Workflow);
            var text = ElementStore.StoreWorkflowElements(selection.ToWorkflow());
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
                    var workflow = ElementStore.RetrieveWorkflowElements(
                        Clipboard.GetText(),
                        out SemanticVersion version);
                    UpgradeHelper.TryUpgradeWorkflow(workflow, version, out workflow);
                    InsertWorkflow(workflow.ToInspectableGraph());
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
            try { Editor.CreateGraphNode(typeName, elementCategory, nodeType, branch, group, arguments); }
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
            var graphNodes = graphView.Nodes.LayeredNodes();
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
                SelectGraphNode(graphNode);
            }
        }

        internal void SelectGraphNode(GraphNode node)
        {
            GraphView.SelectedNode = node;
            EditorControl.SelectTab(this);
            GraphView.Select();
            UpdateSelection();
        }

        internal void ClearGraphNode(WorkflowEditorPath path)
        {
            SetGraphNodeHighlight(path, false, false);
        }

        internal void HighlightGraphNode(WorkflowEditorPath path, bool selectNode)
        {
            SetGraphNodeHighlight(path, selectNode, true);
        }

        private void SetGraphNodeHighlight(WorkflowEditorPath path, bool selectNode, bool highlight)
        {
            if (selectNode)
                WorkflowPath = path?.Parent;

            while (path != null)
            {
                if (path.Parent == WorkflowPath)
                {
                    var builder = Workflow[path.Index].Value;
                    var graphNode = FindGraphNode(builder);
                    if (graphNode == null)
                    {
                        throw new InvalidOperationException(Resources.ExceptionNodeNotFound_Error);
                    }

                    GraphView.Invalidate(graphNode);
                    if (selectNode) GraphView.SelectedNode = graphNode;
                    graphNode.Highlight = highlight;
                    break;
                }

                path = path.Parent;
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

        private static bool IsAnnotation(GraphNode node)
        {
            return node != null && node.IsAnnotation;
        }

        private void LaunchDefaultAction(GraphNode node)
        {
            if (!editorState.WorkflowRunning && !IsAnnotation(node) || ModifierKeys == Keys.Control)
            {
                LaunchDefaultEditor(node);
            }
            else
            {
                LaunchVisualizer(node);
            }
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
            else if (builder != null && LaunchBuilderEditor(builder))
            {
                if (!editorState.WorkflowRunning)
                {
                    editorService.ValidateWorkflow();
                }

                RefreshEditorNode(node);
            }
        }

        private bool LaunchBuilderEditor(ExpressionBuilder builder)
        {
            var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
            try
            {
                if (uiService.CanShowComponentEditor(workflowElement))
                {
                    return uiService.ShowComponentEditor(workflowElement, this);
                }

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

                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                uiService.ShowError(ex);
                return false;
            }
        }

        private void LaunchVisualizer(GraphNode node)
        {
            if (IsAnnotation(node) &&
                WorkflowEditor.GetGraphNodeBuilder(node) is AnnotationBuilder annotationBuilder)
            {
                EditorControl.AnnotationPanel.NavigateToString(annotationBuilder.Text);
                EditorControl.ExpandAnnotationPanel(annotationBuilder);
                return;
            }

            var builder = (InspectBuilder)Workflow[node.Index].Value;
            var visualizerDialogs = (VisualizerDialogMap)serviceProvider.GetService(typeof(VisualizerDialogMap));
            if (visualizerDialogs != null)
            {
                if (!visualizerDialogs.TryGetValue(builder, out VisualizerDialogLauncher visualizerLauncher))
                {
                    visualizerSettings.TryGetValue(builder, out VisualizerDialogSettings dialogSettings);
                    visualizerLauncher = visualizerDialogs.Add(builder, Workflow, dialogSettings);
                }

                var ownerWindow = uiService.GetDialogOwnerWindow();
                visualizerLauncher.Show(ownerWindow, serviceProvider);
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
            var builder = WorkflowEditor.GetGraphNodeBuilder(node);
            var disableBuilder = builder as DisableBuilder;
            var workflowExpressionBuilder = (disableBuilder != null ? disableBuilder.Builder : builder) as IWorkflowExpressionBuilder;
            if (workflowExpressionBuilder != null)
            {
                var newPath = new WorkflowEditorPath(node.Index, WorkflowPath);
                LaunchWorkflowPath(newPath);
            }
        }

        private void LaunchWorkflowPath(WorkflowEditorPath path)
        {
            Editor.NavigateTo(path);
        }

        internal void UpdateSelection()
        {
            UpdateSelection(forceUpdate: false);
        }

        internal void UpdateSelection(bool forceUpdate)
        {
            if (forceUpdate ||
                selectionModel.SelectedView != this ||
                selectionModel.SelectedNodes != GraphView.SelectedNodes)
            {
                selectionModel.UpdateSelection(this);
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
            var builder = WorkflowEditor.GetGraphNodeBuilder(node);
            if (builder is AnnotationBuilder annotationBuilder &&
                EditorControl.AnnotationPanel.Tag == annotationBuilder)
            {
                LaunchVisualizer(node);
            }
        }

        private void UpdateGraphLayout()
        {
            UpdateGraphLayout(validateWorkflow: true);
        }

        private void UpdateGraphLayout(bool validateWorkflow)
        {
            graphView.Nodes = Workflow.ConnectedComponentLayering();
            graphView.Invalidate();
            if (validateWorkflow)
            {
                editorService.ValidateWorkflow();
            }

            if (validateWorkflow)
            {
                EditorControl.SelectTab(this);
                if (EditorControl.AnnotationPanel.Tag is ExpressionBuilder builder)
                {
                    var workflowBuilder = (WorkflowBuilder)serviceProvider.GetService(typeof(WorkflowBuilder));
                    if (!workflowBuilder.Workflow.Descendants().Contains(builder))
                    {
                        EditorControl.AnnotationPanel.NavigateToString(string.Empty);
                        EditorControl.AnnotationPanel.Tag = null;
                    }
                }
            }
            UpdateSelection();
            editorService.RefreshEditor();
        }

        private void InvalidateGraphLayout(bool validateWorkflow)
        {
            graphView.Refresh();
            if (validateWorkflow)
            {
                editorService.ValidateWorkflow();
            }
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
                var node = WorkflowEditor.GetGraphNodeTag(Workflow, graphViewNode, false);
                if (node != null && Workflow.Contains(node))
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
                        InsertWorkflow(elements.ToInspectableGraph(recurse: false), nodeType, branch);
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

            if (e.KeyCode == Keys.Return && !CanEdit)
            {
                LaunchDefaultAction(graphView.SelectedNode);
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
                LaunchWorkflowPath(WorkflowPath?.Parent);
            }

            if (CanEdit)
            {
                if (e.KeyCode == Keys.Return)
                {
                    if (graphView.SelectedNode != null && !graphView.SelectedNodes.Contains(graphView.CursorNode))
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
                    else LaunchDefaultAction(graphView.SelectedNode);
                }

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
            LaunchDefaultAction(e.Node);
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
            themeRenderer.ThemeChanged -= themeRenderer_ThemeChanged;
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
            Editor.Error.Subscribe(uiService.ShowError);
            Editor.UpdateLayout.Subscribe(UpdateGraphLayout);
            Editor.InvalidateLayout.Subscribe(InvalidateGraphLayout);
            Editor.WorkflowPathChanged.Subscribe(path =>
            {
                UpdateSelection(forceUpdate: true);
                graphView.PathFlags = Editor.WorkflowPathFlags;
                OnWorkflowPathChanged(EventArgs.Empty);
            });

            Editor.UpdateSelection.Subscribe(selection =>
            {
                var activeView = graphView;
                activeView.SelectedNodes = activeView.Nodes.LayeredNodes()
                    .Where(node =>
                    {
                        var nodeBuilder = WorkflowEditor.GetGraphNodeBuilder(node);
                        return selection.Any(builder => ExpressionBuilder.Unwrap(builder) == nodeBuilder);
                    });
            });
            Editor.ResetNavigation();
        }

        #endregion

        #region Context Menu

        private void defaultEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LaunchDefaultEditor(graphView.SelectedNode);
        }

        private void docsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorService.OnKeyDown(new KeyEventArgs(Keys.F1));
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

        static bool IsBrowsableMember(MemberInfo member)
        {
            return !member.IsDefined(typeof(ObsoleteAttribute)) &&
                   !member.GetCustomAttributes<BrowsableAttribute>().Contains(BrowsableAttribute.No);
        }

        private IDisposable CreateOutputMenuItems(Type type, ToolStripMenuItem ownerItem, GraphNode selectedNode)
        {
            if (type.IsEnum) return Disposable.Empty;
            var root = string.IsNullOrEmpty(ownerItem.Name);

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                                      .Where(IsBrowsableMember)
                                      .OrderBy(member => member.MetadataToken))
            {
                var memberSelector = root ? field.Name : string.Join(ExpressionHelper.MemberSeparator, ownerItem.Name, field.Name);
                var menuItem = CreateOutputMenuItem(field.Name, memberSelector, field.FieldType, selectedNode);
                ownerItem.DropDownItems.Add(menuItem);
            }

            foreach (var property in GetProperties(type, BindingFlags.Instance | BindingFlags.Public)
                                         .Where(IsBrowsableMember)
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
            if (inspectBuilder.Builder is SubscribeSubject subscribeBuilder)
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
                            ModifierKeys.HasFlag(Keys.Shift)
                                ? CreateGraphNodeType.Predecessor
                                : CreateGraphNodeType.Successor,
                            branch: false,
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
            SubscribeSubject subscribeSubject,
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
                        : typeof(SubscribeSubject);
                    var builder = (SubscribeSubject)Activator.CreateInstance(subscribeSubjectType);
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
            foreach (var predecessor in Workflow.Predecessors(WorkflowEditor.GetGraphNodeTag(Workflow, node)))
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
                var mappingNode = (from predecessor in Workflow.Predecessors(WorkflowEditor.GetGraphNodeTag(Workflow, selectedNode))
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

                Editor.CreateGraphNode(propertySource, default(GraphNode), CreateGraphNodeType.Successor, branch: true);
                contextMenuStrip.Close(ToolStripDropDownCloseReason.ItemClicked);
            });

            InitializeOutputMenuItem(menuItem, memberName, memberType);
            return menuItem;
        }

        private ToolStripMenuItem CreateVisualizerMenuItem(string typeName, GraphNode selectedNode)
        {
            ToolStripMenuItem menuItem = null;
            var emptyVisualizer = string.IsNullOrEmpty(typeName);
            var itemText = emptyVisualizer ? Resources.ContextMenu_NoneMenuItemLabel : TypeHelper.GetTypeName(typeName);
            menuItem = new ToolStripMenuItem(itemText, null, delegate
            {
                var inspectBuilder = (InspectBuilder)selectedNode.Value;
                if (ExpressionBuilder.Unwrap(inspectBuilder) is VisualizerMappingBuilder mappingBuilder)
                {
                    if (emptyVisualizer) mappingBuilder.VisualizerType = null;
                    else
                    {
                        var visualizerType = typeVisualizerMap.GetVisualizerType(typeName);
                        var mappingType = typeof(TypeMapping<>).MakeGenericType(visualizerType);
                        mappingBuilder.VisualizerType = (TypeMapping)Activator.CreateInstance(mappingType);
                    }
                    UpdateSelection(forceUpdate: true);
                }
                else if (!menuItem.Checked)
                {
                    var dialogSettings = emptyVisualizer ? default : new VisualizerDialogSettings
                    {
                        Tag = inspectBuilder,
                        VisualizerTypeName = typeName,
                        Visible = true,
                        Bounds = Rectangle.Empty
                    };

                    if (editorState.WorkflowRunning)
                    {
                        var visualizerDialogs = (VisualizerDialogMap)serviceProvider.GetService(typeof(VisualizerDialogMap));
                        if (visualizerDialogs.TryGetValue(inspectBuilder, out VisualizerDialogLauncher visualizerDialog))
                        {
                            visualizerDialog.Hide();
                            visualizerDialogs.Remove(visualizerDialog);
                        }

                        if (!emptyVisualizer)
                        {
                            var dialogLauncher = visualizerDialogs.Add(inspectBuilder, Workflow, dialogSettings);
                            var ownerWindow = uiService.GetDialogOwnerWindow();
                            visualizerDialog.Show(ownerWindow, serviceProvider);
                        }
                    }
                    else
                    {
                        if (emptyVisualizer)
                            visualizerSettings.Remove(inspectBuilder);
                        else
                            visualizerSettings[inspectBuilder] = dialogSettings;
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
                                        where element.ElementTypes.Contains(ElementCategory.Nested) &&
                                              element.FullyQualifiedName.Contains(typeof(WorkflowExpressionBuilder).Assembly.FullName)
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
                docsToolStripMenuItem.Enabled = true;

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

                var activeVisualizer = visualizerSettings.TryGetValue(inspectBuilder, out var dialogSettings)
                    ? dialogSettings.VisualizerTypeName
                    : null;

                if (workflowElement is VisualizerMappingBuilder mappingBuilder &&
                    mappingBuilder.VisualizerType != null)
                {
                    activeVisualizer = mappingBuilder.VisualizerType.GetType().GetGenericArguments()[0].FullName;
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
                        var typeName = type?.FullName ?? string.Empty;
                        var menuItem = CreateVisualizerMenuItem(typeName, selectedNode);
                        visualizerToolStripMenuItem.DropDownItems.Add(menuItem);
                        menuItem.Checked = type is null
                            ? activeVisualizer is null
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
