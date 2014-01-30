using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Bonsai.Design
{
    class ConfigurationEditorService : IWindowsFormsEditorService, IServiceProvider, ITypeDescriptorContext
    {
        Control ownerControl;

        public ConfigurationEditorService(Control owner)
        {
            ownerControl = owner;
        }

        public DialogResult DialogResult { get; private set; }

        public void CloseDropDown()
        {
        }

        public void DropDownControl(Control control)
        {
        }

        public DialogResult ShowDialog(Form dialog)
        {
            DialogResult = dialog.ShowDialog(ownerControl);
            return DialogResult;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IWindowsFormsEditorService))
            {
                return this;
            }

            return null;
        }

        public IContainer Container
        {
            get { return null; }
        }

        public object Instance
        {
            get { return null; }
        }

        public void OnComponentChanged()
        {
        }

        public bool OnComponentChanging()
        {
            return true;
        }

        public PropertyDescriptor PropertyDescriptor
        {
            get { return null; }
        }
    }
}
