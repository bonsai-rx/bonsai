using ScintillaNET;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Scripting.Expressions.Design
{
    internal partial class ExpressionScriptEditorDialog : Form
    {
        public ExpressionScriptEditorDialog()
        {
            InitializeComponent();
            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 10;
            scintilla.StyleClearAll();

            scintilla.CaretLineBackColor = ColorTranslator.FromHtml("#feefff");
            scintilla.Styles[Style.Cpp.Default].ForeColor = Color.Black;
            scintilla.Styles[Style.Cpp.Number].ForeColor = Color.Black;
            scintilla.Styles[Style.Cpp.Character].ForeColor = ColorTranslator.FromHtml("#a31515");
            scintilla.Styles[Style.Cpp.String].ForeColor = ColorTranslator.FromHtml("#a31515");
            scintilla.Styles[Style.Cpp.StringEol].ForeColor = ColorTranslator.FromHtml("#a31515");
            scintilla.Styles[Style.Cpp.Word].ForeColor = ColorTranslator.FromHtml("#0000ff");
            scintilla.Styles[Style.Cpp.Word2].ForeColor = ColorTranslator.FromHtml("#2b91af");
            scintilla.Lexer = Lexer.Cpp;

            var types = "Object Boolean Char String SByte Byte Int16 UInt16 Int32 UInt32 Int64 UInt64 Single Double Decimal DateTime DateTimeOffset TimeSpan Guid Math Convert";
            scintilla.SetKeywords(0, "it iif new outerIt as true false null");
            scintilla.SetKeywords(1, string.Join(" ", types, types.ToLowerInvariant()));
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
