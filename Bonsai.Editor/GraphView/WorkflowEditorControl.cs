using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Bonsai.Expressions;
using Bonsai.Editor.Themes;
using Bonsai.Editor.GraphModel;

namespace Bonsai.Editor.GraphView
{
    partial class WorkflowEditorControl : UserControl
    {
        readonly IServiceProvider serviceProvider;
        readonly IWorkflowEditorService editorService;
        readonly TabPageController workflowTab;
        readonly ThemeRenderer themeRenderer;
        Padding? adjustMargin;

        public WorkflowEditorControl(IServiceProvider provider)
        {
            InitializeComponent();
            serviceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
            themeRenderer = (ThemeRenderer)provider.GetService(typeof(ThemeRenderer));
            workflowTab = InitializeTab(workflowTabPage);
            annotationPanel.ThemeRenderer = themeRenderer;
            annotationPanel.LinkClicked += (sender, e) => { EditorDialog.OpenUrl(e.LinkText); };
            annotationPanel.CloseRequested += delegate { CollapseAnnotationPanel(); };
            InitializeTheme(workflowTabPage);
        }

        public WorkflowGraphView WorkflowGraphView
        {
            get { return workflowTab.WorkflowGraphView; }
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

        public TabPageController ActiveTab { get; private set; }

        public int ItemHeight
        {
            get { return tabControl.DisplayRectangle.Y; }
        }

        TabPageController InitializeTab(TabPage tabPage)
        {
            var workflowGraphView = new WorkflowGraphView(serviceProvider, this);
            workflowGraphView.BackColorChanged += (sender, e) =>
            {
                tabPage.BackColor = workflowGraphView.BackColor;
                if (tabControl.SelectedTab == tabPage) InitializeTheme(tabPage);
            };
            workflowGraphView.Margin = new Padding(0);
            workflowGraphView.Dock = DockStyle.Fill;
            workflowGraphView.Font = Font;
            workflowGraphView.Tag = tabPage;

            var tabState = new TabPageController(tabPage, workflowGraphView);
            tabPage.Tag = tabState;
            tabPage.SuspendLayout();

            var breadcrumbs = new WorkflowPathNavigationControl(serviceProvider);
            breadcrumbs.Margin = new Padding(0);
            breadcrumbs.WorkflowPath = null;
            breadcrumbs.WorkflowPathMouseClick += (sender, e) => workflowGraphView.WorkflowPath = e.Path;
            workflowGraphView.WorkflowPathChanged += (sender, e) =>
            {
                breadcrumbs.WorkflowPath = workflowGraphView.WorkflowPath;
            };

            var navigationPanel = new TableLayoutPanel();
            navigationPanel.Dock = DockStyle.Fill;
            navigationPanel.ColumnCount = 1;
            navigationPanel.RowCount = 2;
            navigationPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, breadcrumbs.Height));
            navigationPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            navigationPanel.Controls.Add(breadcrumbs);
            navigationPanel.Controls.Add(workflowGraphView);

            // TODO: This should be handled by docking, but some strange interaction prevents shrinking to min size
            navigationPanel.Layout += (sender, e) => breadcrumbs.Width = navigationPanel.Width;
            breadcrumbs.Width = navigationPanel.Width;

            tabPage.Controls.Add(navigationPanel);
            tabPage.BackColor = workflowGraphView.BackColor;
            tabPage.ResumeLayout(false);
            tabPage.PerformLayout();
            return tabState;
        }

        public TabPageController CreateTab(WorkflowEditorPath workflowPath)
        {
            var tabPage = new TabPage();
            tabPage.Padding = workflowTabPage.Padding;
            tabPage.UseVisualStyleBackColor = workflowTabPage.UseVisualStyleBackColor;

            var tabState = InitializeTab(tabPage);
            tabState.WorkflowGraphView.WorkflowPath = workflowPath;
            tabControl.TabPages.Add(tabPage);
            return tabState;
        }

        public void SelectTab(WorkflowGraphView workflowGraphView)
        {
            var tabPage = (TabPage)workflowGraphView.Tag;
            if (tabPage != null)
            {
                var tabIndex = tabControl.TabPages.IndexOf(tabPage);
                if (tabIndex >= 0) tabControl.SelectTab(tabIndex);
            }
        }

        public void ResetNavigation()
        {
            CloseAll();
            WorkflowGraphView.Editor.ResetNavigation();
        }

        void CloseAll()
        {
            while (tabControl.TabCount > 1)
            {
                CloseTab(tabControl.TabPages[1]);
            }
        }

        void CloseTab(TabPage tabPage)
        {
            var tabState = (TabPageController)tabPage.Tag;
            CloseTab(tabState);
        }

        void CloseTab(TabPageController tabState)
        {
            var tabPage = tabState.TabPage;
            var cancelEventArgs = new CancelEventArgs();
            tabState.OnTabClosing(cancelEventArgs);
            if (!cancelEventArgs.Cancel)
            {
                tabControl.SuspendLayout();
                var tabIndex = tabControl.TabPages.IndexOf(tabPage);
                if (tabControl.SelectedIndex >= tabIndex)
                {
                    tabControl.SelectTab(tabIndex - 1);
                }
                tabControl.TabPages.Remove(tabPage);
                tabControl.ResumeLayout();
                tabPage.Dispose();
            }
        }

