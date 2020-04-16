using System.Windows.Forms;

namespace Bonsai.Design
{
    public class OpenFileNameEditor : FileNameEditor
    {
        protected override FileDialog CreateFileDialog()
        {
            return new OpenFileDialog();
        }
    }
}
