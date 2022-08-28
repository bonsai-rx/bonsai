using System.Windows.Forms;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides an interface for a <see cref="DialogTypeVisualizer"/> to add
    /// controls to a type visualizer dialog.
    /// </summary>
    public interface IDialogTypeVisualizerService
    {
        /// <summary>
        /// Adds a control to the type visualizer dialog.
        /// </summary>
        /// <param name="control">The control to add to the type visualizer dialog.</param>
        void AddControl(Control control);
    }
}
