using Bonsai.Shaders.Design;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Bonsai.Shaders.Configuration.Design
{
    [Obsolete]
    partial class ShaderConfigurationEditorDialog : Form
    {
        int initialHeight;
        int initialCollectionEditorHeight;
        GlslScriptEditorDialog glslEditor;
        FormClosingEventArgs closingEventArgs;
        ShaderConfigurationEditorPage selectedPage;
        ShaderWindowSettings configuration;
        string currentDirectory;

        public ShaderConfigurationEditorDialog()
        {
            InitializeComponent();
            glslEditor = new GlslScriptEditorDialog();
            glslEditor.FormClosing += glslEditor_FormClosing;
            shaderButton.Tag = shaderCollectionEditor;
            meshButton.Tag = meshCollectionEditor;
            textureButton.Tag = textureCollectionEditor;
            shaderCollectionEditor.CollectionItemType = typeof(ShaderConfiguration);
            shaderCollectionEditor.NewItemTypes = new[] { typeof(MaterialConfiguration), typeof(ViewportEffectConfiguration), typeof(ComputeProgramConfiguration) };
            meshCollectionEditor.CollectionItemType = typeof(MeshConfiguration);
            meshCollectionEditor.NewItemTypes = new[] { typeof(MeshConfiguration), typeof(TexturedQuad), typeof(TexturedModel) };
            textureCollectionEditor.CollectionItemType = typeof(TextureConfiguration);
            textureCollectionEditor.NewItemTypes = new[] { typeof(Texture2D), typeof(Cubemap), typeof(ImageTexture), typeof(ImageCubemap) };
        }

        public GlslScriptExampleCollection ScriptExamples
        {
            get { return glslEditor.ScriptExamples; }
        }

        public ShaderConfigurationEditorPage SelectedPage
        {
            get { return selectedPage; }
            set
            {
                selectedPage = value;
                if (configuration != null)
                {
                    UpdateSelectedPage();
                }
            }
        }

        string CurrentDirectory
        {
            get { return currentDirectory; }
            set
            {
                currentDirectory = value;
                glslEditor.InitialDirectory = currentDirectory;
            }
        }

        void UpdateSelectedPage()
        {
            switch (selectedPage)
            {
                case ShaderConfigurationEditorPage.Meshes:
                    meshButton.Checked = true;
                    break;
                case ShaderConfigurationEditorPage.Textures:
                    textureButton.Checked = true;
                    break;
                case ShaderConfigurationEditorPage.Shaders:
                    shaderButton.Checked = true;
                    break;
                default:
                case ShaderConfigurationEditorPage.Window:
                    windowButton.Checked = true;
                    break;
            }
        }

        void ShowReadError(string message)
        {
            message = string.Format(Resources.ConfigurationReadError_Message, message);
            MessageBox.Show(this, message, Resources.ConfigurationReadError_Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnLoad(EventArgs e)
        {
            var loadResult = DialogResult.Cancel;
            CurrentDirectory = Environment.CurrentDirectory;
            try { configuration = ConfigurationHelper.LoadConfiguration(out loadResult); }
            catch (SecurityException ex) { ShowReadError(ex.Message); }
            catch (IOException ex) { ShowReadError(ex.Message); }
            catch (XmlException ex) { ShowReadError(ex.Message); }
            catch (InvalidOperationException ex)
            {
                ShowReadError(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }

            if (loadResult == DialogResult.Cancel)
            {
                Close();
                return;
            }

            if (Owner != null)
            {
                glslEditor.Icon = Icon = Owner.Icon;
                glslEditor.ShowIcon = ShowIcon = true;
            }

            initialHeight = Height;
            shaderCollectionEditor.Items = configuration.Shaders;
            meshCollectionEditor.Items = configuration.Meshes;
            textureCollectionEditor.Items = configuration.Textures;
            initialCollectionEditorHeight = shaderCollectionEditor.Height;
            UpdateSelectedPage();
            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (initialHeight > 0)
            {
                var expansion = Height - initialHeight;
                shaderCollectionEditor.Height = initialCollectionEditorHeight + expansion;
                meshCollectionEditor.Height = initialCollectionEditorHeight + expansion;
                textureCollectionEditor.Height = initialCollectionEditorHeight + expansion;
            }
            base.OnResize(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            closingEventArgs = e;
            try { glslEditor.Close(); }
            finally { closingEventArgs = null; }
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            propertyGrid.SelectedObject = null;
            base.OnFormClosed(e);
        }

        void RefreshCollectionEditor(CollectionEditorControl collectionEditor)
        {
            propertyGrid.SelectedObjects = collectionEditor.Visible
                ? collectionEditor.SelectedItems.Cast<object>().ToArray()
                : null;
        }

        void SetCollectionItems(CollectionEditorControl collectionEditor, IList collection)
        {
            collection.Clear();
            foreach (var item in collectionEditor.Items)
            {
                collection.Add(item);
            }
        }

        private void collectionEditor_SelectedItemChanged(object sender, EventArgs e)
        {
            var collectionEditor = (CollectionEditorControl)sender;
            RefreshCollectionEditor(collectionEditor);
        }

        private void windowButton_CheckedChanged(object sender, EventArgs e)
        {
            propertyGrid.SelectedObject = windowButton.Checked ? configuration : null;
        }

        private void shaderButton_CheckedChanged(object sender, EventArgs e)
        {
            var radioButton = (RadioButton)sender;
            var collectionEditor = (CollectionEditorControl)radioButton.Tag;
            collectionEditor.Visible = radioButton.Checked;
            RefreshCollectionEditor(collectionEditor);
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (shaderCollectionEditor.Visible) shaderCollectionEditor.Refresh();
            if (meshCollectionEditor.Visible) meshCollectionEditor.Refresh();
            if (textureCollectionEditor.Visible) textureCollectionEditor.Refresh();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentDirectory != Environment.CurrentDirectory)
            {
                if (MessageBox.Show(this,
                    Resources.ConfigurationDirectoryChanged_Message,
                    Resources.ConfigurationDirectoryChanged_Caption,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }

                CurrentDirectory = Environment.CurrentDirectory;
            }

            SetCollectionItems(shaderCollectionEditor, configuration.Shaders);
            SetCollectionItems(meshCollectionEditor, configuration.Meshes);
            SetCollectionItems(textureCollectionEditor, configuration.Textures);
            ShaderManager.SaveConfiguration(configuration);
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (shaderCollectionEditor.Visible) shaderCollectionEditor.Cut();
            if (meshCollectionEditor.Visible) meshCollectionEditor.Cut();
            if (textureCollectionEditor.Visible) textureCollectionEditor.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (shaderCollectionEditor.Visible) shaderCollectionEditor.Copy();
            if (meshCollectionEditor.Visible) meshCollectionEditor.Copy();
            if (textureCollectionEditor.Visible) textureCollectionEditor.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (shaderCollectionEditor.Visible) shaderCollectionEditor.Paste();
            if (meshCollectionEditor.Visible) meshCollectionEditor.Paste();
            if (textureCollectionEditor.Visible) textureCollectionEditor.Paste();
        }

        private void glslScriptEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (glslEditor.Visible)
            {
                if (glslEditor.WindowState == FormWindowState.Minimized)
                {
                    glslEditor.WindowState = FormWindowState.Normal;
                }
                glslEditor.Activate();
            }
            else
            {
                var owner = Owner ?? this;
                glslEditor.Show(owner);
            }
        }

        void glslEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (closingEventArgs != null) closingEventArgs.Cancel = e.Cancel;
            else if (!e.Cancel && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                glslEditor.Hide();
            }
        }
    }
}
