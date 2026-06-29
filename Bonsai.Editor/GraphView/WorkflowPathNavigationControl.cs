using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Bonsai.Editor.GraphModel;
using Bonsai.Editor.Themes;

namespace Bonsai.Editor.GraphView
{
    partial class WorkflowPathNavigationControl : UserControl
    {
        const string HomeGlyph = "🏠";
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
            flowLayoutPanel.HandleCreated += (sender, e) =>
            {
                // ensure child handles are created in collection order so changes in visibility never
                // force an out-of-order CreateWindowEx, which would reorder the control collection
                foreach (Control control in flowLayoutPanel.Controls)
                    _ = control.Handle;
            };
        }

        public WorkflowEditorPath WorkflowPath
        {
            get { return workflowPath; }
            set
            {
                var workflowBuilder = (WorkflowBuilder)serviceProvider.GetService(typeof(WorkflowBuilder));
                var pathElements = WorkflowEditorPath.GetPathDisplayElements(value, workflowBuilder);
                if (workflowPath == value && flowLayoutPanel.Controls.Count > 1)
                {
                    RefreshDisplayNames(pathElements);
                }
                else
                {
                    workflowPath = value;
                    SetPath(pathElements);
                }
            }
        }

        private void RefreshDisplayNames(IEnumerable<KeyValuePair<string, WorkflowEditorPath>> pathElements)
        {
            SuspendLayout();
            flowLayoutPanel.Controls[0].AccessibleName = editorService.GetProjectDisplayName();
            using var elementEnumerator = pathElements.GetEnumerator();
            for (int i = 1; i < flowLayoutPanel.Controls.Count; i++)
            {
                var control = flowLayoutPanel.Controls[i];
                if (control.Tag is WorkflowEditorPath && elementEnumerator.MoveNext())
                    control.Text = elementEnumerator.Current.Key;
            }
            UpdatePathWidth();
            CompressPath();
            ResumeLayout(true);
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

        private void SetPath(IEnumerable<KeyValuePair<string, WorkflowEditorPath>> pathElements)
        {
            SuspendLayout();
            flowLayoutPanel.Controls.Clear();
            var homeButton = AddPathButton(HomeGlyph, null);
            homeButton.AccessibleName = editorService.GetProjectDisplayName();
            AddPathButton("...", null, createEvent: false, visible: false);
            foreach (var path in pathElements)
            {
                AddPathButton(">", null, createEvent: false);
                AddPathButton(path.Key, path.Value);
            }
            UpdatePathWidth();
            CompressPath();
            ResumeLayout(true);
        }

        private void UpdatePathWidth()
        {
            totalPathWidth = GetControlWidth(flowLayoutPanel.Controls[0]);
            for (int i = 2; i < flowLayoutPanel.Controls.Count; i++)
                totalPathWidth += GetControlWidth(flowLayoutPanel.Controls[i]);
        }

        private void CompressPath()
        {
            if (flowLayoutPanel.Controls.Count <= 4)
                return;

            var excessWidth = totalPathWidth - Width;
            if (excessWidth > 0)
            {
                // the home button is pinned, so the ellipsis only adds to the path width
                excessWidth += flowLayoutPanel.Controls[1].PreferredSize.Width;
            }

            bool compressPath = false;
            for (int i = 2; i < flowLayoutPanel.Controls.Count - 4; i++)
            {
                // separator and breadcrumb buttons are hidden together
                var visible = excessWidth <= 0;
                if (i % 2 != 0) visible &= flowLayoutPanel.Controls[i - 1].Visible;

                // hide excess breadcrumb levels
                flowLayoutPanel.Controls[i].Visible = visible;
                compressPath |= !visible;
                if (excessWidth > 0)
                {
                    excessWidth -= GetControlWidth(flowLayoutPanel.Controls[i]);
                }
            }

            // the ellipsis marks collapsed levels
            flowLayoutPanel.Controls[1].Visible = compressPath;
        }

        private int GetControlWidth(Control control)
        {
            return control.PreferredSize.Width + control.Margin.Horizontal + flowLayoutPanel.Padding.Right;
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
