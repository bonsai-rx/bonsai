using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Bonsai.Editor.GraphModel;
using Bonsai.Editor.GraphView;
using Bonsai.Editor.Themes;
using Bonsai.Expressions;

namespace Bonsai.Editor.Docking
{
    partial class WatchToolWindow : NavigateToolWindow
    {
        readonly WorkflowEditorControl editorControl;
        readonly WorkflowWatchMap watchMap;

        public WatchToolWindow(IServiceProvider provider, WorkflowEditorControl owner)
            : base(provider)
        {
            InitializeComponent();
            editorControl = owner ?? throw new ArgumentNullException(nameof(owner));
            watchMap = (WorkflowWatchMap)provider.GetService(typeof(WorkflowWatchMap));
            watchListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        protected override void InitializeTheme(ThemeRenderer themeRenderer)
        {
            var colorTable = themeRenderer.ToolStripRenderer.ColorTable;
            watchListView.BackColor = colorTable.WindowBackColor;
            watchListView.ForeColor = colorTable.ControlForeColor;
        }

        public WorkflowWatchMap WatchMap => watchMap;

        public void UpdateWatchList()
        {
            watchListView.BeginUpdate();
            if (watchListView.Items.Count > 0)
                watchListView.Items.Clear();

            if (watchMap is not null)
            {
                var workflowBuilder = (WorkflowBuilder)ServiceProvider.GetService(typeof(WorkflowBuilder));
                foreach (var watch in workflowBuilder.FindAll(watchMap.Contains, unwrap: false))
                {
                    var inspectBuilder = (InspectBuilder)watch.Builder;
                    var item = CreateNavigationViewItem(inspectBuilder, watch.Path, workflowBuilder);
                    watchListView.Items.Add(item);
                }

                if (watchListView.Items.Count > 0)
                {
                    watchListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    watchListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                }
            }
            watchListView.EndUpdate();
        }

        private void DeleteSelectedItems()
        {
            if (watchListView.SelectedItems.Count == 0)
                return;

            watchListView.BeginUpdate();
            var updatedPaths = new HashSet<WorkflowEditorPath>();
            var workflowBuilder = (WorkflowBuilder)ServiceProvider.GetService(typeof(WorkflowBuilder));
            for (int i = watchListView.SelectedItems.Count - 1; i >= 0; i--)
            {
                var selectedItem = watchListView.SelectedItems[i];
                var workflowPath = (WorkflowEditorPath)selectedItem.Tag;
                var inspectBuilder = (InspectBuilder)workflowPath.Resolve(workflowBuilder);
                watchListView.Items.RemoveAt(selectedItem.Index);
                if (watchMap.Remove(inspectBuilder))
                {
                    updatedPaths.Add(workflowPath.Parent);
                }
            }
            watchListView.EndUpdate();
            editorControl.UpdateWatchLayout(updatedPaths);
        }

        private void SelectAllItems()
        {
            watchListView.BeginUpdate();
            foreach (ListViewItem item in watchListView.Items)
            {
                item.Selected = true;
            }
            watchListView.EndUpdate();
        }

        private void NavigateToSelectedItem(NavigationPreference navigationPreference)
        {
            if (watchListView.SelectedItems.Count == 0)
                return;

            var selectedItem = watchListView.SelectedItems[0];
            var workflowPath = (WorkflowEditorPath)selectedItem.Tag;
            OnNavigate(new WorkflowNavigateEventArgs(workflowPath, navigationPreference));
        }

        private void watchListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var focusedItem = watchListView.FocusedItem;
                if (focusedItem is not null && focusedItem.Bounds.Contains(e.Location))
                {
                    contextMenuStrip.Show(Cursor.Position);
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == openNewTabToolStripMenuItem.ShortcutKeys)
                openNewTabToolStripMenuItem_Click(this, EventArgs.Empty);
            if (keyData == openNewWindowToolStripMenuItem.ShortcutKeys)
                openNewWindowToolStripMenuItem_Click(this, EventArgs.Empty);
            if (keyData == deleteWatchToolStripMenuItem.ShortcutKeys)
                deleteWatchToolStripMenuItem_Click(this, EventArgs.Empty);
            if (keyData == selectAllToolStripMenuItem.ShortcutKeys)
                selectAllToolStripMenuItem_Click(this, EventArgs.Empty);
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void watchListView_ItemActivate(object sender, EventArgs e)
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

        private void deleteWatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedItems();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectAllItems();
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectAllItems();
            DeleteSelectedItems();
        }
    }
}
