using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Bonsai.Editor.GraphView
{
    class WorkflowGraphViewEditorService : IWindowsFormsEditorService, IServiceProvider
    {
        Control ownerControl;
        IServiceProvider serviceProvider;

        internal WorkflowGraphViewEditorService(Control owner, IServiceProvider provider)
        {
            ownerControl = owner;
            serviceProvider = provider;
        }

        public void CloseDropDown()
        {
        }

        public void DropDownControl(Control control)
        {
        }

        public DialogResult ShowDialog(Form dialog)
        {
            return dialog.ShowDialog(ownerControl);
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IWindowsFormsEditorService))
            {
                return this;
            }

            if (serviceProvider != null)
            {
                return serviceProvider.GetService(serviceType);
            }

            return null;
        }
    }
}
