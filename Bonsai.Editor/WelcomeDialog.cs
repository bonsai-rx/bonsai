using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Bonsai.Editor.Properties;

namespace Bonsai.Editor
{
    public partial class WelcomeDialog : Form
    {
        public WelcomeDialog()
        {
            InitializeComponent();
            showWelcomeDialogCheckBox.Checked = Settings.Default.ShowWelcomeDialog;
        }

        public bool ShowWelcomeDialog
        {
            get { return showWelcomeDialogCheckBox.Checked; }
        }
    }
}
