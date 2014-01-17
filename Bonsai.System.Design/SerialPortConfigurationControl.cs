using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Design;
using System.Collections.ObjectModel;
using System.Windows.Forms.Design;
using System.IO.Ports;
using Bonsai.Design;

namespace Bonsai.IO.Design
{
    public partial class SerialPortConfigurationControl : UserControl
    {
        ConfigurationEditorService editorService;

        public SerialPortConfigurationControl()
        {
            InitializeComponent();
            editorService = new ConfigurationEditorService(this);

            SuspendLayout();
            foreach (var portName in SerialPort.GetPortNames())
            {
                portNameListbox.Items.Add(portName);
            }

            portNameListbox.Height = portNameListbox.ItemHeight * (portNameListbox.Items.Count + 0);
            Height = portNameListbox.PreferredHeight + configurationManagerButton.Height;
            ResumeLayout();
        }

        public object SelectedValue
        {
            get { return portNameListbox.SelectedItem; }
            set { portNameListbox.SelectedItem = value; }
        }

        public event EventHandler SelectedValueChanged;

        private void OnSelectedValueChanged(EventArgs e)
        {
            var handler = SelectedValueChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual object LoadConfiguration()
        {
            return SerialPortManager.LoadConfiguration();
        }

        protected virtual void SaveConfiguration(object configuration)
        {
            var serialPortConfiguration = configuration as SerialPortConfigurationCollection;
            if (serialPortConfiguration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            SerialPortManager.SaveConfiguration(serialPortConfiguration);
        }

        private void configurationManagerButton_Click(object sender, EventArgs e)
        {
            var configuration = LoadConfiguration();
            if (configuration == null)
            {
                throw new InvalidOperationException("Failed to load configuration.");
            }

            var configurationManager = new SerialPortConfigurationCollectionEditor(configuration.GetType());
            configurationManager.EditValue(editorService, editorService, configuration);
            if (editorService.DialogResult == DialogResult.OK)
            {
                SaveConfiguration(configuration);
            }
        }

        private void portNameListbox_SelectedValueChanged(object sender, EventArgs e)
        {
            OnSelectedValueChanged(e);
        }

        class FlatButton : Button
        {
            public override void NotifyDefault(bool value)
            {
                base.NotifyDefault(false);
            }
        }

        class SerialPortConfigurationCollectionEditor : DescriptiveCollectionEditor
        {
            public SerialPortConfigurationCollectionEditor(Type type)
                : base(type)
            {
            }

            protected override string GetDisplayText(object value)
            {
                var configuration = value as SerialPortConfiguration;
                if (configuration != null)
                {
                    if (!string.IsNullOrEmpty(configuration.PortName))
                    {
                        return configuration.PortName;
                    }

                    return typeof(SerialPortConfiguration).Name;
                }

                return base.GetDisplayText(value);
            }
        }

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
}
