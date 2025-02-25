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

        public WorkflowEditorControl(IServiceProvider provider)
        {
            InitializeComponent();
            serviceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
            themeRenderer = (ThemeRenderer)provider.GetService(typeof(ThemeRenderer));
            commandExecutor = (CommandExecutor)provider.GetService(typeof(CommandExecutor));
            ConfigureDockTheme(lightTheme);
            ConfigureDockTheme(darkTheme);
            annotationPanel.ThemeRenderer = themeRenderer;
            annotationPanel.LinkClicked += (sender, e) => { EditorDialog.OpenUrl(e.LinkText); };
            annotationPanel.CloseRequested += delegate { CollapseAnnotationPanel(); };
            if (EditorSettings.IsRunningOnMono)
                CreateDockContent(null);
            InitializeTheme();
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

        public void SaveEditorLayout(string fileName)
        {
            var document = dockPanel.SaveAsXml();
            document.DescendantNodes().OfType<XComment>().Remove();
            using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            using var writer = XmlWriter.Create(fileStream, new XmlWriterSettings { Indent = true });
            document.WriteTo(writer);
        }

        public void ResetEditorLayout(string fileName = default)
        {
            CloseAll();
            if (File.Exists(fileName))
            {
                var workflowBuilder = (WorkflowBuilder)serviceProvider.GetService(typeof(WorkflowBuilder));
                using var stream = File.OpenRead(fileName);
                dockPanel.LoadFromXml(stream, content =>
                {
                    try { return DockPanelSerializer.DeserializeContent(this, workflowBuilder, content); }
                    catch { return new InvalidDockContent(); } // best effort
                });
                CloseInvalidContents();
            }
            else
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
            UpdateGraphView(graphView => graphView.HighlightGraphNode(path, selectNode && graphView == selectedView));
        }

        internal void RefreshSelection(WorkflowGraphView selectedView)
        {
            UpdateGraphView(selectedView, graphView => graphView.InvalidateGraphLayout(validateWorkflow: false));
            selectedView.RefreshSelection();
            UpdateAllText();
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

        void CloseAll()
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
                if (contents[i] is WorkflowDockContent workflowContent)
                    workflowContent.Close();
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
            theme.Extender.DockPaneStripFactory = new WorkflowDockPaneStripFactory();
            theme.Measures.DockPadding = 0;
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
                dockPanel.Theme = dockTheme;
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
    }
}
