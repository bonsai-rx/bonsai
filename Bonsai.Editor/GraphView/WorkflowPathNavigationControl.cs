using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Bonsai.Editor.GraphModel;
using Bonsai.Editor.Themes;
using Bonsai.Expressions;

namespace Bonsai.Editor.GraphView
{
    partial class WorkflowPathNavigationControl : UserControl
    {
        static readonly object WorkflowPathMouseClickEvent = new();
        readonly IServiceProvider serviceProvider;
        readonly IWorkflowEditorService editorService;
        readonly ThemeRenderer themeRenderer;
        WorkflowEditorPath workflowPath;

        public WorkflowPathNavigationControl(IServiceProvider provider)
        {
            InitializeComponent();
            serviceProvider = provider;
            themeRenderer = (ThemeRenderer)provider.GetService(typeof(ThemeRenderer));
            themeRenderer.ThemeChanged += ThemeRenderer_ThemeChanged;
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
        }

        public string DisplayName
        {
            get
            {
                return flowLayoutPanel.Controls.Count > 0
                    ? flowLayoutPanel.Controls[flowLayoutPanel.Controls.Count - 1].Text
                    : editorService.GetProjectDisplayName();
            }
        }

        public WorkflowEditorPath WorkflowPath
        {
            get { return workflowPath; }
            set
            {
                workflowPath = value;
                var workflowBuilder = (WorkflowBuilder)serviceProvider.GetService(typeof(WorkflowBuilder));
                var pathElements = GetPathElements(workflowPath, workflowBuilder);
                SetPath(pathElements);
            }
        }

        public event EventHandler<WorkflowPathMouseEventArgs> WorkflowPathMouseClick
        {
            add { Events.AddHandler(WorkflowPathMouseClickEvent, value); }
            remove { Events.RemoveHandler(WorkflowPathMouseClickEvent, value); }
        }

        private void OnWorkflowPathMouseClick(WorkflowPathMouseEventArgs e)
        {
            (Events[WorkflowPathMouseClickEvent] as EventHandler<WorkflowPathMouseEventArgs>)?.Invoke(this, e);
        }

        static IEnumerable<KeyValuePair<string, WorkflowEditorPath>> GetPathElements(WorkflowEditorPath workflowPath, WorkflowBuilder workflowBuilder)
        {
            var workflow = workflowBuilder.Workflow;
            foreach (var pathElement in workflowPath?.GetPathElements() ?? Enumerable.Empty<WorkflowEditorPath>())
            {
                var builder = workflow[pathElement.Index].Value;
                if (ExpressionBuilder.Unwrap(builder) is IWorkflowExpressionBuilder nestedWorkflowBuilder)
                {
                    workflow = nestedWorkflowBuilder.Workflow;
                }

                yield return new(
                    key: ExpressionBuilder.GetElementDisplayName(builder),
                    value: pathElement);
            }
        }

        private void SetPath(IEnumerable<KeyValuePair<string, WorkflowEditorPath>> pathElements)
        {
            SuspendLayout();
            var rootButton = CreateButton(editorService.GetProjectDisplayName(), null);
            flowLayoutPanel.Controls.Clear();
            flowLayoutPanel.Controls.Add(rootButton);
            foreach (var path in pathElements)
            {
                var separator = CreateButton(">", null, createEvent: false);
                var pathButton = CreateButton(path.Key, path.Value);
                flowLayoutPanel.Controls.Add(separator);
                flowLayoutPanel.Controls.Add(pathButton);
            }
            ResumeLayout(true);
        }

        private Button CreateButton(string text, WorkflowEditorPath path, bool createEvent = true)
        {
            var breadcrumbButton = new BreadcrumbButtton
            {
                AutoSize = true,
                Locked = !createEvent,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Text = text,
                Tag = path
            };
            if (createEvent)
                breadcrumbButton.MouseClick += BreadcrumbButton_MouseClick;
            SetBreadcrumbTheme(breadcrumbButton, themeRenderer);
            return breadcrumbButton;
        }

        private void BreadcrumbButton_MouseClick(object sender, MouseEventArgs e)
        {
            var button = (Button)sender;
            var path = (WorkflowEditorPath)button.Tag;
            OnWorkflowPathMouseClick(new WorkflowPathMouseEventArgs(path, e.Button, e.Clicks, e.X, e.Y, e.Delta));
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            themeRenderer.ThemeChanged -= ThemeRenderer_ThemeChanged;
            base.OnHandleDestroyed(e);
        }

        private void ThemeRenderer_ThemeChanged(object sender, EventArgs e)
        {
            InitializeTheme();
        }

        internal void InitializeTheme()
        {
            foreach (Button button in flowLayoutPanel.Controls)
            {
                SetBreadcrumbTheme(button, themeRenderer);
            }
        }

        private static void SetBreadcrumbTheme(Button button, ThemeRenderer themeRenderer)
        {
            if (themeRenderer == null)
                return;

            var colorTable = themeRenderer.ToolStripRenderer.ColorTable;
            button.BackColor = colorTable.WindowBackColor;
            button.ForeColor = colorTable.WindowText;
        }

        class BreadcrumbButtton : Button
        {
            bool locked;

            public bool Locked
            {
                get => locked;
                set
                {
                    locked = value;
                    SetStyle(ControlStyles.Selectable, !locked);
                }
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                if (Locked)
                    return;
                base.OnMouseEnter(e);
            }

            protected override void OnMouseDown(MouseEventArgs mevent)
            {
                if (Locked)
                    return;
                base.OnMouseDown(mevent);
            }
        }
    }
}
