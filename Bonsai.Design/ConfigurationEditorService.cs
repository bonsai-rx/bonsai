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
        IServiceProvider parentProvider;

        public ConfigurationEditorService(Control owner, IServiceProvider provider)
        {
            ownerControl = owner;
            parentProvider = provider;
        }

        public DialogResult DialogResult { get; private set; }

        public void CloseDropDown()
        {
            throw new NotSupportedException();
        }

        public void DropDownControl(Control control)
        {
            throw new NotSupportedException();
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

            if (parentProvider != null)
            {
                return parentProvider.GetService(serviceType);
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
