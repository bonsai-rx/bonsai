using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.Scripting
{
    public partial class PythonScriptEditorDialog : Form
    {
        public PythonScriptEditorDialog()
        {
            InitializeComponent();
        }

        public string Script
        {
            get { return richTextBox.Text; }
            set { richTextBox.Text = value; }
        }
    }
}
