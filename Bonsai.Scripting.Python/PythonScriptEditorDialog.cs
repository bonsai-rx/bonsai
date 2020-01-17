using ScintillaNET;
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
            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 10;
            scintilla.StyleClearAll();

            scintilla.CaretLineBackColor = ColorTranslator.FromHtml("#feefff");
            scintilla.Styles[Style.Python.Default].ForeColor = Color.Black;
            scintilla.Styles[Style.Python.Character].ForeColor = ColorTranslator.FromHtml("#00aa00");
            scintilla.Styles[Style.Python.ClassName].ForeColor = Color.Black;
            scintilla.Styles[Style.Python.ClassName].Bold = true;
            scintilla.Styles[Style.Python.CommentLine].ForeColor = ColorTranslator.FromHtml("#adadad");
            scintilla.Styles[Style.Python.CommentBlock].ForeColor = ColorTranslator.FromHtml("#adadad");
            scintilla.Styles[Style.Python.DefName].ForeColor = Color.Black;
            scintilla.Styles[Style.Python.DefName].Bold = true;
            scintilla.Styles[Style.Python.Number].ForeColor = ColorTranslator.FromHtml("#800000");
            scintilla.Styles[Style.Python.String].ForeColor = ColorTranslator.FromHtml("#00aa00");
            scintilla.Styles[Style.Python.StringEol].ForeColor = ColorTranslator.FromHtml("#00aa00");
            scintilla.Styles[Style.Python.Triple].ForeColor = ColorTranslator.FromHtml("#adadad");
            scintilla.Styles[Style.Python.TripleDouble].ForeColor = ColorTranslator.FromHtml("#adadad");
            scintilla.Styles[Style.Python.Word].ForeColor = ColorTranslator.FromHtml("#0000ff");
            scintilla.Styles[Style.Python.Word2].ForeColor = ColorTranslator.FromHtml("#900090");
            scintilla.Lexer = Lexer.Python;

            scintilla.SetKeywords(0, "and del from not while as elif global or with assert else if pass yield break except import print class exec in raise continue finally is return def for lambda try");
            scintilla.SetKeywords(1, "self None True False abs divmod input open staticmethod all enumerate int ord str any eval isinstance pow sum basestring execfile issubclass print super bin file iter property tuple bool filter len range type bytearray float list raw_input unichr callable format locals reduce unicode chr frozenset long reload vars classmethod getattr map repr xrange cmp globals max reversed zip compile hasattr memoryview round __import__ complex hash min set  delattr help next setattr  dict hex object slice dir id oct sorted");
        }

        public string Script { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            scintilla.Text = Script;
            scintilla.EmptyUndoBuffer();
            if (Owner != null)
            {
                Icon = Owner.Icon;
                ShowIcon = true;
            }

            base.OnLoad(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && !e.Handled)
            {
                Close();
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        private void scintilla_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.Control)
            {
                okButton.PerformClick();
            }
        }

        private void scintilla_TextChanged(object sender, EventArgs e)
        {
            Script = scintilla.Text;
        }
    }
}
