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
        int totalPathWidth;

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
                if (ExpressionBuilder.GetWorkflowElement(builder) is IWorkflowExpressionBuilder nestedWorkflowBuilder)
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
            totalPathWidth = 0;
            flowLayoutPanel.Controls.Clear();
            AddPathButton("...", null, createEvent: false, visible: false);
            AddPathButton(editorService.GetProjectDisplayName(), null);
            foreach (var path in pathElements)
            {
                AddPathButton(">", null, createEvent: false);
                AddPathButton(path.Key, path.Value);
            }
            CompressPath();
            ResumeLayout(true);
        }

        private void CompressPath()
        {
            if (flowLayoutPanel.Controls.Count <= 4)
                return;

            bool compressPath = false;
            var totalWidth = totalPathWidth;
            if (totalWidth > Width)
            {
                // adjust for inserting the ellipsis button
                totalWidth -= flowLayoutPanel.Controls[1].Width;
                totalWidth += flowLayoutPanel.Controls[0].Width;
                compressPath = true;
            }

            var excessWidth = totalWidth - Width;
            for (int i = 2; i < flowLayoutPanel.Controls.Count - 4; i++)
            {
                // separator and breadcrumb buttons are hidden together
                var visible = !compressPath || excessWidth <= 0;
                if (i % 2 != 0) visible &= flowLayoutPanel.Controls[i - 1].Visible;

                // hide excess breadcrumb levels
                flowLayoutPanel.Controls[i].Visible = visible;
                if (excessWidth > 0)
                {
                    excessWidth -= GetControlWidth(flowLayoutPanel.Controls[i]);
                }
            }

            // either the root or ellipsis button is shown
            flowLayoutPanel.Controls[0].Visible = compressPath;
            flowLayoutPanel.Controls[1].Visible = !compressPath;
        }

        private int GetControlWidth(Control control)
        {
            return control.Width + control.Margin.Horizontal + flowLayoutPanel.Padding.Right;
        }

        private BreadcrumbButtton AddPathButton(string text, WorkflowEditorPath path, bool createEvent = true, bool visible = true)
        {
            var breadcrumbButton = new BreadcrumbButtton
            {
                AutoSize = true,
                Locked = !createEvent,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Visible = visible,
                Text = text,
                Tag = path
            };
            if (createEvent)
                breadcrumbButton.MouseClick += BreadcrumbButton_MouseClick;
            breadcrumbButton.ParentChanged += BreadcrumbButton_ParentChanged;
            SetBreadcrumbTheme(breadcrumbButton, themeRenderer);
            flowLayoutPanel.Controls.Add(breadcrumbButton);
            if (flowLayoutPanel.Controls.Count > 1)
                totalPathWidth += GetControlWidth(breadcrumbButton);
            return breadcrumbButton;
        }

        private void BreadcrumbButton_ParentChanged(object sender, EventArgs e)
        {
            var button = (Button)sender;
            if (button.Parent == null)
                button.Dispose();
        }

        private void BreadcrumbButton_MouseClick(object sender, MouseEventArgs e)
        {
            var button = (Button)sender;
            var path = (WorkflowEditorPath)button.Tag;
            OnWorkflowPathMouseClick(new WorkflowPathMouseEventArgs(path, e.Button, e.Clicks, e.X, e.Y, e.Delta));
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            CompressPath();
            base.OnLayout(e);
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
