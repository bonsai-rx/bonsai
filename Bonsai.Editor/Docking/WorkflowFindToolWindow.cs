using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Bonsai.Editor.GraphModel;
using Bonsai.Editor.Themes;
using Bonsai.Expressions;

namespace Bonsai.Editor.Docking
{
    partial class WorkflowFindToolWindow : NavigateToolWindow
    {
        IEnumerable<WorkflowQueryResult> query;

        public WorkflowFindToolWindow(IServiceProvider provider)
            : base(provider)
        {
            InitializeComponent();
            findListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        protected override void InitializeTheme(ThemeRenderer themeRenderer)
        {
            var colorTable = themeRenderer.ToolStripRenderer.ColorTable;
            findListView.BackColor = colorTable.WindowBackColor;
            findListView.ForeColor = colorTable.ControlForeColor;
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
                    var item = CreateNavigationViewItem(inspectBuilder, result.Path, workflowBuilder);
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
