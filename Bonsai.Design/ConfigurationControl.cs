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

namespace Bonsai.Design
{
    public abstract partial class ConfigurationControl : UserControl
    {
        ConfigurationEditorService editorService;

        public ConfigurationControl()
        {
            InitializeComponent();
            editorService = new ConfigurationEditorService(this);

            SuspendLayout();
            foreach (var configurationName in GetConfigurationNames())
            {
                if (!string.IsNullOrWhiteSpace(configurationName))
                {
                    configurationNameListbox.Items.Add(configurationName);
                }
            }

            configurationNameListbox.Height = configurationNameListbox.ItemHeight * (configurationNameListbox.Items.Count + 0);
            Height = configurationNameListbox.PreferredHeight + configurationManagerButton.Height;
            ResumeLayout();
        }

        public object SelectedValue
        {
            get { return configurationNameListbox.SelectedItem; }
            set { configurationNameListbox.SelectedItem = value; }
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

        protected abstract IEnumerable<string> GetConfigurationNames();

        protected abstract object LoadConfiguration();

        protected abstract void SaveConfiguration(object configuration);

        protected abstract CollectionEditor CreateConfigurationEditor(Type type);

        private void configurationManagerButton_Click(object sender, EventArgs e)
        {
            var configuration = LoadConfiguration();
            if (configuration == null)
            {
                throw new InvalidOperationException("Failed to load configuration.");
            }

            var configurationManager = CreateConfigurationEditor(configuration.GetType());
            configurationManager.EditValue(editorService, editorService, configuration);
            if (editorService.DialogResult == DialogResult.OK)
            {
                SaveConfiguration(configuration);
            }
        }

        private void configurationNameListbox_SelectedValueChanged(object sender, EventArgs e)
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
    }
}
