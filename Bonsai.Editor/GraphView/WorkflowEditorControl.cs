using System;
using System.Drawing;
using System.Windows.Forms;
using Bonsai.Expressions;
using Bonsai.Editor.Docking;
using Bonsai.Editor.Themes;
using Bonsai.Editor.GraphModel;
using WeifenLuo.WinFormsUI.Docking;
using Bonsai.Design;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Bonsai.Editor.GraphView
{
    partial class WorkflowEditorControl : UserControl
    {
        readonly IServiceProvider serviceProvider;
        readonly IWorkflowEditorService editorService;
        readonly ThemeRenderer themeRenderer;
        readonly CommandExecutor commandExecutor;
        readonly WatchToolWindow watchToolWindow;
        readonly WorkflowFindToolWindow findToolWindow;
        readonly EditorToolWindowCollection toolWindows;

        public WorkflowEditorControl(IServiceProvider provider)
        {
            InitializeComponent();
            serviceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
            themeRenderer = (ThemeRenderer)provider.GetService(typeof(ThemeRenderer));
            commandExecutor = (CommandExecutor)provider.GetService(typeof(CommandExecutor));
            watchToolWindow = new WatchToolWindow(provider, this);
            watchToolWindow.Navigate += ToolWindow_Navigate;
            findToolWindow = new WorkflowFindToolWindow(provider);
            findToolWindow.Navigate += ToolWindow_Navigate;
            toolWindows = new EditorToolWindowCollection { findToolWindow, watchToolWindow };
            AssignToolWindows();

            ConfigureDockTheme(lightTheme);
            ConfigureDockTheme(darkTheme);
            annotationPanel.ThemeRenderer = themeRenderer;
            annotationPanel.LinkClicked += (sender, e) => { EditorDialog.OpenUrl(e.LinkText); };
            annotationPanel.CloseRequested += delegate { CollapseAnnotationPanel(); };
            if (EditorSettings.IsRunningOnMono)
                CreateDockContent(null);
            InitializeTheme();
        }

        public EditorToolWindowCollection ToolWindows
        {
            get { return toolWindows; }
        }

        public event EventHandler ActiveContentChanged
        {
            add { dockPanel.ActiveContentChanged += value; }
            remove { dockPanel.ActiveContentChanged -= value; }
        }

        public WorkflowGraphView ActiveContent
        {
            get
            {
                return dockPanel.ActiveContent is WorkflowDockContent workflowContent
                    ? workflowContent.WorkflowGraphView
                    : null;
            }
        }

        public AnnotationPanel AnnotationPanel
        {
            get { return annotationPanel; }
        }

        public bool AnnotationCollapsed
        {
            get { return splitContainer.Panel1Collapsed; }
        }

        public int AnnotationPanelSize
        {
            get { return splitContainer.SplitterDistance; }
            set
            {
                splitContainer.SplitterDistance = value;
                splitContainer.Panel1MinSize = value / 2;
            }
        }

        public void ExpandAnnotationPanel(ExpressionBuilder builder)
        {
            annotationPanel.Tag = builder;
            ExpandAnnotationPanel(ElementHelper.GetElementName(builder));
        }

        public void ExpandAnnotationPanel(string label)
        {
            browserLabel.Text = label;
            splitContainer.Panel1Collapsed = false;
            EnsureWebViewSize();
        }

        public void CollapseAnnotationPanel()
        {
            splitContainer.Panel1Collapsed = true;
            ClearAnnotationPanel();
        }

        public void ClearAnnotationPanel()
        {
            annotationPanel.NavigateToString(string.Empty);
            annotationPanel.Tag = null;
        }

        public WorkflowDockContent CreateDockContent(WorkflowEditorPath workflowPath, DockState dockState = DockState.Document)
        {
            var currentPath = workflowPath;
            var editor = new WorkflowEditor(serviceProvider);
            return dockPanel.CreateDynamicContent(
                panel => CreateWorkflowDockContent(currentPath, editor),
                content => currentPath = content.WorkflowGraphView.WorkflowPath,
                dockState,
                commandExecutor);
        }

        private WorkflowDockContent CreateWorkflowDockContent(WorkflowEditorPath workflowPath, WorkflowEditor editor)
        {
            var workflowGraphView = new WorkflowGraphView(serviceProvider, this, editor);
            var dockContent = new WorkflowDockContent(workflowGraphView, serviceProvider);
            dockContent.DockAreas = DockAreas.Float | DockAreas.Document;
            dockContent.SuspendLayout();

            workflowGraphView.BackColorChanged += (sender, e) =>
            {
                dockContent.BackColor = workflowGraphView.BackColor;
            };
            workflowGraphView.Margin = new Padding(0);
            workflowGraphView.Dock = DockStyle.Fill;
            workflowGraphView.Font = Font;
            workflowGraphView.Tag = dockContent;

            var breadcrumbs = new WorkflowPathNavigationControl(serviceProvider);
            breadcrumbs.Margin = new Padding(0);
            breadcrumbs.WorkflowPath = null;
            breadcrumbs.WorkflowPathMouseClick += (sender, e) => workflowGraphView.WorkflowPath = e.Path;
            workflowGraphView.WorkflowPathChanged += (sender, e) =>
            {
                breadcrumbs.WorkflowPath = workflowGraphView.WorkflowPath;
                dockContent.UpdateText();
            };

            var navigationPanel = new TableLayoutPanel();
            navigationPanel.Dock = DockStyle.Fill;
            navigationPanel.ColumnCount = 1;
            navigationPanel.RowCount = 2;
            navigationPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, breadcrumbs.Height));
            navigationPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            navigationPanel.Controls.Add(breadcrumbs);
            navigationPanel.Controls.Add(workflowGraphView);
            workflowGraphView.TabIndex = 0;
            breadcrumbs.TabIndex = 1;

            // TODO: This should be handled by docking, but some strange interaction prevents shrinking to min size
            navigationPanel.Layout += (sender, e) => breadcrumbs.Width = navigationPanel.Width;
            breadcrumbs.Width = navigationPanel.Width;
            workflowGraphView.Editor.ResetNavigation(workflowPath);

            dockContent.Controls.Add(navigationPanel);
            dockContent.BackColor = workflowGraphView.BackColor;
            dockContent.ResumeLayout(false);
            dockContent.PerformLayout();
            return dockContent;
        }

        public void CloseDockContent(IWorkflowExpressionBuilder workflowBuilder)
        {
            if (workflowBuilder is null)
            {
                throw new ArgumentNullException(nameof(workflowBuilder));
            }

            if (workflowBuilder.Workflow is null)
                return;

            for (int i = dockPanel.Contents.Count - 1; i >= 0; i--)
            {
                if (dockPanel.Contents[i] is WorkflowDockContent workflowContent &&
                    workflowContent.WorkflowGraphView.Workflow == workflowBuilder.Workflow)
                {
                    workflowContent.Close();
                }
            }

            foreach (var node in workflowBuilder.Workflow)
            {
                if (ExpressionBuilder.GetWorkflowElement(node.Value) is IWorkflowExpressionBuilder nestedBuilder)
                {
                    CloseDockContent(nestedBuilder);
                }
            }
        }

        public void SelectTab(WorkflowGraphView workflowGraphView)
        {
            foreach (var content in dockPanel.Contents)
            {
                if (content is WorkflowDockContent workflowContent &&
                    workflowContent.WorkflowGraphView == workflowGraphView)
                {
                    workflowContent.Activate();
                }
            }
        }

        public void ShowFindResults()
        {
            ActivateToolWindow(findToolWindow);
        }

        public void ShowFindResults(string text, IEnumerable<WorkflowQueryResult> query)
        {
            findToolWindow.Query = query;
            findToolWindow.Text = text;
            ShowFindResults();
        }

        public void UpdateWatchTool(IEnumerable<InspectBuilder> selectedItems = default)
        {
            watchToolWindow.UpdateWatchList(selectedItems);
        }

        public void ShowWatchTool(IEnumerable<InspectBuilder> selectedItems = default)
        {
            UpdateWatchTool(selectedItems);
            ActivateToolWindow(watchToolWindow);
        }

        private void ActivateToolWindow(EditorToolWindow toolWindow)
        {
            if (toolWindow.DockState == DockState.Hidden)
                toolWindow.Show(dockPanel);
            else if (toolWindow.DockState == DockState.Unknown)
                toolWindow.Show(dockPanel, DockState.DockBottom);
            else if (toolWindow.DockState >= DockState.DockTopAutoHide &&
                     toolWindow.DockState <= DockState.DockRightAutoHide)
                dockPanel.ActiveAutoHideContent = toolWindow;
            else
                toolWindow.Activate();
        }

        private XElement PersistDockingLayout()
        {
            var element = dockPanel.SaveAsXml();
            element.DescendantNodes().OfType<XComment>().Remove();
            return element;
        }

        private void RestoreDockingLayout(XElement element)
        {
            using var memoryStream = new MemoryStream();
            element.Save(memoryStream);
            memoryStream.Position = 0;

            var workflowBuilder = (WorkflowBuilder)serviceProvider.GetService(typeof(WorkflowBuilder));
            dockPanel.LoadFromXml(memoryStream, content =>
            {
                try { return DockPanelSerializer.DeserializeContent(this, workflowBuilder, content); }
                catch { return new InvalidDockContent(); } // best effort
            });
        }

        private void LoadEditorLayout(string fileName)
        {
            using var stream = File.OpenRead(fileName);
            var editorSettings = (WorkflowEditorSettings)WorkflowEditorSettings.Serializer.Deserialize(stream);
            RestoreDockingLayout(editorSettings.DockPanel);

            if (editorSettings.WatchSettings is not null)
            {
                var watchMap = (WorkflowWatchMap)serviceProvider.GetService(typeof(WorkflowWatchMap));
                var workflowBuilder = (WorkflowBuilder)serviceProvider.GetService(typeof(WorkflowBuilder));
                watchMap.SetWatchSettings(workflowBuilder, editorSettings.WatchSettings);
                UpdateWatchTool();
                UpdateWatchLayout();
            }
        }

        public void SaveEditorLayout(string fileName)
        {
            var editorSettings = new WorkflowEditorSettings();
            editorSettings.Version = AboutBox.AssemblyVersion;
            editorSettings.DockPanel = PersistDockingLayout();

            var watchMap = (WorkflowWatchMap)serviceProvider.GetService(typeof(WorkflowWatchMap));
            var workflowBuilder = (WorkflowBuilder)serviceProvider.GetService(typeof(WorkflowBuilder));
            if (watchMap.Count > 0)
                editorSettings.WatchSettings = watchMap.GetWatchSettings(workflowBuilder);

            ElementStore.SaveElement(WorkflowEditorSettings.Serializer, fileName, editorSettings, ElementStore.EmptyNamespaces);
        }

        public void InitializeEditorLayout(string fileName = default)
        {
            if (File.Exists(fileName))
            {
                CloseToolWindows();
                LoadEditorLayout(fileName);
                CloseInvalidContents();
                AssignToolWindows();
            }

            if (!dockPanel.Documents.Any())
            {
                CreateDockContent(null);
            }
        }

        void CloseInvalidContents()
        {
            for (int i = dockPanel.Contents.Count - 1; i >= 0; i--)
            {
                if (dockPanel.Contents[i] is InvalidDockContent invalidContent)
                    invalidContent.Close();
            }
        }

        internal void ClearGraphNode(WorkflowEditorPath path)
        {
            UpdateGraphView(graphView => graphView.ClearGraphNode(path));
        }

        internal void HighlightGraphNode(WorkflowGraphView selectedView, WorkflowEditorPath path, bool selectNode)
        {
            UpdateGraphView(selectedView, graphView => graphView.HighlightGraphNode(path, selectNode: false));
            selectedView?.HighlightGraphNode(path, selectNode);
        }

        internal void RefreshSelection(WorkflowGraphView selectedView)
        {
            UpdateGraphView(selectedView, graphView => graphView.InvalidateGraphLayout(validateWorkflow: false));
            selectedView.RefreshSelection();
            UpdateAllText();
        }

        internal void UpdateWatchLayout()
        {
            UpdateGraphView(graphView => graphView.UpdateWatchLayout());
        }

        internal void UpdateWatchLayout(WorkflowEditorPath path)
        {
            UpdateWatchLayout(new[] { path });
        }

        internal void UpdateWatchLayout(IEnumerable<WorkflowEditorPath> editorPaths)
        {
            UpdateGraphView(graphView =>
            {
                if (editorPaths.Contains(graphView.WorkflowPath))
                    graphView.UpdateWatchLayout();
            });
        }

        internal void InvalidateGraphLayout(WorkflowGraphView selectedView, bool validateWorkflow)
        {
            UpdateGraphView(selectedView, graphView => graphView.InvalidateGraphLayout(validateWorkflow: false));
            selectedView.InvalidateGraphLayout(validateWorkflow);
        }

        internal void UpdateGraphLayout(WorkflowGraphView selectedView, bool validateWorkflow)
        {
            UpdateGraphView(selectedView, graphView =>
                graphView.UpdateGraphLayout(validateWorkflow: false, updateSelection: false));
            selectedView.UpdateGraphLayout(validateWorkflow, updateSelection: true);
        }

        private void UpdateGraphView(Action<WorkflowGraphView> action)
        {
            UpdateGraphView(selectedView: null, action);
        }

        private void UpdateGraphView(WorkflowGraphView selectedView, Action<WorkflowGraphView> action)
        {
            for (int i = dockPanel.Contents.Count - 1; i >= 0; i--)
            {
                if (dockPanel.Contents[i] is WorkflowDockContent workflowContent &&
                    workflowContent.WorkflowGraphView != selectedView &&
                    !workflowContent.WorkflowGraphView.IsDisposed &&
                    (selectedView is null ||
                     workflowContent.WorkflowGraphView.WorkflowPath == selectedView.WorkflowPath))
                {
                    action(workflowContent.WorkflowGraphView);
                }
            }
        }

        internal void UpdateFindResults()
        {
            for (int i = dockPanel.Contents.Count - 1; i >= 0; i--)
            {
                if (dockPanel.Contents[i] is WorkflowFindToolWindow findToolWindow &&
                    findToolWindow.DockState != DockState.Hidden)
                {
                    findToolWindow.UpdateResults();
                }
            }
        }

        void UpdateAllText()
        {
            for (int i = dockPanel.Contents.Count - 1; i >= 0; i--)
            {
                if (dockPanel.Contents[i] is WorkflowDockContent workflowContent &&
                    !workflowContent.WorkflowGraphView.IsDisposed)
                {
                    workflowContent.UpdateText();
                }
            }
        }

        public void CloseAll()
        {
            var contents = new List<IDockContent>(dockPanel.Contents.Count);
            IEnumerable<INestedPanesContainer> paneContainers = dockPanel.DockWindows;
            foreach (var dockWindow in paneContainers.Concat(dockPanel.FloatWindows))
            {
                foreach (var nestedPane in dockWindow.NestedPanes)
                {
                    WorkflowDockContent firstContent = null;
                    foreach (var content in nestedPane.Contents)
                    {
                        if (content.DockHandler.Pane != nestedPane)
                            continue;

                        if (firstContent is null && content is WorkflowDockContent dockContent)
                        {
                            firstContent = dockContent;
                            nestedPane.Tag = dockContent.Tag;
                        }

                        contents.Add(content);
                    }
                }
            }

            for (int i = contents.Count - 1; i >= 0; i--)
            {
                if (contents[i] is DockContent dockContent && !dockContent.HideOnClose)
                    dockContent.Close();
            }

            var floatWindows = new FloatWindow[dockPanel.FloatWindows.Count];
            dockPanel.FloatWindows.CopyTo(floatWindows, 0);
            for (int i = 0; i < floatWindows.Length; i++)
            {
                if (floatWindows[i].NestedPanes.All(nestedPane => nestedPane.Contents.Count == 0))
                    floatWindows[i].Close();
            }
        }

        private void AssignToolWindows()
        {
            foreach (var toolWindow in toolWindows)
            {
                toolWindow.DockPanel ??= dockPanel;
            }
        }

        private void CloseToolWindows()
        {
            var contents = new IDockContent[dockPanel.Contents.Count];
            dockPanel.Contents.CopyTo(contents, 0);
            for (int i = contents.Length - 1; i >= 0; i--)
            {
                contents[i].DockHandler.DockPanel = null;
            }

            var floatWindows = new FloatWindow[dockPanel.FloatWindows.Count];
            dockPanel.FloatWindows.CopyTo(floatWindows, 0);
            for (int i = 0; i < floatWindows.Length; i++)
            {
                floatWindows[i].Close();
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            foreach (var content in dockPanel.Contents)
            {
                if (content is WorkflowDockContent workflowContent)
                {
                    workflowContent.WorkflowGraphView.Font = Font;
                }
            }

            UpdateDockThemeFont(lightTheme);
            UpdateDockThemeFont(darkTheme);
            base.OnFontChanged(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            EnsureWebViewSize();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            editorService.OnKeyDown(e);
            base.OnKeyDown(e);
        }

        protected override void OnEnter(EventArgs e)
        {
            if (dockPanel.GetDocumentPane()?.ActiveContent is WorkflowDockContent workflowContent)
            {
                workflowContent.Activate();
            }
            base.OnEnter(e);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            AnnotationPanelSize = (int)Math.Round(splitContainer.SplitterDistance * factor.Width);
            ScaleDockTheme(lightTheme, factor);
            ScaleDockTheme(darkTheme, factor);
            splitContainer.FixedPanel = FixedPanel.Panel1;
            dockPanel.DefaultFloatWindowSize = Size.Round(new SizeF(
                width: 320 * factor.Width,
                height: 240 * factor.Height
            ));
        }

        private void EnsureWebViewSize()
        {
            if (splitContainer.FixedPanel != FixedPanel.None)
            {
                if (Width < 4 * splitContainer.Panel1MinSize)
                {
                    splitContainer.SplitterDistance = Width / 2;
                }
                else
                {
                    splitContainer.SplitterDistance = Math.Max(
                        2 * splitContainer.Panel1MinSize - splitContainer.SplitterWidth,
                        splitContainer.SplitterDistance);
                }
            }
        }

        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (IsHandleCreated)
            {
                var delta = PointToClient(MousePosition).X - e.X;
                if (delta == 0)
                {
                    AnnotationPanelSize = e.SplitX;
                }
            }
        }

        private void ConfigureDockTheme(ThemeBase theme)
        {
            theme.Extender.FloatWindowFactory = new WorkflowFloatWindowFactory(serviceProvider);
            theme.Extender.DockPaneCaptionFactory = new WorkflowDockPaneCaptionFactory();
            theme.Extender.DockPaneStripFactory = new WorkflowDockPaneStripFactory();
            theme.Measures.DockPadding = 0;
        }

        private void ScaleDockTheme(ThemeBase theme, SizeF factor)
        {
            theme.Measures.SplitterSize = (int)Math.Round(6 * factor.Height);
            theme.Measures.AutoHideSplitterSize = (int)Math.Round(3 * factor.Height);
        }

        private void UpdateDockThemeFont(ThemeBase theme)
        {
            theme.Skin.DockPaneStripSkin.TextFont = Font;
            theme.Skin.AutoHideStripSkin.TextFont = Font;
        }

        internal void InitializeTheme()
        {
            browserLayoutPanel.RowStyles[0].Height = themeRenderer.LabelHeight;

            var colorTable = themeRenderer.ToolStripRenderer.ColorTable;
            browserLabel.BackColor = closeBrowserButton.BackColor = colorTable.SeparatorDark;
            browserLabel.ForeColor = closeBrowserButton.ForeColor = colorTable.ControlForeColor;
            annotationPanel.InitializeTheme();

            ThemeBase dockTheme = themeRenderer.ActiveTheme == ColorTheme.Light ? lightTheme : darkTheme;
            if (dockPanel.Theme != dockTheme)
            {
                var restoreContents = dockPanel.Contents.Count > 0;
                if (restoreContents)
                {
                    commandExecutor.BeginCompositeCommand();
                    CloseAll();
                    commandExecutor.EndCompositeCommand();
                }

                var toolWindowLayout = PersistDockingLayout();
                CloseToolWindows();
                dockPanel.Theme = dockTheme;
                RestoreDockingLayout(toolWindowLayout);
                if (restoreContents) commandExecutor.Undo();
            }
        }

        private void annotationPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.F4: CollapseAnnotationPanel(); break;
                    case Keys.Back:
                        e.Handled = true;
                        if (dockPanel.ActiveContent is WorkflowDockContent workflowContent)
                        {
                            workflowContent.WorkflowGraphView.Focus();
                        }
                        break;
                }
            }
        }

        private void closeBrowserButton_Click(object sender, EventArgs e)
        {
            CollapseAnnotationPanel();
        }

        private void ToolWindow_Navigate(object sender, WorkflowNavigateEventArgs e)
        {
            editorService.SelectBuilderNode(e.WorkflowPath, e.NavigationPreference);
        }
    }
}
