using Bonsai.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Design
{
    class NestedEditorDialog : TypeVisualizerDialog
    {
        IWorkflowEditorService editorService;

        public NestedEditorDialog(IServiceProvider provider)
        {
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override bool ProcessTabKey(bool forward)
        {
            var selected = SelectNextControl(ActiveControl, forward, true, true, false);
            if (!selected)
            {
                var parent = Parent;
                if (parent != null) return parent.SelectNextControl(this, forward, true, true, false);
                else editorService.SelectNextControl(forward);
            }

            return selected;
        }
    }
}
