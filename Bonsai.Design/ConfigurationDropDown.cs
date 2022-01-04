using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Design;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides an abstract base class for drop-down editor controls
    /// listing custom configuration objects.
    /// </summary>
    [Obsolete]
    public abstract partial class ConfigurationDropDown : UserControl
    {
        const float DefaultDpi = 96f;
        readonly ConfigurationEditorService editorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationDropDown"/> class.
        /// </summary>
        public ConfigurationDropDown()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationDropDown"/> class
        /// using the specified service provider.
        /// </summary>
        /// <param name="provider">
        /// A service provider object through which editing services can be obtained.
        /// </param>
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

        /// <inheritdoc/>
        public override string Text
        {
            get { return configurationManagerButton.Text; }
            set { configurationManagerButton.Text = value; }
        }

        /// <summary>
        /// Gets or sets the currently selected value in the drop-down.
        /// </summary>
        public object SelectedValue
        {
            get { return configurationNameListbox.SelectedItem; }
            set { configurationNameListbox.SelectedItem = value; }
        }

        /// <summary>
        /// Occurs when the selected value changes.
        /// </summary>
        public event EventHandler SelectedValueChanged;

        private void OnSelectedValueChanged(EventArgs e)
        {
            SelectedValueChanged?.Invoke(this, e);
        }

        /// <summary>
        /// When overridden in a derived class, gets the collection of available configuration
        /// names to list in the drop-down.
        /// </summary>
        /// <returns>
        /// An enumerable collection of <see cref="string"/> values representing the
        /// available configurations.
        /// </returns>
        protected abstract IEnumerable<string> GetConfigurationNames();

        /// <summary>
        /// When overridden in a derived class, loads the custom configuration object
        /// storing the current settings for the editor control.
        /// </summary>
        /// <returns>
        /// The restored custom configuration object.
        /// </returns>
        protected abstract object LoadConfiguration();

        /// <summary>
        /// When overridden in a derived class, saves the current configuration object
        /// storing the current settings configured by the editor control.
        /// </summary>
        /// <param name="configuration">
        /// The configuration object to be stored.
        /// </param>
        protected abstract void SaveConfiguration(object configuration);

        /// <summary>
        /// When overridden in a derived class, creates the custom UI type editor
        /// for the specified type.
        /// </summary>
        /// <param name="type">
        /// The type of values representing the custom configuration objects.
        /// </param>
        /// <returns>
        /// An instance of the <see cref="UITypeEditor"/> class used to design
        /// value editors for the specified type.
        /// </returns>
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
