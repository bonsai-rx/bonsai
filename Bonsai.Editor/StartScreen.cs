using Bonsai.Design;
using Bonsai.Editor.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    public partial class StartScreen : Form
    {
        bool tabSelect;
        Font recentFileNameFont;
        Font recentFilePathFont;
        TreeNode newProjectNode = new TreeNode("New Project", 0, 0);
        TreeNode openProjectNode = new TreeNode("Open Project", 1, 1);
        TreeNode galleryNode = new TreeNode("Bonsai Gallery", 2, 2);
        TreeNode packageManagerNode = new TreeNode("Manage Packages", 3, 3);
        TreeNode documentationNode = new TreeNode("Documentation");
        TreeNode forumNode = new TreeNode("Forums");

        public StartScreen()
        {
            InitializeComponent();
            getStartedTreeView.Nodes.Add(documentationNode);
            getStartedTreeView.Nodes.Add(forumNode);
            openTreeView.Nodes.Add(newProjectNode);
            openTreeView.Nodes.Add(openProjectNode);
            openTreeView.Nodes.Add(galleryNode);
            openTreeView.Nodes.Add(packageManagerNode);
        }

        public EditorResult EditorResult { get; private set; }

        public string FileName
        {
            get { return openWorkflowDialog.FileName; }
        }

        private void StartBrowser(string url)
        {
            Uri result;
            var validUrl = Uri.TryCreate(url, UriKind.Absolute, out result) &&
                (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
            if (!validUrl)
            {
                throw new ArgumentException("The URL is malformed.");
            }

            try
            {
                Cursor = Cursors.AppStarting;
                Process.Start(url);
            }
            catch { } //best effort
            finally
            {
                Cursor = null;
            }
        }

        private void ActivateNode(TreeNode node)
        {
            if (node == documentationNode) StartBrowser("http://bonsai-rx.org/docs/editor/");
            if (node == forumNode) StartBrowser("https://groups.google.com/forum/#!forum/bonsai-users");
            if (node == newProjectNode) EditorResult = EditorResult.ReloadEditor;
            if (node == galleryNode) EditorResult = EditorResult.OpenGallery;
            if (node == packageManagerNode) EditorResult = EditorResult.ManagePackages;
            if (node == openProjectNode && openWorkflowDialog.ShowDialog() == DialogResult.OK)
            {
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
                    openWorkflowDialog.FileName = path;
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
            recentFilePathFont = recentFileView.Font;
            recentFileNameFont = new Font(recentFilePathFont.FontFamily, recentFilePathFont.SizeInPoints + 1, FontStyle.Bold);

            var recentlyUsedFiles = EditorSettings.Instance.RecentlyUsedFiles;
            foreach (var file in recentlyUsedFiles)
            {
                recentFileView.Nodes.Add(Path.GetFileName(file.FileName), file.FileName);
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
            var resources = new ComponentResourceManager(typeof(StartScreen));
            iconList.Images.Add((Image)(resources.GetObject("newToolStripMenuItem.Image")));
            iconList.Images.Add((Image)(resources.GetObject("openToolStripMenuItem.Image")));
            iconList.Images.Add((Image)(resources.GetObject("galleryToolStripMenuItem.Image")));
            iconList.Images.Add((Image)(resources.GetObject("packageManagerToolStripMenuItem.Image")));
            base.ScaleControl(factor, specified);
        }

        protected override bool ProcessTabKey(bool forward)
        {
            var treeView = ActiveControl as TreeView;
            if (treeView != null)
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
            var font = node.NodeFont ?? node.TreeView.Font;
            var hot = (e.State & TreeNodeStates.Hot) == TreeNodeStates.Hot;
            var selected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
            var hotColor = node.TreeView.ForeColor == SystemColors.HotTrack ? SystemColors.ActiveCaption : SystemColors.HotTrack;
            var color = hot ? hotColor : node.TreeView.ForeColor;
            e.Graphics.FillRectangle(SystemBrushes.Window, bounds);
            TextRenderer.DrawText(e.Graphics, node.Text, font, bounds, color, TextFormatFlags.NoClipping);
            if (selected && tabSelect) ControlPaint.DrawFocusRectangle(e.Graphics, bounds);
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
                treeView.SelectedNode = null;
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
