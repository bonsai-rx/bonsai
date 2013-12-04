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
        public MemberSelectorEditorDialog()
        {
            InitializeComponent();
        }

        public string Selector
        {
            get
            {
                var memberChain = GetSelectedMembers().Reverse();
                return string.Join(ExpressionHelper.MemberSeparator, memberChain.ToArray());
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var memberChain = value.Split(new[] { ExpressionHelper.MemberSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    InitializeSelection(memberChain);
                }
                else
                {
                    treeView.CollapseAll();
                    treeView.SelectedNode = null;
                }
            }
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

            EnsureNode(treeView.Nodes, name, type);
        }

        void InitializeSelection(IEnumerable<string> memberChain)
        {
            var nodes = treeView.Nodes;
            foreach (var memberName in memberChain)
            {
                var nodeName = memberName;
                var indexBegin = nodeName.IndexOf(ExpressionHelper.IndexBegin);
                if (indexBegin >= 0) nodeName = nodeName.Substring(0, indexBegin);
                var node = nodes[nodeName];
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

        TreeNode CreateNode(TreeNodeCollection nodes, string name, Type type)
        {
            var node = nodes.Add(name, name + string.Format(" ({0})", type.FullName));
            node.Tag = type;
            return node;
        }

        TreeNode EnsureNode(TreeNodeCollection nodes, string name, Type type)
        {
            var node = nodes[name];
            if (node == null)
            {
                node = CreateNode(nodes, name, type);
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
                CreateNode(nodes, field.Name, field.FieldType);
            }

            foreach (var property in componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                CreateNode(nodes, property.Name, property.PropertyType);
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
                yield return node.Name;
                node = node.Parent;
            }
        }

        IEnumerable<string> GetMemberChain()
        {
            return GetSelectedMembers().Reverse();
        }
    }
}
