using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Bonsai.Editor.GraphModel;
using Bonsai.Editor.Themes;
using Bonsai.Expressions;

namespace Bonsai.Editor.Docking
{
    partial class WorkflowFindToolWindow : EditorToolWindow
    {
        static readonly object EventNavigate = new();
        IEnumerable<WorkflowQueryResult> query;

        public WorkflowFindToolWindow(IServiceProvider provider)
            : base(provider)
        {
            InitializeComponent();
            findListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        public event WorkflowNavigateEventHandler Navigate
        {
            add { Events.AddHandler(EventNavigate, value); }
            remove { Events.RemoveHandler(EventNavigate, value); }
        }

        protected override void InitializeTheme(ThemeRenderer themeRenderer)
        {
            var colorTable = themeRenderer.ToolStripRenderer.ColorTable;
            findListView.BackColor = colorTable.WindowBackColor;
            findListView.ForeColor = colorTable.ControlForeColor;
        }

        protected virtual void OnNavigate(WorkflowNavigateEventArgs e)
        {
            if (Events[EventNavigate] is WorkflowNavigateEventHandler handler)
            {
                handler(this, e);
            }
        }

        public IEnumerable<WorkflowQueryResult> Query
        {
            get => query;
            set
            {
                query = value;
                UpdateResults();
            }
        }

        public void UpdateResults()
        {
            findListView.BeginUpdate();
            if (findListView.Items.Count > 0)
                findListView.Items.Clear();

            if (query is not null)
            {
                var workflowBuilder = (WorkflowBuilder)ServiceProvider.GetService(typeof(WorkflowBuilder));
                foreach (var result in query)
                {
                    var inspectBuilder = (InspectBuilder)result.Builder;
                    var name = ExpressionBuilder.GetElementDisplayName(inspectBuilder);
                    var elementType = ExpressionBuilder.GetWorkflowElement(inspectBuilder).GetType();
                    var containerPath = GetElementDisplayPath(workflowBuilder, result.Path.Parent);
                    var item = new ListViewItem(name);
                    item.Tag = result.Path;
                    item.SubItems.Add(containerPath);
                    item.SubItems.Add(TypeHelper.GetTypeName(elementType));
                    item.SubItems.Add(inspectBuilder.ObservableType is not null
                        ? TypeHelper.GetTypeName(inspectBuilder.ObservableType)
                        : string.Empty);
                    findListView.Items.Add(item);
                }

                if (findListView.Items.Count > 0)
                {
                    findListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    findListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                }
            }
            findListView.EndUpdate();
        }

        static string GetElementDisplayPath(WorkflowBuilder workflowBuilder, WorkflowEditorPath path)
        {
            var sb = new StringBuilder();
            foreach (var pathElement in WorkflowEditorPath.GetPathDisplayElements(path, workflowBuilder))
            {
                if (sb.Length > 0)
                    sb.Append(" > ");
                sb.Append(pathElement.Key);
            }

            return sb.ToString();
        }

        private void NavigateToSelectedItem(NavigationPreference navigationPreference)
        {
            if (findListView.SelectedItems.Count == 0)
                return;

            var selectedItem = findListView.SelectedItems[0];
            var workflowPath = (WorkflowEditorPath)selectedItem.Tag;
            OnNavigate(new WorkflowNavigateEventArgs(workflowPath, navigationPreference));
        }

        private void findListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var focusedItem = findListView.FocusedItem;
                if (focusedItem is not null && focusedItem.Bounds.Contains(e.Location))
                {
                    contextMenuStrip.Show(Cursor.Position);
                }
            }
        }

        private void findListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == openNewTabToolStripMenuItem.ShortcutKeys)
                openNewTabToolStripMenuItem_Click(sender, e);
            if (e.KeyData == openNewWindowToolStripMenuItem.ShortcutKeys)
                openNewWindowToolStripMenuItem_Click(sender, e);
        }

        private void findListView_ItemActivate(object sender, EventArgs e)
        {
            NavigateToSelectedItem(NavigationPreference.Current);
        }

        private void openNewTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NavigateToSelectedItem(NavigationPreference.NewTab);
        }

        private void openNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NavigateToSelectedItem(NavigationPreference.NewWindow);
        }
    }
}
