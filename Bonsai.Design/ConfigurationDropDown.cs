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
using System.Drawing.Design;

namespace Bonsai.Design
{
    [Obsolete]
    public abstract partial class ConfigurationDropDown : UserControl
    {
        const float DefaultDpi = 96f;
        ConfigurationEditorService editorService;

        public ConfigurationDropDown()
            : this(null)
        {
        }

        public ConfigurationDropDown(IServiceProvider provider)
        {
            InitializeComponent();
            BackColor = SystemColors.Window;
            editorService = new ConfigurationEditorService(this, provider);

            using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                SuspendLayout();
                foreach (var configurationName in GetConfigurationNames())
                {
                    if (!string.IsNullOrWhiteSpace(configurationName))
                    {
                        configurationNameListbox.Items.Add(configurationName);
                    }
                }

                var drawScale = graphics.DpiY / DefaultDpi;
                configurationNameListbox.Height = (int)Math.Ceiling(configurationNameListbox.ItemHeight * configurationNameListbox.Items.Count * drawScale);
                Height = configurationNameListbox.Height + configurationManagerButton.Height;
                ResumeLayout();
            }
        }

        public override string Text
        {
            get { return configurationManagerButton.Text; }
            set { configurationManagerButton.Text = value; }
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

        protected abstract UITypeEditor CreateConfigurationEditor(Type type);

        private void configurationManagerButton_Click(object sender, EventArgs e)
        {
            var configuration = LoadConfiguration();
            var configurationType = configuration != null ? configuration.GetType() : null;
            var configurationManager = CreateConfigurationEditor(configurationType);
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
