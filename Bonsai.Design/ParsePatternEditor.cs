using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Bonsai.Design
{
    public class ParsePatternEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var pattern = (string)value;
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                using (var control = new ParsePatternControl())
                {
                    control.Commit += (sender, e) =>
                    {
                        pattern += ((ParsePattern)control.SelectedItem).Pattern;
                        editorService.CloseDropDown();
                    };
                    editorService.DropDownControl(control);
                }
            }

            return pattern;
        }

        class ParsePatternControl : ListBox
        {
            bool allowCommit;
            public event EventHandler Commit;

            private void OnCommit(EventArgs e)
            {
                var commit = Commit;
                if (commit != null)
                {
                    commit(this, e);
                }
            }

            protected override void OnCreateControl()
            {
                base.OnCreateControl();
                Items.AddRange(new[]
                {
                    new ParsePattern("%B", typeof(byte)),
                    new ParsePattern("%h", typeof(short)),
                    new ParsePattern("%H", typeof(ushort)),
                    new ParsePattern("%i", typeof(int)),
                    new ParsePattern("%I", typeof(uint)),
                    new ParsePattern("%l", typeof(long)),
                    new ParsePattern("%L", typeof(ulong)),
                    new ParsePattern("%f", typeof(float)),
                    new ParsePattern("%d", typeof(double)),
                    new ParsePattern("%b", typeof(bool)),
                    new ParsePattern("%c", typeof(char)),
                    new ParsePattern("%s", typeof(string)),
                    new ParsePattern("%t", typeof(DateTimeOffset)),
                    new ParsePattern("%T", typeof(TimeSpan)),
                });

                Height = ItemHeight * (Items.Count + 1);
            }

            private void SubmitSelection()
            {
                if (allowCommit && SelectedItem != null)
                {
                    OnCommit(EventArgs.Empty);
                }
            }

            protected override void OnSelectedValueChanged(EventArgs e)
            {
                allowCommit = true;
                base.OnSelectedValueChanged(e);
            }

            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                if (keyData == Keys.Return)
                {
                    SubmitSelection();
                }
                return base.ProcessCmdKey(ref msg, keyData);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                SubmitSelection();
                base.OnMouseUp(e);
            }
        }

        class ParsePattern
        {
            public ParsePattern(string pattern, Type type)
            {
                Pattern = pattern;
                Type = type;
            }

            public string Pattern { get; set; }

            public Type Type { get; set; }

            public override string ToString()
            {
                return string.Format("{0} - {1}", Pattern, Type.Name);
            }
        }
    }
}
