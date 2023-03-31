using ScintillaNET;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Design
{
    internal partial class AnnotationBuilderEditorDialog : Form
    {
        public AnnotationBuilderEditorDialog()
        {
            InitializeComponent();
            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 10;
            scintilla.StyleClearAll();

            scintilla.CaretLineBackColor = ColorTranslator.FromHtml("#feefff");
            scintilla.Styles[Style.Markdown.Default].ForeColor = Color.Black;
            scintilla.Styles[Style.Markdown.Link].Underline = true;
            scintilla.Styles[Style.Markdown.Em1].Italic = true;
            scintilla.Styles[Style.Markdown.Em2].Italic = true;
            scintilla.Styles[Style.Markdown.Strong1].Bold = true;
            scintilla.Styles[Style.Markdown.Strong2].Bold = true;
            scintilla.Styles[Style.Markdown.Header1].Bold = true;
            scintilla.Styles[Style.Markdown.Header2].Bold = true;
            scintilla.Styles[Style.Markdown.Header3].Bold = true;
            scintilla.Styles[Style.Markdown.Header4].Bold = true;
            scintilla.Styles[Style.Markdown.Header5].Bold = true;
            scintilla.Styles[Style.Markdown.Header6].Bold = true;
            scintilla.Styles[Style.Markdown.Strong1].ForeColor = ColorTranslator.FromHtml("#2b91af");
            scintilla.Styles[Style.Markdown.Strong2].ForeColor = ColorTranslator.FromHtml("#2b91af");
            scintilla.Styles[Style.Markdown.Header1].ForeColor = ColorTranslator.FromHtml("#2b91af");
            scintilla.Styles[Style.Markdown.Header2].ForeColor = ColorTranslator.FromHtml("#2b91af");
            scintilla.Styles[Style.Markdown.Header3].ForeColor = ColorTranslator.FromHtml("#2b91af");
            scintilla.Styles[Style.Markdown.Header4].ForeColor = ColorTranslator.FromHtml("#2b91af");
            scintilla.Styles[Style.Markdown.Header5].ForeColor = ColorTranslator.FromHtml("#2b91af");
            scintilla.Styles[Style.Markdown.Header6].ForeColor = ColorTranslator.FromHtml("#2b91af");
            scintilla.Styles[Style.Markdown.UListItem].ForeColor = ColorTranslator.FromHtml("#2b91af");
            scintilla.Styles[Style.Markdown.OListItem].ForeColor = ColorTranslator.FromHtml("#2b91af");
            scintilla.Styles[Style.Markdown.HRule].ForeColor = ColorTranslator.FromHtml("#2b91af");
            scintilla.Styles[Style.Markdown.Code].ForeColor = ColorTranslator.FromHtml("#a31515");
            scintilla.Styles[Style.Markdown.Code2].ForeColor = ColorTranslator.FromHtml("#a31515");
            scintilla.Styles[Style.Markdown.CodeBk].ForeColor = ColorTranslator.FromHtml("#a31515");
            scintilla.Styles[Style.Markdown.BlockQuote].ForeColor = ColorTranslator.FromHtml("#a31515");
            scintilla.Lexer = Lexer.Markdown;
        }

        public string Annotation { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            scintilla.Text = Annotation;
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
            Annotation = scintilla.Text;
        }
    }
}
