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
            DialogResult = DialogResult.None;
            var acceptButton = dialog.AcceptButton as Button;
            if (acceptButton != null)
            {
                acceptButton.Click += acceptButton_Click;
            }

            var result = dialog.ShowDialog(ownerControl);
            if (acceptButton != null)
            {
                acceptButton.Click -= acceptButton_Click;
            }

            return DialogResult == DialogResult.None ? result : DialogResult;
        }

        void acceptButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
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
