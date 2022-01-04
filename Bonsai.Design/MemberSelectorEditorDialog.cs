using System;
using System.Windows.Forms;

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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            controller.Dispose();
            base.OnFormClosed(e);
        }
    }
}
