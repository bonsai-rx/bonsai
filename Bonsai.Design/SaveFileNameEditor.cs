using System.Windows.Forms;

namespace Bonsai.Design
{
    public class SaveFileNameEditor : FileNameEditor
    {
        protected override FileDialog CreateFileDialog()
        {
            return new SaveFileDialog();
        }
    }
}
