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

            var fontSize = TextRenderer.MeasureText("c", richTextBox1.Font);
            var tabSize = fontSize.Width * 4;
            var tabs = new int[10];
            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i] = (i + 1) * tabSize;
            }

            richTextBox1.SelectionTabs = tabs;
        }

        public string Script
        {
            get { return richTextBox1.Text; }
            set { richTextBox1.Text = value; }
        }
    }
}
