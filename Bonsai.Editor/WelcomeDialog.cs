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
        Image[] screens;
        int currentScreen;

        public WelcomeDialog()
        {
            InitializeComponent();
            showWelcomeDialogCheckBox.Checked = Settings.Default.ShowWelcomeDialog;
            screens = new[] { Resources.BonsaiWelcome, Resources.BonsaiCommands };
            UpdateStatus();
        }

        public bool ShowWelcomeDialog
        {
            get { return showWelcomeDialogCheckBox.Checked; }
        }

        void UpdateStatus()
        {
            previousButton.Enabled = currentScreen > 0;
            nextButton.Enabled = currentScreen < screens.Length - 1;
            pictureBox.Image = screens[currentScreen];
        }

        private void previousButton_Click(object sender, EventArgs e)
        {
            currentScreen--;
            UpdateStatus();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            currentScreen++;
            UpdateStatus();
        }
    }
}
