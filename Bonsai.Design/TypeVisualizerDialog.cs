using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Design
{
    /// <summary>
    /// Represents a window or panel where a type visualizer can be displayed.
    /// </summary>
    public partial class TypeVisualizerDialog : Form, IDialogTypeVisualizerService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeVisualizerDialog"/> class.
        /// </summary>
        public TypeVisualizerDialog()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public void AddControl(Control control)
        {
            ClientSize = control.Size;
            if (control.MinimumSize != Size.Empty)
            {
                MinimumSize = new Size(
                    control.MinimumSize.Width + Width - control.Width,
                    control.MinimumSize.Height + Height - control.Height);
            }
            Controls.Add(control);
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && !e.Handled)
            {
                Close();
                e.Handled = true;
            }

            if (e.KeyCode == Keys.F11)
            {
                if (FormBorderStyle == FormBorderStyle.None) FormBorderStyle = FormBorderStyle.Sizable;
                else FormBorderStyle = FormBorderStyle.None;
            }
            base.OnKeyDown(e);
        }
    }
}
