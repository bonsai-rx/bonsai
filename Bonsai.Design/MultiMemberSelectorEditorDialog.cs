using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.ObjectModel;
using Microsoft.CSharp;
using System.CodeDom;

namespace Bonsai.Design
{
    public partial class MultiMemberSelectorEditorDialog : Form, IMemberSelectorEditorDialog
    {
        int mouseClicks;
        const int ButtonMargin = 2;
        readonly MemberSelectorEditorController controller;

        public MultiMemberSelectorEditorDialog(Type type)
        {
            InitializeComponent();
            controller = new MemberSelectorEditorController(treeView, type);
        }

        public string Selector
        {
            get
            {
                var selectedMembers = selectionListBox.Items.Cast<string>();
                return string.Join(ExpressionHelper.ArgumentSeparator, selectedMembers.ToArray());
            }
            set
            {
                var memberNames = ExpressionHelper.SelectMemberNames(value).ToArray();
                if (memberNames.Length > 0)
                {
                    foreach (var memberSelector in memberNames)
                    {
                        selectionListBox.Items.Add(memberSelector);
                    }

                    selectionListBox.SelectedIndex = selectionListBox.Items.Count - 1;
                }

                treeView.ExpandAll();
                treeView.SelectedNode = null;
            }
        }

        public void AddMember(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            controller.InitializeMemberTree(treeView.Nodes, type);
        }

        public void AddMember(string name, Type type)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            controller.EnsureNode(treeView.Nodes, name, type);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            selectionListBox.ItemHeight = (int)(13 * factor.Height);
            base.ScaleControl(factor, specified);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            controller.Dispose();
            base.OnFormClosed(e);
        }

        private void selectionListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                var buttonBounds = e.Bounds;
                var maxIndex = selectionListBox.Items.Count - 1;
                buttonBounds.Width = (int)e.Graphics.MeasureString(maxIndex.ToString(), e.Font).Width;
                buttonBounds.Width += 2 * ButtonMargin + 1;
                ControlPaint.DrawButton(e.Graphics, buttonBounds, ButtonState.Inactive);

                var indexWidth = (int)e.Graphics.MeasureString(e.Index.ToString(), e.Font).Width;
                buttonBounds.X = (buttonBounds.Width - indexWidth) / 2;
                e.Graphics.DrawString(e.Index.ToString(), e.Font, Brushes.Black, buttonBounds.Location);

                var itemBounds = e.Bounds;
                itemBounds.X += buttonBounds.Width;

                var itemBrush = Brushes.Black;
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    itemBrush = Brushes.White;
                }

                var itemText = selectionListBox.Items[e.Index].ToString();
                var itemExtent = (int)e.Graphics.MeasureString(itemText, e.Font).Width + buttonBounds.Width;
                selectionListBox.HorizontalExtent = Math.Max(selectionListBox.HorizontalExtent, itemExtent);
                e.Graphics.DrawString(itemText, e.Font, itemBrush, itemBounds, StringFormat.GenericDefault);
            }
            e.DrawFocusRectangle();
        }

        private void AddSelectedNode()
        {
            var memberSelector = controller.GetSelectedMember();
            if (string.IsNullOrEmpty(memberSelector)) memberSelector = ExpressionHelper.ImplicitParameterName;

            var index = selectionListBox.SelectedIndex + 1;
            selectionListBox.Items.Insert(index, memberSelector);
            selectionListBox.SelectedIndex = index;
        }

        private void RemoveSelectedItem()
        {
            if (selectionListBox.SelectedItem != null)
            {
                var selectedIndex = selectionListBox.SelectedIndex;
                selectionListBox.Items.RemoveAt(selectedIndex);
                selectionListBox.SelectedIndex = Math.Min(selectedIndex, selectionListBox.Items.Count - 1);
                selectionListBox.HorizontalExtent = 0;
            }
        }

        private void SwapSelectedIndex(int newIndex)
        {
            var selectedIndex = selectionListBox.SelectedIndex;
            if (newIndex >= 0 && newIndex < selectionListBox.Items.Count)
            {
                var temp = selectionListBox.Items[newIndex];
                selectionListBox.Items[newIndex] = selectionListBox.SelectedItem;
                selectionListBox.Items[selectedIndex] = temp;
                selectionListBox.SelectedIndex = newIndex;
            }
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            SwapSelectedIndex(selectionListBox.SelectedIndex - 1);
        }

        private void downButton_Click(object sender, EventArgs e)
        {
            SwapSelectedIndex(selectionListBox.SelectedIndex + 1);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            AddSelectedNode();
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            RemoveSelectedItem();
        }

        private void addAllButton_Click(object sender, EventArgs e)
        {
            if (treeView.Nodes.Count > 0 && treeView.SelectedNode != null)
            {
                var selectedNode = treeView.SelectedNode;
                var selectedMember = controller.GetSelectedMember();
                var index = selectionListBox.SelectedIndex;
                if (selectedNode.Nodes.Count > 0)
                {
                    selectionListBox.BeginUpdate();
                    foreach (TreeNode node in selectedNode.Nodes)
                    {
                        var memberSelector = !string.IsNullOrEmpty(selectedMember)
                            ? string.Join(ExpressionHelper.MemberSeparator, new[] { selectedMember, node.Name })
                            : node.Name;
                        selectionListBox.Items.Insert(++index, memberSelector);
                    }
                    selectionListBox.EndUpdate();
                }
                else if (!string.IsNullOrEmpty(selectedMember))
                {
                    selectionListBox.Items.Insert(++index, selectedMember);
                }
                selectionListBox.SelectedIndex = index;
            }
        }

        private void removeAllButton_Click(object sender, EventArgs e)
        {
            selectionListBox.Items.Clear();
            selectionListBox.HorizontalExtent = 0;
        }

        private void selectionListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var memberSelector = (string)selectionListBox.SelectedItem;
            if (memberSelector != null)
            {
                var memberChain = memberSelector.Split(new[] { ExpressionHelper.MemberSeparator }, StringSplitOptions.RemoveEmptyEntries);
                controller.InitializeSelection(memberChain);
            }
        }

        private void selectionListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.None)
            {
                RemoveSelectedItem();
            }
        }

        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                AddSelectedNode();
            }
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (mouseClicks > 1 &&
               (e.Action == TreeViewAction.Expand || e.Action == TreeViewAction.Collapse))
            {
                mouseClicks = 0;
                e.Cancel = true;
            }
        }

        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            mouseClicks = e.Clicks;
        }

        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && e.Modifiers == Keys.None)
            {
                AddSelectedNode();
            }
        }
    }
}
