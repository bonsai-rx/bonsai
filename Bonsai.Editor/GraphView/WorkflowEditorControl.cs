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

namespace Bonsai.Design
{
    partial class WorkflowEditorControl : UserControl, IWorkflowEditorView
    {
        IServiceProvider serviceProvider;
        IWorkflowEditorService editorService;
        WorkflowSelectionModel selectionModel;
        TabPageController workflowTab;
        TabPageController activeTab;

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
            InitializeTabState(workflowTabPage, readOnly);
            workflowTab = (TabPageController)workflowTabPage.Tag;
        }

        public GraphView GraphView
        {
            get { return null; }
        }

        public WorkflowGraphView WorkflowGraphView
        {
            get { return workflowTab.WorkflowGraphView; }
        }

        public VisualizerLayout VisualizerLayout
        {
            get { return workflowTab.WorkflowGraphView.VisualizerLayout; }
            set { workflowTab.WorkflowGraphView.VisualizerLayout = value; }
        }

        public ExpressionBuilderGraph Workflow
        {
            get { return workflowTab.WorkflowGraphView.Workflow; }
            set { workflowTab.WorkflowGraphView.Workflow = value; }
        }

        public GraphNode FindGraphNode(object value)
        {
            return null;
        }

        public void LaunchWorkflowView(GraphNode node)
        {
        }

        public WorkflowEditorLauncher GetWorkflowEditorLauncher(GraphNode node)
        {
            return null;
        }

        public void UpdateVisualizerLayout()
        {
        }

        TabPageController InitializeTabState(TabPage tabPage, bool readOnly)
        {
            var workflowGraphView = new WorkflowGraphView(serviceProvider, this, readOnly);
            workflowGraphView.AutoScaleDimensions = new SizeF(6F, 13F);
            workflowGraphView.Dock = DockStyle.Fill;
            workflowGraphView.Font = Font;

            var tabState = new TabPageController(tabPage, workflowGraphView, this);
            tabPage.Tag = tabState;
            tabPage.SuspendLayout();
            tabPage.Controls.Add(workflowGraphView);
            tabPage.BackColor = workflowGraphView.BackColor;
            tabPage.ResumeLayout(false);
            tabPage.PerformLayout();
            return tabState;
        }

        public void OpenWorkflow(IncludeWorkflowBuilder builder)
        {
            var tabPage = FindTabPage(builder);
            if (tabPage == null)
            {
                tabPage = CreateTabPage(builder);
            }

            tabControl.SelectTab(tabPage);
        }

        public void CloseWorkflow(IncludeWorkflowBuilder builder)
        {
            var tabPage = FindTabPage(builder);
            if (tabPage != null)
            {
                tabControl.SuspendLayout();
                var tabIndex = tabControl.TabPages.IndexOf(tabPage);
                tabControl.SelectTab(tabIndex - 1);
                tabControl.TabPages.Remove(tabPage);
                tabControl.ResumeLayout();
            }
        }

        TabPage FindTabPage(IncludeWorkflowBuilder builder)
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

        TabPage CreateTabPage(IncludeWorkflowBuilder builder)
        {
            var tabPage = new TabPage();
            tabPage.Padding = workflowTabPage.Padding;
            tabPage.UseVisualStyleBackColor = workflowTabPage.UseVisualStyleBackColor;
            var tabState = InitializeTabState(tabPage, true);
            tabState.Text = ExpressionBuilder.GetElementDisplayName(builder);
            tabState.WorkflowGraphView.Workflow = builder.Workflow;
            tabState.Builder = builder;
            tabControl.TabPages.Add(tabPage);
            return tabPage;
        }

        void ActivateTabPage(TabPage tabPage)
        {
            var tabState = tabPage != null ? (TabPageController)tabPage.Tag : null;
            if (tabState != null && activeTab != tabState)
            {
                activeTab = tabState;
                activeTab.WorkflowGraphView.Select();
                selectionModel.UpdateSelection(activeTab.WorkflowGraphView);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            ActivateTabPage(workflowTabPage);
            base.OnLoad(e);
        }

        class TabPageController
        {
            const string CloseSuffix = "   \u2715";

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

            public IncludeWorkflowBuilder Builder { get; set; }

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
                TabPage.Text = displayText + CloseSuffix;
            }
        }

        private void tabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (e.Action == TabControlAction.Selected)
            {
                ActivateTabPage(e.TabPage);
            }
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
                    CloseWorkflow(tabState.Builder);
                }
            }
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
                        CloseWorkflow(tabState.Builder);
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
                CloseWorkflow(tabState.Builder);
            }
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            while (tabControl.TabCount > 1)
            {
                var tabState = (TabPageController)tabControl.TabPages[1].Tag;
                if (tabState.Builder != null)
                {
                    CloseWorkflow(tabState.Builder);
                }
            }
        }
    }
}
