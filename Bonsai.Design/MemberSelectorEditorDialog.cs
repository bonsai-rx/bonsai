using System;
using System.Windows.Forms;

namespace Bonsai.Design
{
    /// <summary>
    /// Represents a dialog for selecting members of a workflow expression type.
    /// </summary>
    public partial class MemberSelectorEditorDialog : Form, IMemberSelectorEditorDialog
    {
        readonly MemberSelectorEditorController controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberSelectorEditorDialog"/> class
        /// using the specified type.
        /// </summary>
        /// <param name="type">The type from which to select an inner property.</param>
        public MemberSelectorEditorDialog(Type type)
        {
            InitializeComponent();
            controller = new MemberSelectorEditorController(treeView, type);
        }

        /// <summary>
        /// Gets or sets the selected inner property of the expression type.
        /// </summary>
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

        /// <inheritdoc/>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            controller.Dispose();
            base.OnFormClosed(e);
        }
    }
}
