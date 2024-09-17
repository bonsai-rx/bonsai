using System.Windows.Forms;

namespace Bonsai.NuGet.Design
{
    public partial class LicenseFileDialog : Form
    {
        string licenseText;

        public LicenseFileDialog()
        {
            InitializeComponent();
        }

        public string LicenseText
        {
            get => licenseText;
            set
            {
                licenseText = value;
                FormatTextBox();
            }
        }

        private void FormatTextBox()
        {
            richTextBox.Text = licenseText;
            richTextBox.SelectAll();
            richTextBox.SelectionIndent += Margin.Left * 3;
            richTextBox.SelectionRightIndent += Margin.Right * 3;
            richTextBox.DeselectAll();
        }
    }
}
