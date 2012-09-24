using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using Bonsai.Configuration;

namespace Bonsai.PackageManager
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var packageConfiguration = (PackageConfiguration)ConfigurationManager.GetSection(PackageConfiguration.SectionName);
            if (packageConfiguration != null)
            {
                foreach (PackageElement package in packageConfiguration.Packages)
                {
                    if (package.Dependency) listBox1.Items.Add(package.AssemblyName);
                    else listBox2.Items.Add(package.AssemblyName);
                }
            }
        }
    }
}
