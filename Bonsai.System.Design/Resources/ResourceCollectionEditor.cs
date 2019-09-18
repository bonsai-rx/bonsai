using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Resources.Design
{
    public abstract class ResourceCollectionEditor : CollectionEditor
    {
        protected ResourceCollectionEditor(Type type)
            : base(type)
        {
        }

        protected abstract bool IsResourceSupported(string fileName);

        protected abstract object CreateResourceConfiguration(string fileName);

        protected override CollectionEditorDialog CreateEditorDialog()
        {
            var editorDialog = base.CreateEditorDialog();
            var editorControl = editorDialog.EditorControl;
            editorControl.AllowDrop = true;
            editorControl.DragEnter += (sender, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                {
                    var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                    e.Effect = Array.Exists(fileNames, IsResourceSupported) ? DragDropEffects.Copy : DragDropEffects.None;
                }
            };

            editorControl.DragDrop += (sender, e) =>
            {
                if (e.Effect == DragDropEffects.Copy && e.Data.GetDataPresent(DataFormats.FileDrop, true))
                {
                    var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        var path = fileNames[i];
                        if (IsResourceSupported(path))
                        {
                            var item = CreateResourceConfiguration(path);
                            if (item != null)
                            {
                                editorControl.AddItem(item);
                            }
                        }
                    }
                }
            };
            return editorDialog;
        }
    }
}
