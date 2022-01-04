using System.Windows.Forms;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides a user interface editor that prompts the user to
    /// open a file.
    /// </summary>
    public class OpenFileNameEditor : FileNameEditor
    {
        /// <summary>
        /// Initializes the dialog box from which the user can select a file.
        /// </summary>
        /// <returns>
        /// The <see cref="FileDialog"/> object which will display the
        /// dialog box from which the user can select a file.
        /// </returns>
        protected override FileDialog CreateFileDialog()
        {
            return new OpenFileDialog();
        }
    }
}
