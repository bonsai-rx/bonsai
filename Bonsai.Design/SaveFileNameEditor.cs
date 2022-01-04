using System.Windows.Forms;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides a user interface editor that prompts the user to
    /// select a location for saving a file.
    /// </summary>
    public class SaveFileNameEditor : FileNameEditor
    {
        /// <summary>
        /// Initializes the dialog box from which the user can select a
        /// location for saving a file.
        /// </summary>
        /// <returns>
        /// The <see cref="FileDialog"/> object which will display the
        /// dialog box from which the user can select a location for
        /// saving a file.
        /// </returns>
        protected override FileDialog CreateFileDialog()
        {
            return new SaveFileDialog();
        }
    }
}
