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
            InitializeTabPage(workflowTabPage, readOnly);
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

        void InitializeTabPage(TabPage tabPage, bool readOnly)
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
        }

        public void OpenWorkflow(IncludeWorkflowBuilder builder)
        {
            var name = ExpressionBuilder.GetElementDisplayName(builder);
            var tabPage = CreateTabPage(name);
            var tabState = (TabPageController)tabPage.Tag;
            tabState.WorkflowGraphView.Workflow = builder.Workflow;
            tabControl.SelectTab(tabPage);
        }

        TabPage CreateTabPage(string text)
        {
            var tabPage = new TabPage(text);
            tabPage.Padding = workflowTabPage.Padding;
            tabPage.UseVisualStyleBackColor = workflowTabPage.UseVisualStyleBackColor;
            InitializeTabPage(tabPage, true);
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
            const string ModifiedMark = "*";
            const string CloseSuffix = "    \u2A2F";

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
    }
}
