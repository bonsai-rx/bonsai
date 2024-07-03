using Bonsai.Editor.Properties;
using Bonsai.Editor.Themes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    public partial class StartScreen : Form
    {
        bool tabSelect;
        readonly List<Image> customImages = new List<Image>();
        readonly ComponentResourceManager resources = new ComponentResourceManager(typeof(StartScreen));
        readonly TreeNode newFileNode = new TreeNode("New File", 0, 0);
        readonly TreeNode openFileNode = new TreeNode("Open File", 1, 1);
        readonly TreeNode openFolderNode = new TreeNode("Open Folder", 2, 2);
        readonly TreeNode galleryNode = new TreeNode("Bonsai Gallery", 3, 3);
        readonly TreeNode packageManagerNode = new TreeNode("Manage Packages", 4, 4);
        readonly TreeNode documentationNode = new TreeNode("Documentation");
        readonly TreeNode forumNode = new TreeNode("Forums");

        public StartScreen()
        {
            InitializeComponent();
            getStartedTreeView.Nodes.Add(documentationNode);
            getStartedTreeView.Nodes.Add(forumNode);
            openTreeView.Nodes.Add(newFileNode);
            openTreeView.Nodes.Add(openFileNode);
            if (!EditorSettings.IsRunningOnMono)
            {
                openTreeView.Nodes.Add(openFolderNode);
            }
            openTreeView.Nodes.Add(galleryNode);
            openTreeView.Nodes.Add(packageManagerNode);
            FileName = string.Empty;
            Text += AboutBox.BuildKindTitleSuffix;
        }

        public EditorResult EditorResult { get; private set; }

        public string FileName { get; private set; }

        private void ActivateNode(TreeNode node)
        {
            if (node == documentationNode) EditorDialog.ShowDocs();
            if (node == forumNode) EditorDialog.ShowForum();
            if (node == newFileNode) EditorResult = EditorResult.ReloadEditor;
            if (node == galleryNode) EditorResult = EditorResult.OpenGallery;
            if (node == packageManagerNode) EditorResult = EditorResult.ManagePackages;
            if (node == openFileNode && openWorkflowDialog.ShowDialog() == DialogResult.OK ||
                node == openFolderNode && openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                FileName = node == openFileNode ? openWorkflowDialog.FileName : openFolderDialog.SelectedPath;
                EditorResult = EditorResult.ReloadEditor;
            }

            if (node.TreeView == recentFileView)
            {
                var path = node.Text;
                if (!File.Exists(path))
                {
                    var result = MessageBox.Show(
                        this,
                        string.Format(Resources.RemoveRecentFile_Question, node.Name),
                        Resources.RemoveRecentFile_Question_Caption,
                        MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        EditorSettings.Instance.RecentlyUsedFiles.Remove(path);
                        EditorSettings.Instance.Save();
                        node.Remove();
                    }
                }
                else
                {
                    FileName = path;
                    EditorResult = EditorResult.ReloadEditor;
                }
            }

            if (EditorResult != EditorResult.Exit)
            {
                Close();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            var recentlyUsedFiles = EditorSettings.Instance.RecentlyUsedFiles;
            foreach (var file in recentlyUsedFiles)
            {
                recentFileView.Nodes.Add(Path.GetFileName(file.FileName), file.FileName);
                if (string.IsNullOrEmpty(openWorkflowDialog.InitialDirectory))
                {
                    var initialDirectory = Path.GetDirectoryName(file.FileName);
                    openWorkflowDialog.InitialDirectory = initialDirectory;
                    openFolderDialog.SelectedPath = initialDirectory;
                }
            }

            openTreeView.Select();
            base.OnLoad(e);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            const int MaxImageSize = 32;
            var scale = factor.Height;
            var itemHeight = (int)(20 * scale);
            var viewMargin = new Padding(
                (int)(10 * scale + 1 * scale * scale * scale), 0,
                (int)(3 * scale), 0);
            getStartedTreeView.ItemHeight = itemHeight;
            getStartedTreeView.Margin = viewMargin;
            openTreeView.ItemHeight = itemHeight;
            openTreeView.Margin = viewMargin;
            iconList.Images.Clear();
            iconList.ImageSize = new Size(
                Math.Min(MaxImageSize, (int)(16 * factor.Height)),
                Math.Min(MaxImageSize, (int)(16 * factor.Height)));

            var newFileImage = (Image)resources.GetObject("newToolStripMenuItem.Image");
            var openFileImage = (Image)resources.GetObject("openFileToolStripMenuItem.Image");
            var openFolderImage = (Image)resources.GetObject("openToolStripMenuItem.Image");
            var galleryItemImage = (Image)(resources.GetObject("galleryToolStripMenuItem.Image"));
            var packageManagerItemImage = (Image)(resources.GetObject("packageManagerToolStripMenuItem.Image"));
            var editorTheme = EditorSettings.Instance.EditorTheme;
            if (editorTheme == ColorTheme.Dark)
            {
                var systemMenuColor = SystemColors.Menu;
                BackColor = ThemeHelper.Invert(systemMenuColor);
                getStartedLabel.ForeColor = ThemeHelper.Invert(getStartedLabel.ForeColor);
                openLabel.ForeColor = ThemeHelper.Invert(openLabel.ForeColor);
                recentLabel.ForeColor = ThemeHelper.Invert(recentLabel.ForeColor);
                getStartedTreeView.BackColor = ThemeHelper.Invert(systemMenuColor);
                getStartedTreeView.ForeColor = ThemeHelper.Invert(getStartedTreeView.ForeColor);
                openTreeView.BackColor = ThemeHelper.Invert(systemMenuColor);
                openTreeView.ForeColor = ThemeHelper.Invert(openTreeView.ForeColor);
                recentFileView.BackColor = ThemeHelper.Invert(systemMenuColor);
                recentFileView.ForeColor = ThemeHelper.Invert(recentFileView.ForeColor);
                recentFileView.LineColor = ThemeHelper.Invert(recentFileView.LineColor);
                customImages.Add(newFileImage = ThemeHelper.InvertScale(newFileImage, iconList.ImageSize));
                customImages.Add(openFileImage = ThemeHelper.InvertScale(openFileImage, iconList.ImageSize));
                customImages.Add(openFolderImage = ThemeHelper.InvertScale(openFolderImage, iconList.ImageSize));
                customImages.Add(galleryItemImage = ThemeHelper.InvertScale(galleryItemImage, iconList.ImageSize));
                customImages.Add(packageManagerItemImage = ThemeHelper.InvertScale(packageManagerItemImage, iconList.ImageSize));
            }

            iconList.Images.Add(newFileImage);
            iconList.Images.Add(openFileImage);
            iconList.Images.Add(openFolderImage);
            iconList.Images.Add(galleryItemImage);
            iconList.Images.Add(packageManagerItemImage);
            base.ScaleControl(factor, specified);
        }

        private void DisposeDrawResources()
        {
            foreach (var image in customImages)
            {
                image.Dispose();
            }

            customImages.Clear();
            resources.ReleaseAllResources();
        }

        protected override bool ProcessTabKey(bool forward)
        {
            if (ActiveControl is TreeView treeView)
            {
                var node = treeView.SelectedNode;
                if (!tabSelect && node != null)
                {
                    treeView.Invalidate(node.Bounds);
                    tabSelect = true;
                    return true;
                }

                if ((!tabSelect || node == null) && treeView.Nodes.Count > 0)
                {
                    treeView.SelectedNode = treeView.Nodes[forward ? 0 : treeView.Nodes.Count - 1];
                }
                else if (node != null)
                {
                    var index = node.Index + (forward ? 1 : -1);
                    if (forward && index < treeView.Nodes.Count ||
                        !forward && index >= 0)
                    {
                        treeView.SelectedNode = treeView.Nodes[index];
                    }
                }

                if (treeView.SelectedNode != node)
                {
                    return true;
                }
                else if (SelectNextControl(treeView, forward, true, true, true))
                {
                    treeView = ActiveControl as TreeView;
                    if (treeView != null && treeView.Nodes.Count > 0)
                    {
                        treeView.SelectedNode = treeView.Nodes[forward ? 0 : treeView.Nodes.Count - 1];
                    }
                    return true;
                }
                else return false;
            }

            return base.ProcessTabKey(forward);
        }

        private void treeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Action == TreeViewAction.ByKeyboard)
            {
                tabSelect = true;
            }
        }

        private void treeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            var node = e.Node;
            var bounds = node.Bounds;
            var treeView = node.TreeView;
            var font = node.NodeFont ?? node.TreeView.Font;
            var hot = (e.State & TreeNodeStates.Hot) == TreeNodeStates.Hot;
            var selected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
            var hotColor = node.TreeView.ForeColor == SystemColors.HotTrack ? SystemColors.ActiveCaption : SystemColors.HotTrack;
            var color = hot ? hotColor : treeView.ForeColor;
            using (var brush = new SolidBrush(treeView.BackColor))
            {
                e.Graphics.FillRectangle(brush, bounds);
            }
            TextRenderer.DrawText(e.Graphics, node.Text, font, bounds, color, TextFormatFlags.NoClipping);
            if (selected && tabSelect) ControlPaint.DrawFocusRectangle(e.Graphics, bounds, treeView.ForeColor, treeView.BackColor);
        }

        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            var treeView = (TreeView)sender;
            var node = treeView.SelectedNode;
            if (node != null && e.KeyCode == Keys.Return)
            {
                ActivateNode(node);
            }
        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var node = e.Node;
            if (node != null)
            {
                var treeView = node.TreeView;
                tabSelect = false;
                ActivateNode(node);
                if (!treeView.IsDisposed)
                {
                    treeView.SelectedNode = null;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }
    }
}
