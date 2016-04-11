using Bonsai.Design;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.Shaders.Design
{
    public partial class GlslScriptEditorDialog : Form
    {
        int maxLineNumberLength;
        const string BaseTitle = "GLSL Script";
        const string ModifiedMark = "*";
        GraphicsContext context;

        public GlslScriptEditorDialog()
        {
            InitializeComponent();
            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 10;
            scintilla.StyleClearAll();

            scintilla.CaretLineBackColor = ColorTranslator.FromHtml("#feefff");
            scintilla.Styles[Style.Cpp.Default].ForeColor = Color.Black;
            scintilla.Styles[Style.Cpp.Character].ForeColor = ColorTranslator.FromHtml("#00aa00");
            scintilla.Styles[Style.Cpp.CommentLine].ForeColor = ColorTranslator.FromHtml("#008000");
            scintilla.Styles[Style.Cpp.Comment].ForeColor = ColorTranslator.FromHtml("#008000");
            scintilla.Styles[Style.Cpp.Preprocessor].ForeColor = ColorTranslator.FromHtml("#adadad");
            scintilla.Styles[Style.Cpp.Number].ForeColor = ColorTranslator.FromHtml("#800000");
            scintilla.Styles[Style.Cpp.String].ForeColor = ColorTranslator.FromHtml("#00aa00");
            scintilla.Styles[Style.Cpp.StringEol].ForeColor = ColorTranslator.FromHtml("#00aa00");
            scintilla.Styles[Style.Cpp.Word].ForeColor = ColorTranslator.FromHtml("#0000ff");
            scintilla.Styles[Style.Cpp.Word2].ForeColor = ColorTranslator.FromHtml("#900090");
            scintilla.Lexer = Lexer.Cpp;

            scintilla.SetKeywords(0, "attribute const uniform varying layout centroid flat smooth noperspective patch sample break continue do for while switch case default if else subroutine in out inout float double int void bool true false invariant discard return mat2 mat3 mat4 dmat2 dmat3 dmat4 mat2x2 mat2x3 mat2x4 dmat2x2 dmat2x3 dmat2x4 mat3x2 mat3x3 mat3x4 dmat3x2 dmat3x3 dmat3x4 mat4x2 mat4x3 mat4x4 dmat4x2 dmat4x3 dmat4x4 vec2 vec3 vec4 ivec2 ivec3 ivec4 bvec2 bvec3 bvec4 dvec2 dvec3 dvec4 uint uvec2 uvec3 uvec4 lowp mediump highp precision sampler1D sampler2D sampler3D samplerCube sampler1DShadow sampler2DShadow samplerCubeShadow sampler1DArray sampler2DArray sampler1DArrayShadow sampler2DArrayShadow isampler1D isampler2D isampler3D isamplerCube isampler1DArray isampler2DArray usampler1D usampler2D usampler3D usamplerCube usampler1DArray usampler2DArray sampler2DRect sampler2DRectShadow isampler2DRect usampler2DRect samplerBuffer isamplerBuffer usamplerBuffer sampler2DMS isampler2DMS usampler2DMS sampler2DMSArray isampler2DMSArray usampler2DMSArray samplerCubeArray samplerCubeArrayShadow isamplerCubeArray usamplerCubeArray struct gl_ClipDistance gl_CullDistance gl_FragCoord gl_FragDepth gl_FrontFacing gl_GlobalInvocationID gl_HelperInvocation gl_InstanceID gl_InvocationID gl_Layer gl_LocalInvocationID gl_LocalInvocationIndex gl_NumSamples gl_NumWorkGroups gl_PatchVerticesIn gl_PointCoord gl_PointSize gl_Position gl_PrimitiveID gl_PrimitiveIDIn gl_SampleID gl_SampleMask gl_SampleMaskIn gl_SamplePosition gl_TessCoord gl_TessLevelInner gl_TessLevelOuter gl_VertexID gl_ViewportIndex gl_WorkGroupID gl_WorkGroupSize");
            scintilla.SetKeywords(1, "abs acos acosh all any asin asinh atan atanh atomicAdd atomicAnd atomicCompSwap atomicCounter atomicCounterDecrement atomicCounterIncrement atomicExchange atomicMax atomicMin atomicOr atomicXor barrier bitCount bitfieldExtract bitfieldInsert bitfieldReverse ceil clamp cos cosh cross degrees determinant dFdx dFdxCoarse dFdxFine dFdy dFdyCoarse dFdyFine distance dot EmitStreamVertex EmitVertex EndPrimitive EndStreamPrimitive equal exp exp2 faceforward findLSB findMSB floatBitsToInt floatBitsToUint floor fma fract frexp fwidth fwidthCoarse fwidthFine greaterThan greaterThanEqual groupMemoryBarrier imageAtomicAdd imageAtomicAnd imageAtomicCompSwap imageAtomicExchange imageAtomicMax imageAtomicMin imageAtomicOr imageAtomicXor imageLoad imageSamples imageSize imageStore imulExtended intBitsToFloat interpolateAtCentroid interpolateAtOffset interpolateAtSample inverse inversesqrt isinf isnan ldexp length lessThan lessThanEqual log log2 matrixCompMult max memoryBarrier memoryBarrierAtomicCounter memoryBarrierBuffer memoryBarrierImage memoryBarrierShared min mix mod modf noise noise1 noise2 noise3 noise4 normalize not notEqual outerProduct packDouble2x32 packHalf2x16 packSnorm2x16 packSnorm4x8 packUnorm packUnorm2x16 packUnorm4x8 pow radians reflect refract removedTypes round roundEven sign sin sinh smoothstep sqrt step tan tanh texelFetch texelFetchOffset texture textureGather textureGatherOffset textureGatherOffsets textureGrad textureGradOffset textureLod textureLodOffset textureOffset textureProj textureProjGrad textureProjGradOffset textureProjLod textureProjLodOffset textureProjOffset textureQueryLevels textureQueryLod textureSamples textureSize transpose trunc uaddCarry uintBitsToFloat umulExtended unpackDouble2x32 unpackHalf2x16 unpackSnorm2x16 unpackSnorm4x8 unpackUnorm unpackUnorm2x16 unpackUnorm4x8 usubBorrow");
        }

        public string FileName { get; set; }

        public ShaderType? ShaderType { get; set; }

        void EnsureGraphicsContext()
        {
            if (context == null)
            {
                var windowInfo = OpenTK.Platform.Utilities.CreateWindowsWindowInfo(Handle);
                context = new GraphicsContext(GraphicsMode.Default, windowInfo);
                context.MakeCurrent(windowInfo);
                context.LoadAll();
            }
        }

        void UpdateUndoStatus()
        {
            undoToolStripButton.Enabled = undoToolStripMenuItem.Enabled = scintilla.CanUndo;
            redoToolStripButton.Enabled = redoToolStripMenuItem.Enabled = scintilla.CanRedo;
        }

        void UpdateDirtyStatus()
        {
            var title = Text;
            if (scintilla.Modified && !title.EndsWith(ModifiedMark))
            {
                Text = title + ModifiedMark;
            }
            else if (!scintilla.Modified && title.EndsWith(ModifiedMark))
            {
                Text = title.TrimEnd(ModifiedMark[0]);
            }
        }

        void UpdateLineNumberMargin()
        {
            const int Padding = 2;
            var maxLineDigits = scintilla.Lines.Count.ToString(CultureInfo.InvariantCulture).Length;
            if (maxLineNumberLength == maxLineDigits) return;

            var largestDigitString = new string('9', maxLineDigits + 1);
            scintilla.Margins[0].Width = scintilla.TextWidth(Style.LineNumber, largestDigitString) + Padding;
            maxLineNumberLength = maxLineDigits;
        }

        void SetFileName(string fileName)
        {
            if (fileName != null && Path.IsPathRooted(fileName))
            {
                fileName = PathConvert.GetProjectPath(fileName);
            }
            var title = FileName = saveFileDialog.FileName = fileName;
            Text = title ?? BaseTitle;
        }

        void ShowMessage(string message, string caption)
        {
            MessageBox.Show(this, message, caption);
        }

        void ShowError(string message, string caption)
        {
            MessageBox.Show(this, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void StartBrowser(string url)
        {
            Uri uriResult;
            var validUrl = Uri.TryCreate(url, UriKind.Absolute, out uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!validUrl)
            {
                throw new ArgumentException("The URL is malformed.");
            }

            try
            {
                var result = MessageBox.Show(
                    this,
                    "The help pages for GLSL will now open in a new window.",
                    "Help Request",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.OK)
                {
                    Cursor = Cursors.AppStarting;
                    Process.Start(url);
                }
            }
            catch { } //best effort
            finally
            {
                Cursor = null;
            }
        }

        void OpenScript(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    var script = File.ReadAllText(fileName);
                    SetFileName(fileName);
                    scintilla.Text = script;
                    scintilla.EmptyUndoBuffer();
                    scintilla.SetSavePoint();
                    UpdateUndoStatus();
                }
                catch (SystemException ex)
                {
                    ShowError(ex.Message, "Read Error");
                }
            }
        }

        void SaveScript(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    var script = scintilla.Text;
                    File.WriteAllText(fileName, script);
                    SetFileName(fileName);
                    scintilla.SetSavePoint();
                }
                catch (SystemException ex)
                {
                    ShowError(ex.Message, "Save Error");
                }
            }
        }

        bool CloseScript()
        {
            if (scintilla.Modified)
            {
                var result = MessageBox.Show(
                    this,
                    "Script has unsaved changes. Save script file?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(this, EventArgs.Empty);
                    return !scintilla.Modified;
                }
                else return result == DialogResult.No;
            }

            return true;
        }

        protected override void OnShown(EventArgs e)
        {
            OpenScript(FileName);
            UpdateLineNumberMargin();
            base.OnShown(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = !CloseScript();
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (context != null)
            {
                context.Dispose();
                context = null;
            }
            base.OnFormClosed(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CloseScript()) return;

            SetFileName(null);
            scintilla.Text = null;
            scintilla.EmptyUndoBuffer();
            UpdateUndoStatus();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CloseScript()) return;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                OpenScript(openFileDialog.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(saveFileDialog.FileName)) saveAsToolStripMenuItem_Click(this, e);
            else
            {
                SaveScript(saveFileDialog.FileName);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                saveToolStripMenuItem_Click(this, e);
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla.Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla.Paste();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla.SelectAll();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartBrowser("https://www.opengl.org/documentation/glsl/");
        }

        private void scintilla_SavePointLeft(object sender, EventArgs e)
        {
            UpdateUndoStatus();
            UpdateDirtyStatus();
        }

        private void scintilla_SavePointReached(object sender, EventArgs e)
        {
            UpdateUndoStatus();
            UpdateDirtyStatus();
        }

        private void scintilla_TextChanged(object sender, EventArgs e)
        {
            UpdateUndoStatus();
            UpdateLineNumberMargin();
        }

        private void validateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string ErrorCaption = "Validation Error";
            const string SuccessCaption = "Validation Completed";
            var shaderType = ShaderType;
            if (!shaderType.HasValue)
            {
                var message = "No specified shader type. Unable to validate source code.";
                ShowError(message, ErrorCaption);
            }

            int status;
            int shader = 0;
            try
            {
                EnsureGraphicsContext();
                shader = GL.CreateShader(shaderType.Value);
                GL.ShaderSource(shader, scintilla.Text);
                GL.CompileShader(shader);
                GL.GetShader(shader, ShaderParameter.CompileStatus, out status);
                if (status == 0)
                {
                    var message = string.Format(
                        "Failed to compile shader.\nShader name: {0}\n{1}",
                        Name,
                        GL.GetShaderInfoLog(shader));
                    ShowError(message, ErrorCaption);
                }
                else ShowMessage("The shader object compiled successfully.", SuccessCaption);
            }
            finally
            {
                if (shader != 0)
                {
                    GL.DeleteShader(shader);
                }
            }
        }
    }
}