        void ActivateTab(TabPage tabPage)
        {
            var tabState = tabPage != null ? (TabPageController)tabPage.Tag : null;
            if (tabState != null && ActiveTab != tabState)
            {
                ActiveTab = tabState;
                ActiveTab.UpdateSelection();
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            WorkflowGraphView.Font = Font;
            base.OnFontChanged(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            ActivateTab(workflowTabPage);
            base.OnLoad(e);
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

        internal class TabPageController
        {
            const string CloseSuffix = "   \u2715";
            const string ReadOnlySuffix = " [Read-only]";
            string displayText;

            public TabPageController(TabPage tabPage, WorkflowGraphView graphView)
            {
                TabPage = tabPage ?? throw new ArgumentNullException(nameof(tabPage));
                WorkflowGraphView = graphView ?? throw new ArgumentNullException(nameof(graphView));
            }

            public TabPage TabPage { get; private set; }

            public WorkflowGraphView WorkflowGraphView { get; private set; }

            public string Text
            {
                get { return displayText; }
                set
                {
                    displayText = value;
                    UpdateDisplayText();
                }
            }

            void UpdateDisplayText()
            {
                TabPage.Text = displayText + (WorkflowGraphView.IsReadOnly ? ReadOnlySuffix : string.Empty);
            }

            public void UpdateSelection()
            {
                WorkflowGraphView.UpdateSelection();
            }

            public event CancelEventHandler TabClosing;

            internal void OnTabClosing(CancelEventArgs e)
            {
                TabClosing?.Invoke(this, e);
            }
        }

        private void tabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (e.Action == TabControlAction.Selected)
            {
                ActivateTab(e.TabPage);
            }
        }

        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            e.Cancel = e.TabPageIndex >= tabControl.TabPages.Count;
        }

        void tabControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F4)
            {
                var selectedTab = tabControl.SelectedTab;
                if (selectedTab == null) return;
                CloseTab(selectedTab);
            }

            editorService.OnKeyDown(e);
        }

        void tabControl_MouseUp(object sender, MouseEventArgs e)
        {
            var selectedTab = tabControl.SelectedTab;
            if (selectedTab == null) return;

            if (e.Button == MouseButtons.Middle)
            {
                CloseTab(selectedTab);
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                tabContextMenuStrip.Show(tabControl, e.Location);
                return;
            }

            var tabState = (TabPageController)selectedTab.Tag;
            var tabRect = tabControl.GetTabRect(tabControl.SelectedIndex);
            if (selectedTab != workflowTabPage && tabRect.Contains(e.Location))
            {
                using (var graphics = selectedTab.CreateGraphics())
                {
                    var textSize = TextRenderer.MeasureText(
                        graphics,
                        selectedTab.Text,
                        selectedTab.Font,
                        tabRect.Size,
                        TextFormatFlags.Default |
                        TextFormatFlags.NoPadding);
                    var padSize = TextRenderer.MeasureText(
                        graphics,
                        selectedTab.Text.Substring(0, selectedTab.Text.Length - 1),
                        selectedTab.Font,
                        tabRect.Size,
                        TextFormatFlags.Default |
                        TextFormatFlags.NoPadding);
                    const float DefaultDpi = 96f;
                    var offset = graphics.DpiX / DefaultDpi;
                    var margin = (tabRect.Width - textSize.Width) / 2;
                    var buttonWidth = textSize.Width - padSize.Width;
                    var buttonRight = tabRect.Right - margin;
                    var buttonLeft = buttonRight - buttonWidth;
                    var buttonTop = tabRect.Top + 2 * selectedTab.Margin.Top;
                    var buttonBottom = tabRect.Bottom - 2 * selectedTab.Margin.Bottom;
                    var buttonHeight = buttonBottom - buttonTop;
                    var buttonBounds = new Rectangle(buttonLeft, buttonTop, (int)(buttonWidth + offset), buttonHeight);
                    if (buttonBounds.Contains(e.Location))
                    {
                        CloseTab(tabState);
                    }
                }
            }
        }

        private void tabContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            var selectedTab = tabControl.SelectedTab;
            if (selectedTab == null) return;
            closeToolStripMenuItem.Enabled = tabControl.SelectedTab != workflowTabPage;
            closeAllToolStripMenuItem.Enabled = tabControl.TabCount > 1;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedTab = tabControl.SelectedTab;
            if (selectedTab == null) return;
            CloseTab(selectedTab);
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseAll();
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            var displayX = tabControl.DisplayRectangle.X;
            var marginLeft = tabControl.Margin.Left;
            var marginTop = tabControl.Margin.Top;
            if (adjustMargin.HasValue)
            {
                var adjustH = (int)Math.Round(marginLeft * factor.Width - marginLeft);
                var adjustV = (int)Math.Round(marginTop * factor.Height - marginTop);
                adjustMargin -= new Padding(adjustH, adjustV, adjustH, adjustH);
            }
            else
            {
                var adjustH = displayX - marginLeft - 1;
                var adjustV = displayX - marginTop - displayX / 2 - 1;
                adjustMargin = new Padding(adjustH, adjustV, adjustH, adjustH);
            }
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

        private void InitializeTheme(TabPage tabPage)
        {
            var adjustRectangle = tabControl.Margin + adjustMargin.GetValueOrDefault();
            if (tabPage.BackColor.GetBrightness() > 0.5f)
            {
                adjustRectangle.Right -= 2;
                adjustRectangle.Bottom -= tabControl.Margin.Top - tabControl.Margin.Left + 1;
            }
            else adjustRectangle.Bottom = adjustRectangle.Left;
            tabControl.AdjustRectangle = adjustRectangle;

            var labelOffset = browserLabel.Height - ItemHeight + 1;
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
                        ActiveTab.WorkflowGraphView.Focus();
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
