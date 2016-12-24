using NuGet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.NuGet
{
    public partial class PackageSourceConfigurationDialog : Form
    {
        const string NuGetSourceName = "nuget.org";
        const string DefaultPackageSourceName = "Package source";
        const string DefaultPackageSource = "http://packagesource";
        readonly IPackageSourceProvider provider;

        public PackageSourceConfigurationDialog(IPackageSourceProvider sourceProvider)
        {
            if (sourceProvider == null)
            {
                throw new ArgumentNullException("sourceProvider");
            }

            InitializeComponent();
            provider = sourceProvider;
        }

        static PackageSource GetItemPackageSource(ListViewItem item)
        {
            if (item.Tag == null)
            {
                throw new InvalidOperationException("Package source list view items must have an associated tag set.");
            }

            return (PackageSource)item.Tag;
        }

        static void ToggleItemChecked(ListViewItem item)
        {
            item.Checked = !item.Checked;
            item.ImageIndex = GetStateImageIndex(item.Checked);
            GetItemPackageSource(item).IsEnabled = item.Checked;
        }

        static void OffsetItemIndex(ListViewItem item, int offset)
        {
            var listView = item.ListView;
            listView.BeginUpdate();
            listView.SuspendLayout();
            var itemIndex = item.Index;
            listView.Items.RemoveAt(itemIndex);
            listView.Items.Insert(itemIndex + offset, item);
            listView.Alignment = ListViewAlignment.Default;
            listView.Alignment = ListViewAlignment.Top;
            listView.ResumeLayout();
            listView.EndUpdate();
        }

        static int GetStateImageIndex(bool checkedState)
        {
            return checkedState ? 1 : 0;
        }

        protected override void OnLoad(EventArgs e)
        {
            foreach (var packageSource in provider.LoadPackageSources())
            {
                var imageIndex = GetStateImageIndex(packageSource.IsEnabled);
                var item = new ListViewItem(new[] { packageSource.Name, packageSource.Source }, imageIndex);
                if (packageSource.IsMachineWide) machineWideListView.Items.Add(item);
                else packageSourceListView.Items.Add(item);
                item.Checked = packageSource.IsEnabled;
                item.Tag = packageSource;
            }
            base.OnLoad(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                provider.SavePackageSources(from item in packageSourceListView.Items.Cast<ListViewItem>().Concat(
                                                         machineWideListView.Items.Cast<ListViewItem>())
                                            select GetItemPackageSource(item));
            }
            base.OnClosed(e);
        }

        private void UpdateItemActionButtons(ListViewItem item)
        {
            var removed = item.ListView == null;
            var editActive = item.Selected && !removed;
            var isMachineWide = GetItemPackageSource(item).IsMachineWide;
            if (item.Text == NuGetSourceName || isMachineWide)
            {
                editActive = !editActive;
            }

            nameTextBox.Enabled = editActive;
            sourceTextBox.Enabled = editActive;
            removeButton.Enabled = editActive;
            sourceEditorButton.Enabled = editActive;
            if (item.Selected)
            {
                nameTextBox.Text = item.SubItems[0].Text;
                sourceTextBox.Text = item.SubItems[1].Text;
                moveUpButton.Enabled = !isMachineWide && !removed && item.Index > 0;
                moveDownButton.Enabled = !isMachineWide && !removed && item.Index < item.ListView.Items.Count - 1;
            }
            else
            {
                nameTextBox.Clear();
                sourceTextBox.Clear();
                moveUpButton.Enabled = false;
                moveDownButton.Enabled = false;
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var sourceName = DefaultPackageSourceName;
            var source = DefaultPackageSource;
            for (int i = 1; packageSourceListView.Items.ContainsKey(sourceName); i++)
            {
                sourceName = string.Format("{0} {1}", DefaultPackageSourceName, i);
                source = DefaultPackageSource + i;
            }

            var imageIndex = GetStateImageIndex(true);
            var item = new ListViewItem(new[] { sourceName, source }, imageIndex);
            packageSourceListView.Items.Add(item);
            item.Checked = true;
            item.Tag = new PackageSource(source, sourceName, true);
            item.Selected = true;
            item.Focused = true;
            packageSourceListView.EnsureVisible(item.Index);
            packageSourceListView.Select();
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (packageSourceListView.SelectedItems.Count > 0)
            {
                var selectedItem = packageSourceListView.SelectedItems[0];
                packageSourceListView.Items.Remove(selectedItem);
                nameTextBox.Clear();
                sourceTextBox.Clear();
            }
        }

        private void moveUpButton_Click(object sender, EventArgs e)
        {
            if (packageSourceListView.SelectedItems.Count > 0)
            {
                OffsetItemIndex(packageSourceListView.SelectedItems[0], -1);
            }
        }

        private void moveDownButton_Click(object sender, EventArgs e)
        {
            if (packageSourceListView.SelectedItems.Count > 0)
            {
                OffsetItemIndex(packageSourceListView.SelectedItems[0], +1);
            }
        }

        private void packageSourceListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            UpdateItemActionButtons(e.Item);
        }

        private void UpdatePackageSource(object sender, EventArgs e)
        {
            if (packageSourceListView.SelectedItems.Count > 0)
            {
                var selectedItem = packageSourceListView.SelectedItems[0];
                selectedItem.SubItems[0].Text = nameTextBox.Text;
                selectedItem.SubItems[1].Text = sourceTextBox.Text;
                selectedItem.Tag = new PackageSource(sourceTextBox.Text, nameTextBox.Text, selectedItem.Checked);
            }
        }

        private void sourceEditorButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                sourceTextBox.Text = folderBrowserDialog.SelectedPath;
                UpdatePackageSource(sender, e);
            }
        }

        private void packageSourceListView_MouseClick(object sender, MouseEventArgs e)
        {
            var selectedListView = (ListView)sender;
            var hit = selectedListView.HitTest(e.Location);
            if (hit.Item != null && hit.Location == ListViewHitTestLocations.Image)
            {
                var item = hit.Item;
                ToggleItemChecked(item);
            }
        }

        private void packageSourceListView_Enter(object sender, EventArgs e)
        {
            var selectedListView = (ListView)sender;
            addButton.Enabled = selectedListView == packageSourceListView;
            if (selectedListView.SelectedItems.Count > 0)
            {
                UpdateItemActionButtons(selectedListView.SelectedItems[0]);
            }
        }

        private void packageSourceListView_KeyPress(object sender, KeyPressEventArgs e)
        {
            var selectedListView = (ListView)sender;
            if (selectedListView.SelectedItems.Count > 0)
            {
                ToggleItemChecked(selectedListView.SelectedItems[0]);
            }
        }
    }
}
