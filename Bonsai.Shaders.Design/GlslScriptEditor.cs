using System;
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
using OpenTK.Graphics.OpenGL4;

namespace Bonsai.Shaders.Design
{
    public class GlslScriptEditor : UITypeEditor
    {
        protected virtual ShaderType? GetShaderType()
        {
            return null;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                var fileName = value as string;
                var editorDialog = new GlslScriptEditorDialog();
                editorDialog.FileName = fileName;
                editorDialog.ShaderType = GetShaderType();
                if (editorService.ShowDialog(editorDialog) == DialogResult.OK)
                {
                    return editorDialog.FileName;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }

    class VertScriptEditor : GlslScriptEditor
    {
        protected override ShaderType? GetShaderType()
        {
            return ShaderType.VertexShader;
        }
    }

    class FragScriptEditor : GlslScriptEditor
    {
        protected override ShaderType? GetShaderType()
        {
            return ShaderType.FragmentShader;
        }
    }

    class GeomScriptEditor : GlslScriptEditor
    {
        protected override ShaderType? GetShaderType()
        {
            return ShaderType.GeometryShader;
        }
    }
}
