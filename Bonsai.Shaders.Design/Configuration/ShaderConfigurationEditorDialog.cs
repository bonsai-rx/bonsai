using Bonsai.Shaders.Design;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Shaders.Configuration.Design
{
    partial class ShaderConfigurationEditorDialog : Form
    {
        int initialHeight;
        int initialCollectionEditorHeight;
        GlslScriptEditorDialog glslEditor;
        FormClosingEventArgs closingEventArgs;
        ShaderConfigurationEditorPage selectedPage;
        ShaderWindowSettings configuration;

        public ShaderConfigurationEditorDialog()
        {
            InitializeComponent();
            shaderButton.Tag = shaderCollectionEditor;
            meshButton.Tag = meshCollectionEditor;
            textureButton.Tag = textureCollectionEditor;
            shaderCollectionEditor.CollectionItemType = typeof(ShaderConfiguration);
            shaderCollectionEditor.NewItemTypes = new[] { typeof(MaterialConfiguration), typeof(ComputeConfiguration) };
            meshCollectionEditor.CollectionItemType = typeof(MeshConfiguration);
            meshCollectionEditor.NewItemTypes = new[] { typeof(MeshConfiguration), typeof(TexturedQuad), typeof(TexturedModel) };
            textureCollectionEditor.CollectionItemType = typeof(TextureConfiguration);
            textureCollectionEditor.NewItemTypes = new[] { typeof(Texture2D), typeof(ImageTexture) };
        }

        public ShaderConfigurationEditorPage SelectedPage
        {
            get { return selectedPage; }
            set
            {
                selectedPage = value;
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
        }

        public ShaderWindowSettings Configuration
        {
            get { return configuration; }
            set
            {
                configuration = value;
                shaderCollectionEditor.Items = configuration.Shaders;
                meshCollectionEditor.Items = configuration.Meshes;
                textureCollectionEditor.Items = configuration.Textures;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (Owner != null)
            {
                Icon = Owner.Icon;
                ShowIcon = true;
            }

            initialHeight = Height;
            initialCollectionEditorHeight = shaderCollectionEditor.Height;
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
            try
            {
                if (glslEditor != null)
                {
                    glslEditor.Close();
                }
            }
            finally { closingEventArgs = null; }
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                SetCollectionItems(shaderCollectionEditor, configuration.Shaders);
                SetCollectionItems(meshCollectionEditor, configuration.Meshes);
                SetCollectionItems(textureCollectionEditor, configuration.Textures);
            }

            propertyGrid.SelectedObject = null;
            base.OnFormClosed(e);
        }

        void RefreshCollectionEditor(CollectionEditorControl collectionEditor)
        {
            propertyGrid.SelectedObject = collectionEditor.Visible ? collectionEditor.SelectedItem : null;
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
            propertyGrid.SelectedObject = windowButton.Checked ? Configuration : null;
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
            ShaderManager.SaveConfiguration(configuration);
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void glslScriptEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (glslEditor == null)
            {
                glslEditor = new GlslScriptEditorDialog();
                glslEditor.FormClosing += glslEditor_FormClosing;
                if (ShowIcon)
                {
                    glslEditor.Icon = Icon;
                    glslEditor.ShowIcon = true;
                }
            }

            if (glslEditor.Visible) glslEditor.Activate();
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
