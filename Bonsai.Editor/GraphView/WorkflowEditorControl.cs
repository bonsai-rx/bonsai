using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Bonsai.Expressions;
using Bonsai.Editor;
using Bonsai.Design;

namespace Bonsai.Editor.GraphView
{
    partial class WorkflowEditorControl : UserControl
    {
        IServiceProvider serviceProvider;
        IWorkflowEditorService editorService;
        WorkflowSelectionModel selectionModel;
        TabPageController workflowTab;
        TabPageController activeTab;
        Padding? adjustMargin;

        public WorkflowEditorControl(IServiceProvider provider)
            : this(provider, false)
        {
        }

        public WorkflowEditorControl(IServiceProvider provider, bool readOnly)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            InitializeComponent();
            serviceProvider = provider;
            selectionModel = (WorkflowSelectionModel)provider.GetService(typeof(WorkflowSelectionModel));
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
            workflowTab = InitializeTab(workflowTabPage, readOnly, null);
            InitializeTheme(workflowTabPage);
        }

        public WorkflowGraphView WorkflowGraphView
        {
            get { return workflowTab.WorkflowGraphView; }
        }

        public VisualizerLayout VisualizerLayout
        {
            get { return WorkflowGraphView.VisualizerLayout; }
            set { WorkflowGraphView.VisualizerLayout = value; }
        }

        public ExpressionBuilderGraph Workflow
        {
            get { return WorkflowGraphView.Workflow; }
            set { WorkflowGraphView.Workflow = value; }
        }

        public void UpdateVisualizerLayout()
        {
            WorkflowGraphView.UpdateVisualizerLayout();
        }

        public TabPageController ActiveTab
        {
            get { return activeTab; }
        }

        public int ItemHeight
        {
            get { return tabControl.DisplayRectangle.Y; }
        }

        TabPageController InitializeTab(TabPage tabPage, bool readOnly, Control container)
        {
            var workflowGraphView = new WorkflowGraphView(serviceProvider, this, readOnly);
            workflowGraphView.BackColorChanged += (sender, e) =>
            {
                tabPage.BackColor = workflowGraphView.BackColor;
                if (tabControl.SelectedTab == tabPage) InitializeTheme(tabPage);
            };
            workflowGraphView.Dock = DockStyle.Fill;
            workflowGraphView.Font = Font;
            workflowGraphView.Tag = tabPage;

            var tabState = new TabPageController(tabPage, workflowGraphView, this);
            tabPage.Tag = tabState;
            tabPage.SuspendLayout();
            if (container != null)
            {
                container.TextChanged += (sender, e) => tabState.Text = container.Text;
                container.Controls.Add(workflowGraphView);
                tabPage.Controls.Add(container);
            }
            else tabPage.Controls.Add(workflowGraphView);
            tabPage.BackColor = workflowGraphView.BackColor;
            tabPage.ResumeLayout(false);
            tabPage.PerformLayout();
            return tabState;
        }

        public TabPageController CreateTab(IWorkflowExpressionBuilder builder, bool readOnly, Control owner)
        {
            var tabPage = new TabPage();
            tabPage.Padding = workflowTabPage.Padding;
            tabPage.UseVisualStyleBackColor = workflowTabPage.UseVisualStyleBackColor;

            var tabState = InitializeTab(tabPage, readOnly || builder is IncludeWorkflowBuilder, owner);
            tabState.Text = ExpressionBuilder.GetElementDisplayName(builder);
            tabState.WorkflowGraphView.Workflow = builder.Workflow;
            tabState.Builder = builder;
            tabControl.TabPages.Add(tabPage);
            return tabState;
        }

        public void SelectTab(IWorkflowExpressionBuilder builder)
        {
            var tabPage = FindTab(builder);
            if (tabPage != null)
            {
                tabControl.SelectTab(tabPage);
            }
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

        public void CloseTab(IWorkflowExpressionBuilder builder)
        {
            var tabPage = FindTab(builder);
            if (tabPage != null)
            {
                var tabState = (TabPageController)tabPage.Tag;
                CloseTab(tabState);
            }
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

        public void RefreshTab(IWorkflowExpressionBuilder builder)
        {
            var tabPage = FindTab(builder);
            if (tabPage != null)
            {
                var tabState = (TabPageController)tabPage.Tag;
                RefreshTab(tabState);
            }
        }

        void RefreshTab(TabPageController tabState)
        {
            var builder = tabState.Builder;
            var workflowGraphView = tabState.WorkflowGraphView;
            if (builder != null && builder.Workflow != workflowGraphView.Workflow)
            {
                CloseTab(tabState);
            }
        }

        TabPage FindTab(IWorkflowExpressionBuilder builder)
        {
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                var tabState = (TabPageController)tabPage.Tag;
                if (tabState.Builder == builder)
                {
                    return tabPage;
                }
            }

            return null;
        }

        void ActivateTab(TabPage tabPage)
        {
            var tabState = tabPage != null ? (TabPageController)tabPage.Tag : null;
            if (tabState != null && activeTab != tabState)
            {
                activeTab = tabState;
                RefreshTab(activeTab);
                activeTab.UpdateSelection();
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
            WorkflowEditorControl owner;

            public TabPageController(TabPage tabPage, WorkflowGraphView graphView, WorkflowEditorControl editorControl)
            {
                if (tabPage == null)
                {
                    throw new ArgumentNullException("tabPage");
                }

                if (graphView == null)
                {
                    throw new ArgumentNullException("graphView");
                }

                if (editorControl == null)
                {
                    throw new ArgumentNullException("editorControl");
                }

                TabPage = tabPage;
                WorkflowGraphView = graphView;
                owner = editorControl;
            }

            public TabPage TabPage { get; private set; }

            public IWorkflowExpressionBuilder Builder { get; set; }

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
                TabPage.Text = displayText + (WorkflowGraphView.ReadOnly ? ReadOnlySuffix : string.Empty) + CloseSuffix;
            }

            public void UpdateSelection()
            {
                WorkflowGraphView.UpdateSelection();
            }

            public event CancelEventHandler TabClosing;

            internal void OnTabClosing(CancelEventArgs e)
            {
                var handler = TabClosing;
                if (handler != null)
                {
                    handler(this, e);
                }
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
                var tabState = (TabPageController)selectedTab.Tag;
                if (tabState.Builder != null)
                {
                    CloseTab(tabState);
                }
            }

            editorService.OnKeyDown(e);
        }

        void tabControl_MouseUp(object sender, MouseEventArgs e)
        {
            var selectedTab = tabControl.SelectedTab;
            if (selectedTab == null) return;

            if (e.Button == MouseButtons.Right)
            {
                tabContextMenuStrip.Show(tabControl, e.Location);
                return;
            }

            var tabState = (TabPageController)selectedTab.Tag;
            var tabRect = tabControl.GetTabRect(tabControl.SelectedIndex);
            if (tabState.Builder != null && tabRect.Contains(e.Location))
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

            var tabState = (TabPageController)selectedTab.Tag;
            if (tabState.Builder != null)
            {
                CloseTab(tabState);
            }
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            while (tabControl.TabCount > 1)
            {
                var tabState = (TabPageController)tabControl.TabPages[1].Tag;
                if (tabState.Builder != null)
                {
                    CloseTab(tabState);
                }
            }
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
        }
    }
}
