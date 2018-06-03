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
    public partial class MemberSelectorEditorDialog : Form, IMemberSelectorEditorDialog
    {
        readonly MemberSelectorEditorController controller;

        public MemberSelectorEditorDialog(Type type)
        {
            InitializeComponent();
            controller = new MemberSelectorEditorController(treeView, type);
        }

        public string Selector
        {
            get
            {
                return controller.GetSelectedMember();
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var memberChain = value.Split(new[] { ExpressionHelper.MemberSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    controller.InitializeSelection(memberChain);
                }
                else
                {
                    if (treeView.Nodes.Count == 1) treeView.ExpandAll();
                    else treeView.CollapseAll();
                    treeView.SelectedNode = null;
                }
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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            controller.Dispose();
            base.OnFormClosed(e);
        }
    }
}
