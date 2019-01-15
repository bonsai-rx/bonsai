using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Design
{
    class MemberSelectorEditorController : IDisposable
    {
        TreeNode root;
        TreeView treeView;
        CSharpCodeProvider provider;
        const string IndexBegin = "[";
        const string RootLabel = "Source";

        public MemberSelectorEditorController(TreeView model, Type type)
        {
            treeView = model;
            provider = new CSharpCodeProvider();
            root = EnsureNode(model.Nodes, RootLabel, type);
            treeView.BeforeExpand += treeView_BeforeExpand;
        }

        internal void InitializeSelection(IEnumerable<string> memberChain)
        {
            var nodes = root.Nodes;
            foreach (var memberName in memberChain)
            {
                var nodeName = memberName;
                var indexBegin = nodeName.IndexOf(IndexBegin);
                if (indexBegin >= 0) nodeName = nodeName.Substring(0, indexBegin);
                var node = nodes[nodeName];
                if (node == null)
                {
                    treeView.SelectedNode = node;
                    break;
                }

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
            var typeRef = new CodeTypeReference(type);
            var node = nodes.Add(name, name + string.Format(" ({0})", provider.GetTypeOutput(typeRef)));
            node.Tag = type;
            return node;
        }

        internal TreeNode EnsureNode(TreeNodeCollection nodes, string name, Type type, bool recurse = true)
        {
            var node = nodes[name];
            if (node == null)
            {
                node = CreateNode(nodes, name, type);
            }

            if (node.Nodes.Count == 0 && recurse)
            {
                InitializeDummyMembers(node.Nodes, type);
            }

            return node;
        }

        static IEnumerable<PropertyInfo> GetProperties(Type type, BindingFlags bindingAttr)
        {
            var properties = type.GetProperties(bindingAttr).Except(type.GetDefaultMembers().OfType<PropertyInfo>());
            if (type.IsInterface)
            {
                properties = properties.Concat(type
                    .GetInterfaces()
                    .SelectMany(i => i.GetProperties(bindingAttr)));
            }
            return properties;
        }

        internal void InitializeMemberTree(TreeNodeCollection nodes, Type componentType)
        {
            if (componentType == null)
            {
                throw new ArgumentNullException("componentType");
            }

            foreach (var field in componentType.GetFields(BindingFlags.Instance | BindingFlags.Public)
                                               .OrderBy(member => member.MetadataToken))
            {
                EnsureNode(nodes, field.Name, field.FieldType);
            }

            foreach (var property in GetProperties(componentType, BindingFlags.Instance | BindingFlags.Public)
                                                  .OrderBy(member => member.MetadataToken))
            {
                EnsureNode(nodes, property.Name, property.PropertyType);
            }
        }

        void InitializeDummyMembers(TreeNodeCollection nodes, Type componentType)
        {
            foreach (var field in componentType.GetFields(BindingFlags.Instance | BindingFlags.Public)
                                               .OrderBy(member => member.MetadataToken))
            {
                EnsureNode(nodes, field.Name, field.FieldType, recurse: false);
            }

            foreach (var property in GetProperties(componentType, BindingFlags.Instance | BindingFlags.Public)
                                                  .OrderBy(member => member.MetadataToken))
            {
                EnsureNode(nodes, property.Name, property.PropertyType, recurse: false);
            }
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            treeView.SuspendLayout();
            ExpandNode(e.Node);
            treeView.ResumeLayout();
        }

        internal string GetSelectedMember()
        {
            var memberChain = GetSelectedMembers().Reverse();
            return string.Join(ExpressionHelper.MemberSeparator, memberChain.ToArray());
        }

        IEnumerable<string> GetSelectedMembers()
        {
            var node = treeView.SelectedNode;
            while (node != null && node != root)
            {
                yield return node.Name;
                node = node.Parent;
            }
        }

        void Dispose(bool disposing)
        {
            if (provider != null && disposing)
            {
                provider.Dispose();
                provider = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
