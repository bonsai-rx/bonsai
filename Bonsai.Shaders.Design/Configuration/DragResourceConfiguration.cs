using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Shaders.Configuration.Design
{
    abstract class DragResourceConfiguration
    {
        public DragResourceConfiguration(CollectionEditorControl editor)
        {
            editor.AllowDrop = true;
            editor.DragEnter += editor_DragEnter;
            editor.DragDrop += (sender, e) =>
            {
                if (e.Effect == DragDropEffects.Copy && e.Data.GetDataPresent(DataFormats.FileDrop, true))
                {
                    var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        var path = fileNames[i];
                        if (IsResourceAllowed(path))
                        {
                            var item = CreateResourceConfiguration(path);
                            if (item != null)
                            {
                                editor.AddItem(item);
                            }
                        }
                    }
                }
            };
        }

        protected abstract bool IsResourceAllowed(string fileName);

        protected abstract object CreateResourceConfiguration(string fileName);

        void editor_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                e.Effect = Array.Exists(fileNames, IsResourceAllowed) ? DragDropEffects.Copy : DragDropEffects.None;
            }
        }
    }
}
