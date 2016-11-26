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
        ShaderWindowSettings selectedObject;

        public ShaderConfigurationEditorDialog()
        {
            InitializeComponent();
            materialButton.Tag = materialCollectionEditor;
            meshButton.Tag = meshCollectionEditor;
            textureButton.Tag = textureCollectionEditor;
            materialCollectionEditor.CollectionItemType = typeof(MaterialConfiguration);
            meshCollectionEditor.CollectionItemType = typeof(MeshConfiguration);
            meshCollectionEditor.NewItemTypes = new[] { typeof(MeshConfiguration), typeof(TexturedQuad), typeof(TexturedModel) };
            textureCollectionEditor.CollectionItemType = typeof(TextureConfiguration);
            textureCollectionEditor.NewItemTypes = new[] { typeof(Texture2D), typeof(ImageTexture) };
        }

        public ShaderConfigurationEditorPage SelectedPage { get; set; }

        public ShaderWindowSettings SelectedObject
        {
            get { return selectedObject; }
            set
            {
                selectedObject = value;
                materialCollectionEditor.Items = selectedObject.Materials;
                meshCollectionEditor.Items = selectedObject.Meshes;
                textureCollectionEditor.Items = selectedObject.Textures;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            initialHeight = Height;
            initialCollectionEditorHeight = materialCollectionEditor.Height;
            switch (SelectedPage)
            {
                case ShaderConfigurationEditorPage.Meshes:
                    meshButton.Checked = true;
                    break;
                case ShaderConfigurationEditorPage.Textures:
                    textureButton.Checked = true;
                    break;
                case ShaderConfigurationEditorPage.Materials:
                    materialButton.Checked = true;
                    break;
                default:
                case ShaderConfigurationEditorPage.Window:
                    windowButton.Checked = true;
                    break;
            }
            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (initialHeight > 0)
            {
                var expansion = Height - initialHeight;
                materialCollectionEditor.Height = initialCollectionEditorHeight + expansion;
                meshCollectionEditor.Height = initialCollectionEditorHeight + expansion;
                textureCollectionEditor.Height = initialCollectionEditorHeight + expansion;
            }
            base.OnResize(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                SetCollectionItems(materialCollectionEditor, selectedObject.Materials);
                SetCollectionItems(meshCollectionEditor, selectedObject.Meshes);
                SetCollectionItems(textureCollectionEditor, selectedObject.Textures);
            }

            propertyGrid.SelectedObject = null;
            base.OnClosed(e);
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
            propertyGrid.SelectedObject = windowButton.Checked ? SelectedObject : null;
        }

        private void materialButton_CheckedChanged(object sender, EventArgs e)
        {
            var radioButton = (RadioButton)sender;
            var collectionEditor = (CollectionEditorControl)radioButton.Tag;
            collectionEditor.Visible = radioButton.Checked;
            RefreshCollectionEditor(collectionEditor);
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (materialCollectionEditor.Visible) materialCollectionEditor.Refresh();
            if (meshCollectionEditor.Visible) meshCollectionEditor.Refresh();
            if (textureCollectionEditor.Visible) textureCollectionEditor.Refresh();
        }
    }
}
