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

namespace Bonsai.Design
{
    public partial class MemberSelectorEditorDialog : Form
    {
        public MemberSelectorEditorDialog(Type componentType, string selector)
        {
            if (componentType == null)
            {
                throw new ArgumentNullException("componentType");
            }

            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            InitializeComponent();
            InitializeMemberTree(treeView.Nodes, componentType);
            InitializeSelection(selector.Split(new[] { ExpressionHelper.MemberSeparator }, StringSplitOptions.RemoveEmptyEntries));
            Text = string.Format("{0} {1}", componentType.Name, Text);
        }

        void InitializeSelection(IEnumerable<string> memberChain)
        {
            var nodes = treeView.Nodes;
            foreach (var memberName in memberChain)
            {
                var node = nodes[memberName];
                ExpandNode(node);
                nodes = node.Nodes;
                treeView.SelectedNode = node;
            }
        }

        void ExpandNode(TreeNode node)
        {
            if (node.Nodes.Count > 0)
            {
                InitializeMemberTree(node.Nodes, (Type)node.Tag);
            }
        }

        TreeNode EnsureNode(TreeNodeCollection nodes, string name, Type type)
        {
            var node = nodes[name];
            if (node == null)
            {
                node = nodes.Add(name, name);
                node.Tag = type;
            }

            if (node.Nodes.Count == 0)
            {
                InitializeDummyMembers(node.Nodes, type);
            }

            return node;
        }

        void InitializeMemberTree(TreeNodeCollection nodes, Type componentType)
        {
            if (componentType == null)
            {
                throw new ArgumentNullException("componentType");
            }

            foreach (var field in componentType.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                EnsureNode(nodes, field.Name, field.FieldType);
            }

            foreach (var property in componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                EnsureNode(nodes, property.Name, property.PropertyType);
            }
        }

        void InitializeDummyMembers(TreeNodeCollection nodes, Type componentType)
        {
            foreach (var field in componentType.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var node = nodes.Add(field.Name, field.Name);
                node.Tag = field.FieldType;
            }

            foreach (var property in componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var node = nodes.Add(property.Name, property.Name);
                node.Tag = property.PropertyType;
            }
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            treeView.SuspendLayout();
            ExpandNode(e.Node);
            treeView.ResumeLayout();
        }

        IEnumerable<string> GetSelectedMembers()
        {
            var node = treeView.SelectedNode;
            while (node != null)
            {
                yield return node.Text;
                node = node.Parent;
            }
        }

        public IEnumerable<string> GetMemberChain()
        {
            return GetSelectedMembers().Reverse();
        }
    }
}
