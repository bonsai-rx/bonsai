using Bonsai.NuGet.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.NuGet
{
    public class SaveFolderDialog : Component
    {
        readonly SaveFileDialog dialog;

        public SaveFolderDialog()
        {
            dialog = new SaveFileDialog();
            dialog.OverwritePrompt = false;
            dialog.FileOk += dialog_FileOk;
        }

        void dialog_FileOk(object sender, CancelEventArgs e)
        {
            if (File.Exists(dialog.FileName))
            {
                var message = string.Format(Resources.SaveFolderExists, Path.GetFileName(dialog.FileName));
                MessageBox.Show(message, Resources.SaveFolderExistsCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
        }

        public string FileName
        {
            get { return dialog.FileName; }
            set { dialog.FileName = value; }
        }

        public DialogResult ShowDialog()
        {
            return dialog.ShowDialog();
        }

        public DialogResult ShowDialog(IWin32Window owner)
        {
            return dialog.ShowDialog(owner);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dialog.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
