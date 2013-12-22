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
        readonly PackageSourceProvider provider;

        public PackageSourceConfigurationDialog(PackageSourceProvider sourceProvider)
        {
            if (sourceProvider == null)
            {
                throw new ArgumentNullException("sourceProvider");
            }

            InitializeComponent();
            provider = sourceProvider;
        }

        protected override void OnLoad(EventArgs e)
        {
            foreach (var packageSource in provider.LoadPackageSources())
            {
                var item = packageSourceListView.Items.Add(packageSource.Name, packageSource.Source, 0);
                item.Checked = packageSource.IsEnabled;
                item.Tag = packageSource;
            }
            base.OnLoad(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                provider.SavePackageSources(from item in packageSourceListView.Items.Cast<ListViewItem>()
                                            select (PackageSource)item.Tag);
            }
            base.OnClosed(e);
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

            var item = packageSourceListView.Items.Add(sourceName, source, 0);
            item.Checked = true;
            item.Tag = new PackageSource(source, sourceName, true);
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (packageSourceListView.SelectedItems.Count > 0)
            {
                var selectedItem = packageSourceListView.SelectedItems[0];
                packageSourceListView.Items.Remove(selectedItem);
            }
        }

        private void moveUpButton_Click(object sender, EventArgs e)
        {
            if (packageSourceListView.SelectedItems.Count > 0)
            {
                var selectedItem = packageSourceListView.SelectedItems[0];
                var index = selectedItem.Index;
                selectedItem.Remove();
                packageSourceListView.Items.Insert(index - 1, selectedItem);
            }
        }

        private void moveDownButton_Click(object sender, EventArgs e)
        {
            if (packageSourceListView.SelectedItems.Count > 0)
            {
                var selectedItem = packageSourceListView.SelectedItems[0];
                var index = selectedItem.Index;
                selectedItem.Remove();
                packageSourceListView.Items.Insert(index + 1, selectedItem);
            }
        }

        private void packageSourceListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.Item.Name == NuGetSourceName)
            {
                nameTextBox.Enabled = !e.IsSelected;
                sourceTextBox.Enabled = !e.IsSelected;
                removeButton.Enabled = !e.IsSelected;
                updateButton.Enabled = !e.IsSelected;
                sourceEditorButton.Enabled = !e.IsSelected;
            }

            if (e.IsSelected)
            {
                nameTextBox.Text = e.Item.Name;
                sourceTextBox.Text = e.Item.Text;
                moveUpButton.Enabled = e.ItemIndex > 0;
                moveDownButton.Enabled = e.ItemIndex < packageSourceListView.Items.Count - 1;
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            if (packageSourceListView.SelectedItems.Count > 0)
            {
                var selectedItem = packageSourceListView.SelectedItems[0];
                selectedItem.Name = nameTextBox.Text;
                selectedItem.Text = sourceTextBox.Text;
                selectedItem.Tag = new PackageSource(selectedItem.Text, selectedItem.Name, selectedItem.Checked);
            }
        }

        private void sourceEditorButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                sourceTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void packageSourceListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Tag != null)
            {
                ((PackageSource)e.Item.Tag).IsEnabled = e.Item.Checked;
            }
        }
    }
}
