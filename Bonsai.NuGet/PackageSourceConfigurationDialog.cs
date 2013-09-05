using NuGet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.NuGet
{
    public partial class PackageSourceConfigurationDialog : Form
    {
        public PackageSourceConfigurationDialog()
        {
            InitializeComponent();
        }

        public PackageSourceProvider SourceProvider { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            var provider = SourceProvider;
            if (provider != null)
            {
                foreach (var packageSource in provider.LoadPackageSources())
                {
                    var item = packageSourceListView.Items.Add(packageSource.Name, packageSource.Source, 0);
                    item.Checked = packageSource.IsEnabled;
                }
            }

            base.OnLoad(e);
        }
    }
}
