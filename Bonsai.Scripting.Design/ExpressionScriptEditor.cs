﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace Bonsai.Scripting.Design
{
    public class ExpressionScriptEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                var script = value as string;
                var editorDialog = new ExpressionScriptEditorDialog();
                editorDialog.Script = script;
                if (editorService.ShowDialog(editorDialog) == DialogResult.OK)
                {
                    return editorDialog.Script;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
