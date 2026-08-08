using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Bonsai.Editor.GraphModel;
using Bonsai.Editor.Properties;
using Bonsai.Editor.Themes;

namespace Bonsai.Editor.GraphView
{
    partial class WorkflowPathNavigationControl : UserControl
    {
        static readonly object WorkflowPathMouseClickEvent = new();
        readonly IServiceProvider serviceProvider;
        readonly IWorkflowEditorService editorService;
        readonly ThemeRenderer themeRenderer;
        readonly Image homeImage;
        Button homeButton;
        WorkflowEditorPath workflowPath;
        int totalPathWidth;

        public WorkflowPathNavigationControl(IServiceProvider provider)
        {
            InitializeComponent();
            serviceProvider = provider;
            themeRenderer = (ThemeRenderer)provider.GetService(typeof(ThemeRenderer));
            themeRenderer.ThemeChanged += ThemeRenderer_ThemeChanged;
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
            homeImage = Resources.HomeImage;
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
            homeButton = AddPathButton(text: null, path: null);
            homeButton.AccessibleName = editorService.GetProjectDisplayName();
            UpdateHomeImage();
            AddPathButton("...", null, createEvent: false, visible: false);
            foreach (var path in pathElements)
            {
                AddSeparator(path.Value);
                AddPathButton(path.Key, path.Value);
            }
            SetSymbolButtonPadding();
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

        private void AddSeparator(WorkflowEditorPath pathElement)
        {
            var separator = new BreadcrumbSeparator
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Text = ">",
                PathElement = pathElement
            };
            separator.MouseClick += Separator_MouseClick;
            separator.ParentChanged += BreadcrumbButton_ParentChanged;
            SetBreadcrumbTheme(separator, themeRenderer);
            flowLayoutPanel.Controls.Add(separator);
        }

        private void UpdateHomeImage()
        {
            if (homeButton == null)
                return;

            var iconSize = new Size(homeButton.Font.Height, homeButton.Font.Height);
            var previousImage = homeButton.Image;
            if (themeRenderer.ActiveTheme == ColorTheme.Dark)
            {
                using var inverted = ThemeHelper.Invert(homeImage);
                homeButton.Image = ScaleImage(inverted, iconSize);
            }
            else homeButton.Image = ScaleImage(homeImage, iconSize);
            previousImage?.Dispose();
        }

        private void SetSymbolButtonPadding()
        {
            var ellipsis = flowLayoutPanel.Controls[1];
            var referenceHeight = ellipsis.PreferredSize.Height;
            var horizontalPadding = ellipsis.Font.Height / 4;
            foreach (Control control in flowLayoutPanel.Controls)
            {
                if (control != homeButton && control != ellipsis && control is not BreadcrumbSeparator)
                    continue;

                control.Padding = Padding.Empty;
                var verticalPadding = Math.Max(0, referenceHeight - control.PreferredSize.Height) / 2;
                control.Padding = new Padding(horizontalPadding, verticalPadding, horizontalPadding, verticalPadding);
            }
        }

        private static Image ScaleImage(Image image, Size size)
        {
            var result = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(result))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, size.Width, size.Height);
            }
            return result;
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

        private void Separator_MouseClick(object sender, MouseEventArgs e)
        {
            var separator = (BreadcrumbSeparator)sender;
            var workflowBuilder = (WorkflowBuilder)serviceProvider.GetService(typeof(WorkflowBuilder));
            var siblings = WorkflowEditorPath.GetSiblingDisplayElements(separator.PathElement, workflowBuilder);
            var menu = new ContextMenuStrip();
            foreach (var sibling in siblings)
            {
                var item = new ToolStripMenuItem(sibling.Key)
                {
                    Tag = sibling.Value,
                    Checked = sibling.Value == separator.PathElement
                };
                item.Click += SiblingItem_Click;
                menu.Items.Add(item);
            }
            menu.Show(separator, new Point(0, separator.Height));
        }

        private void SiblingItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var path = (WorkflowEditorPath)item.Tag;
            OnWorkflowPathMouseClick(new WorkflowPathMouseEventArgs(path, MouseButtons.Left, 1, 0, 0, 0));
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
            UpdateHomeImage();
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

        class BreadcrumbSeparator : BreadcrumbButtton
        {
            public WorkflowEditorPath PathElement { get; set; }
        }
    }
}
