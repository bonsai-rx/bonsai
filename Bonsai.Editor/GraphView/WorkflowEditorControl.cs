using System;
using System.Drawing;
using System.Windows.Forms;
using Bonsai.Expressions;
using Bonsai.Editor.Themes;
using Bonsai.Editor.GraphModel;
using WeifenLuo.WinFormsUI.Docking;
using Bonsai.Design;

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
            annotationPanel.ThemeRenderer = themeRenderer;
            annotationPanel.LinkClicked += (sender, e) => { EditorDialog.OpenUrl(e.LinkText); };
            annotationPanel.CloseRequested += delegate { CollapseAnnotationPanel(); };
            CreateDockContent(null);
            InitializeTheme();
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

        public Rectangle ContentArea
        {
            get
            {
                var dockArea = dockPanel.DockArea;
                if (dockPanel.Panes.Count > 0)
                {
                    var tabHeight = dockPanel.Panes[0].TabStripControl.Height;
                    dockArea.Y += tabHeight;
                    dockArea.Height -= tabHeight;
                }

                return dockArea;
            }
        }

        public void CreateDockContent(WorkflowEditorPath workflowPath, DockState dockState = DockState.Document)
        {
            var editor = new WorkflowEditor(serviceProvider);
            dockPanel.CreateDynamicContent(
                panel => CreateWorkflowDockContent(workflowPath, editor),
                dockState,
                commandExecutor);
        }

        private WorkflowDockContent CreateWorkflowDockContent(WorkflowEditorPath workflowPath, WorkflowEditor editor)
        {
            var workflowGraphView = new WorkflowGraphView(serviceProvider, this, editor);
            var dockContent = new WorkflowDockContent(workflowGraphView);
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

        public void ResetNavigation()
        {
            CloseAll();
            CreateDockContent(null);
        }

        void CloseAll()
        {
            for (int i = dockPanel.Contents.Count - 1; i >= 0; i--)
            {
                if (dockPanel.Contents[i] is WorkflowDockContent workflowContent)
                {
                    workflowContent.Close();
                }
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

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            AnnotationPanelSize = (int)Math.Round(splitContainer.SplitterDistance * factor.Width);
            splitContainer.FixedPanel = FixedPanel.Panel1;
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

        private void InitializeTheme()
        {
            var labelOffset = browserLabel.Height - ContentArea.Top;
            if (themeRenderer.ActiveTheme == ColorTheme.Light && labelOffset < 0)
            {
                labelOffset += 1;
            }
            browserLayoutPanel.RowStyles[0].Height -= labelOffset;

            var colorTable = themeRenderer.ToolStripRenderer.ColorTable;
            browserLabel.BackColor = closeBrowserButton.BackColor = colorTable.SeparatorDark;
            browserLabel.ForeColor = closeBrowserButton.ForeColor = colorTable.ControlForeColor;
            annotationPanel.InitializeTheme();
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
