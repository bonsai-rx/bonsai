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
        const string DefaultTabName = "script";
        const string DefaultScript = @"#version 400

void main()
{
}
";

        readonly GlslScriptExampleCollection shaderExamples;
        GraphicsContext context;
        TabPageController activeTab;
        bool defaultActiveTab;

        public GlslScriptEditorDialog()
        {
            InitializeComponent();
            shaderExamples = new GlslScriptExampleCollection();
        }

        public string InitialDirectory
        {
            get { return openFileDialog.InitialDirectory; }
            set { openFileDialog.InitialDirectory = saveFileDialog.InitialDirectory = value; }
        }

        public GlslScriptExampleCollection ScriptExamples
        {
            get { return shaderExamples; }
        }

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
            undoToolStripButton.Enabled = undoToolStripMenuItem.Enabled = activeTab != null && activeTab.Editor.CanUndo;
            redoToolStripButton.Enabled = redoToolStripMenuItem.Enabled = activeTab != null && activeTab.Editor.CanRedo;
        }

        static string GetFileFilter(string name, string extension)
        {
            return string.Format("{0}Shader Files ({1})|{1}", name, extension);
        }

        static int GetFileFilterIndex(ShaderType? shaderType)
        {
            switch (shaderType.GetValueOrDefault((ShaderType)0))
            {
                case ShaderType.ComputeShader: return 1;
                case ShaderType.FragmentShader: return 2;
                case ShaderType.GeometryShader: return 3;
                case ShaderType.TessControlShader: return 4;
                case ShaderType.TessEvaluationShader: return 5;
                case ShaderType.VertexShader: return 6;
                default: return 7;
            }
        }

        static string GetShaderTypePrefix(ShaderType shaderType)
        {
            switch (shaderType)
            {
                case ShaderType.ComputeShader: return "Compute ";
                case ShaderType.FragmentShader: return "Fragment ";
                case ShaderType.GeometryShader: return "Geometry ";
                case ShaderType.TessControlShader: return "Tessellation Control ";
                case ShaderType.TessEvaluationShader: return "Tessellation Evaluation ";
                case ShaderType.VertexShader: return "Vertex ";
                default: throw new ArgumentException("Invalid shader type.");
            }
        }

        static string GetShaderExtension(ShaderType shaderType)
        {
            switch (shaderType)
            {
                case ShaderType.ComputeShader: return ".comp";
                case ShaderType.FragmentShader: return ".frag";
                case ShaderType.GeometryShader: return ".geom";
                case ShaderType.TessControlShader: return ".tesc";
                case ShaderType.TessEvaluationShader: return ".tese";
                case ShaderType.VertexShader: return ".vert";
                default: throw new ArgumentException("Invalid shader type.");
            }
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

        TabPage CreateTabPage(string fileName, string script)
        {
            var tabPage = new TabPage();
            var scintilla = new Scintilla();
            scintilla.Dock = DockStyle.Fill;
            scintilla.BorderStyle = BorderStyle.None;
            scintilla.Location = new Point(0, 0);
            scintilla.Margin = new Padding(0);
            scintilla.TabWidth = 2;
            scintilla.UseTabs = false;
            scintilla.WrapMode = WrapMode.Word;

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

            scintilla.SetKeywords(0, "attribute const uniform varying buffer shared coherent volatile restrict readonly writeonly atomic_uint layout centroid flat smooth noperspective patch sample break continue do for while switch case default if else subroutine in out inout float double int void bool true false invariant precise discard return mat2 mat3 mat4 dmat2 dmat3 dmat4 mat2x2 mat2x3 mat2x4 dmat2x2 dmat2x3 dmat2x4 mat3x2 mat3x3 mat3x4 dmat3x2 dmat3x3 dmat3x4 mat4x2 mat4x3 mat4x4 dmat4x2 dmat4x3 dmat4x4 vec2 vec3 vec4 ivec2 ivec3 ivec4 bvec2 bvec3 bvec4 dvec2 dvec3 dvec4 uint uvec2 uvec3 uvec4 lowp mediump highp precision sampler1D sampler2D sampler3D samplerCube sampler1DShadow sampler2DShadow samplerCubeShadow sampler1DArray sampler2DArray sampler1DArrayShadow sampler2DArrayShadow isampler1D isampler2D isampler3D isamplerCube isampler1DArray isampler2DArray usampler1D usampler2D usampler3D usamplerCube usampler1DArray usampler2DArray sampler2DRect sampler2DRectShadow isampler2DRect usampler2DRect samplerBuffer isamplerBuffer usamplerBuffer sampler2DMS isampler2DMS usampler2DMS sampler2DMSArray isampler2DMSArray usampler2DMSArray samplerCubeArray samplerCubeArrayShadow isamplerCubeArray usamplerCubeArray image1D iimage1D uimage1D image2D iimage2D uimage2D image3D iimage3D uimage3D image2DRect iimage2DRect uimage2DRect imageCube iimageCube uimageCube imageBuffer iimageBuffer uimageBuffer image1DArray iimage1DArray uimage1DArray image2DArray iimage2DArray uimage2DArray imageCubeArray iimageCubeArray uimageCubeArray image2DMS iimage2DMS uimage2DMS image2DMSArray iimage2DMSArray uimage2DMSArray struct common partition active asm class union enum typedef template this resource goto inline noinline public static extern external interface long short half fixed unsigned superp input output hvec2 hvec3 hvec4 fvec2 fvec3 fvec4 sampler3DRect filter sizeof cast namespace using gl_VertexID gl_InstanceID gl_Position gl_PointSize gl_ClipDistance gl_CullDistance gl_MaxPatchVertices gl_PatchVerticesIn gl_InvocationID gl_TessLevelOuter gl_TessLevelInner gl_TessCoord gl_PrimitiveIDIn gl_Layer gl_ViewportIndex gl_FragCoord gl_FrontFacing gl_PointCoord gl_PrimitiveID gl_SampleID gl_SamplePosition gl_SampleMaskIn gl_HelperInvocation gl_FragDepth gl_SampleMask gl_NumWorkGroups gl_WorkGroupSize gl_LocalGroupSize gl_WorkGroupID gl_LocalInvocationID gl_GlobalInvocationID gl_LocalInvocationIndex gl_MaxComputeWorkGroupCount gl_MaxComputeWorkGroupSize gl_MaxComputeUniformComponents gl_MaxComputeTextureImageUnits gl_MaxComputeImageUniforms gl_MaxComputeAtomicCounters gl_MaxComputeAtomicCounterBuffers gl_MaxVertexAttribs gl_MaxVertexUniformComponents gl_MaxVaryingComponents gl_MaxVertexOutputComponents gl_MaxGeometryInputComponents gl_MaxGeometryOutputComponents gl_MaxFragmentInputComponents gl_MaxVertexTextureImageUnits gl_MaxCombinedTextureImageUnits gl_MaxTextureImageUnits gl_MaxImageUnits gl_MaxCombinedImageUnitsAndFragmentOutputs gl_MaxImageSamples gl_MaxVertexImageUniforms gl_MaxTessControlImageUniforms gl_MaxTessEvaluationImageUniforms gl_MaxGeometryImageUniforms gl_MaxFragmentImageUniforms gl_MaxCombinedImageUniforms gl_MaxFragmentUniformComponents gl_MaxDrawBuffers gl_MaxClipDistances gl_MaxGeometryTextureImageUnits gl_MaxGeometryOutputVertices gl_MaxGeometryTotalOutputComponents gl_MaxGeometryUniformComponents gl_MaxGeometryVaryingComponents gl_MaxTessControlInputComponents gl_MaxTessControlOutputComponents gl_MaxTessControlTextureImageUnits gl_MaxTessControlUniformComponents gl_MaxTessControlTotalOutputComponents gl_MaxTessEvaluationInputComponents gl_MaxTessEvaluationOutputComponents gl_MaxTessEvaluationTextureImageUnits gl_MaxTessEvaluationUniformComponents gl_MaxTessPatchComponents gl_MaxPatchVertices gl_MaxTessGenLevel gl_MaxViewports gl_MaxVertexUniformVectors gl_MaxFragmentUniformVectors gl_MaxVaryingVectors gl_MaxVertexAtomicCounters gl_MaxTessControlAtomicCounters gl_MaxTessEvaluationAtomicCounters gl_MaxGeometryAtomicCounters gl_MaxFragmentAtomicCounters gl_MaxCombinedAtomicCounters gl_MaxAtomicCounterBindings gl_MaxVertexAtomicCounterBuffers gl_MaxTessControlAtomicCounterBuffers gl_MaxTessEvaluationAtomicCounterBuffers gl_MaxGeometryAtomicCounterBuffers gl_MaxFragmentAtomicCounterBuffers gl_MaxCombinedAtomicCounterBuffers gl_MaxAtomicCounterBufferSize gl_MinProgramTexelOffset gl_MaxProgramTexelOffset gl_MaxTransformFeedbackBuffers gl_MaxTransformFeedbackInterleavedComponents gl_MaxCullDistances gl_MaxCombinedClipAndCullDistances gl_MaxSamples gl_MaxVertexImageUniforms gl_MaxFragmentImageUniforms gl_MaxComputeImageUniforms gl_MaxCombinedImageUniforms gl_MaxCombinedShaderOutputResources");
            scintilla.SetKeywords(1, "abs acos acosh all any asin asinh atan atanh atomicAdd atomicAnd atomicCompSwap atomicCounter atomicCounterDecrement atomicCounterIncrement atomicExchange atomicMax atomicMin atomicOr atomicXor barrier bitCount bitfieldExtract bitfieldInsert bitfieldReverse ceil clamp cos cosh cross degrees determinant dFdx dFdxCoarse dFdxFine dFdy dFdyCoarse dFdyFine distance dot EmitStreamVertex EmitVertex EndPrimitive EndStreamPrimitive equal exp exp2 faceforward findLSB findMSB floatBitsToInt floatBitsToUint floor fma fract frexp fwidth fwidthCoarse fwidthFine greaterThan greaterThanEqual groupMemoryBarrier imageAtomicAdd imageAtomicAnd imageAtomicCompSwap imageAtomicExchange imageAtomicMax imageAtomicMin imageAtomicOr imageAtomicXor imageLoad imageSamples imageSize imageStore imulExtended intBitsToFloat interpolateAtCentroid interpolateAtOffset interpolateAtSample inverse inversesqrt isinf isnan ldexp length lessThan lessThanEqual log log2 matrixCompMult max memoryBarrier memoryBarrierAtomicCounter memoryBarrierBuffer memoryBarrierImage memoryBarrierShared min mix mod modf noise noise1 noise2 noise3 noise4 normalize not notEqual outerProduct packDouble2x32 packHalf2x16 packSnorm2x16 packSnorm4x8 packUnorm packUnorm2x16 packUnorm4x8 pow radians reflect refract removedTypes round roundEven sign sin sinh smoothstep sqrt step tan tanh texelFetch texelFetchOffset texture textureGather textureGatherOffset textureGatherOffsets textureGrad textureGradOffset textureLod textureLodOffset textureOffset textureProj textureProjGrad textureProjGradOffset textureProjLod textureProjLodOffset textureProjOffset textureQueryLevels textureQueryLod textureSamples textureSize transpose trunc uaddCarry uintBitsToFloat umulExtended unpackDouble2x32 unpackHalf2x16 unpackSnorm2x16 unpackSnorm4x8 unpackUnorm unpackUnorm2x16 unpackUnorm4x8 usubBorrow");
            scintilla.Text = script;
            scintilla.EmptyUndoBuffer();
            scintilla.SetSavePoint();
            scintilla.SavePointLeft += scintilla_SavePointLeft;
            scintilla.SavePointReached += scintilla_SavePointReached;
            scintilla.TextChanged += scintilla_TextChanged;

            var tabState = new TabPageController(tabPage, scintilla);
            tabState.FileName = fileName;
            tabPage.Tag = tabState;
            tabPage.SuspendLayout();
            tabPage.Controls.Add(scintilla);
            tabPage.ResumeLayout(false);
            tabPage.PerformLayout();
            editorTabControl.TabPages.Add(tabPage);
            return tabPage;
        }

        void ActivateTabPage(TabPage tabPage)
        {
            var tabState = tabPage != null ? (TabPageController)tabPage.Tag : null;
            if (tabState != null && activeTab != tabState)
            {
                activeTab = tabState;
                defaultActiveTab = false;
                activeTab.UpdateLineNumberMargin();
                activeTab.UpdateDirtyStatus();
                UpdateUndoStatus();
            }
        }

        int GetNewScriptIndex()
        {
            var indices = new List<int>();
            for (int i = 0; i < editorTabControl.TabCount; i++)
            {
                var tabPage = editorTabControl.TabPages[i];
                var tabState = (TabPageController)tabPage.Tag;
                if (!string.IsNullOrEmpty(tabState.FileName)) continue;

                int index;
                var tabName = Path.GetFileNameWithoutExtension(tabState.Text);
                if (!int.TryParse(tabName.Substring(DefaultTabName.Length), out index)) continue;
                indices.Add(index - 1);
            }

            indices.Sort();
            for (int i = 0; i < indices.Count; i++)
            {
                if (indices[i] != i) return i;
            }

            return indices.Count;
        }

        void RemoveDefaultTabPage()
        {
            if (editorTabControl.TabCount == 1 && defaultActiveTab && !activeTab.Editor.Modified)
            {
                editorTabControl.TabPages.Clear();
            }
        }

        void NewScript(string script)
        {
            if (!string.ReferenceEquals(script, DefaultScript))
            {
                RemoveDefaultTabPage();
            }

            var newScriptIndex = GetNewScriptIndex();
            var tabPage = CreateTabPage(null, script);
            var tabState = (TabPageController)tabPage.Tag;
            tabState.Text = DefaultTabName + (newScriptIndex + 1);
            editorTabControl.SelectTab(tabPage);
            ActivateTabPage(tabPage);
        }

        void OpenScript(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    var tabPage = editorTabControl.TabPages[fileName];
                    if (tabPage == null)
                    {
                        RemoveDefaultTabPage();
                        var script = File.ReadAllText(fileName);
                        tabPage = CreateTabPage(fileName, script);
                    }

                    editorTabControl.SelectTab(tabPage);
                    ActivateTabPage(tabPage);
                }
                catch (SystemException ex)
                {
                    ShowError(ex.Message, "Open Error");
                }
            }
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            var imageSize = toolStrip.ImageScalingSize;
            var scalingFactor = ((int)(factor.Height * 100) / 50 * 50) / 100f;
            if (scalingFactor > 1)
            {
                toolStrip.ImageScalingSize = new Size((int)(imageSize.Width * scalingFactor), (int)(imageSize.Height * scalingFactor));
                menuStrip.ImageScalingSize = toolStrip.ImageScalingSize;
            }
            base.ScaleControl(factor, specified);
        }

        protected override void OnLoad(EventArgs e)
        {
            exampleToolStripMenuItem.Visible = shaderExamples.Count > 0;
            foreach (var exampleType in shaderExamples.GroupBy(example => example.Type))
            {
                var groupName = GetShaderTypePrefix(exampleType.Key) + "Shader";
                var groupMenuItem = new ToolStripMenuItem(groupName);
                exampleToolStripMenuItem.DropDownItems.Add(groupMenuItem);
                foreach (var example in exampleType)
                {
                    var name = example.Name;
                    var source = example.Source;
                    var extension = GetShaderExtension(exampleType.Key);
                    var menuItem = groupMenuItem.DropDownItems.Add(name);
                    menuItem.Click += delegate
                    {
                        NewScript(source);
                        activeTab.Text += extension;
                    };
                }
            }

            var filter = string.Join("|",
                GetFileFilter(GetShaderTypePrefix(ShaderType.ComputeShader), "*.comp"),
                GetFileFilter(GetShaderTypePrefix(ShaderType.FragmentShader), "*.frag"),
                GetFileFilter(GetShaderTypePrefix(ShaderType.GeometryShader), "*.geom"),
                GetFileFilter(GetShaderTypePrefix(ShaderType.TessControlShader), "*.tesc"),
                GetFileFilter(GetShaderTypePrefix(ShaderType.TessEvaluationShader), "*.tese"),
                GetFileFilter(GetShaderTypePrefix(ShaderType.VertexShader), "*.vert"),
                GetFileFilter(string.Empty, "*.comp; *.frag; *.geom; *.tesc; *.tese; *.vert"),
                "All Files (*.*)|*.*");
            openFileDialog.Filter = saveFileDialog.Filter = filter;
            openFileDialog.FilterIndex = GetFileFilterIndex(null);
            base.OnLoad(e);
        }

        protected override void OnShown(EventArgs e)
        {
            NewScript(DefaultScript);
            defaultActiveTab = true;
            base.OnShown(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            foreach (TabPage tabPage in editorTabControl.TabPages)
            {
                var tabState = (TabPageController)tabPage.Tag;
                if (!tabState.CloseScript())
                {
                    e.Cancel = true;
                    break;
                }
            }

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
            NewScript(DefaultScript);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var fileName in openFileDialog.FileNames)
                {
                    OpenScript(fileName);
                }
                InitialDirectory = string.Empty;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(activeTab.FileName)) saveAsToolStripMenuItem_Click(this, e);
            else
            {
                activeTab.SaveScript();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = activeTab.FileName;
            saveFileDialog.FilterIndex = GetFileFilterIndex(activeTab.ScriptType);
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                activeTab.SaveScript(saveFileDialog.FileName);
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            activeTab.Editor.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            activeTab.Editor.Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            activeTab.Editor.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            activeTab.Editor.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            activeTab.Editor.Paste();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            activeTab.Editor.SelectAll();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartBrowser("https://www.opengl.org/documentation/glsl/");
        }

        private void scintilla_SavePointLeft(object sender, EventArgs e)
        {
            UpdateUndoStatus();
            activeTab.UpdateDirtyStatus();
        }

        private void scintilla_SavePointReached(object sender, EventArgs e)
        {
            UpdateUndoStatus();
            activeTab.UpdateDirtyStatus();
        }

        private void scintilla_TextChanged(object sender, EventArgs e)
        {
            UpdateUndoStatus();
            activeTab.UpdateLineNumberMargin();
        }

        private void validateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EnsureGraphicsContext();
            activeTab.ValidateScript();
        }

        private void editorTabControl_MouseDown(object sender, MouseEventArgs e)
        {
            var selectedTab = editorTabControl.SelectedTab;
            if (selectedTab == null) return;
            var tabRect = editorTabControl.GetTabRect(editorTabControl.SelectedIndex);
            if (tabRect.Contains(e.Location))
            {
                using (var graphics = selectedTab.CreateGraphics())
                {
                    var textSize = TextRenderer.MeasureText(
                        graphics,
                        selectedTab.Text,
                        selectedTab.Font,
                        tabRect.Size,
                        TextFormatFlags.Default |
                        TextFormatFlags.NoPadding);
                    var padSize = TextRenderer.MeasureText(
                        graphics,
                        selectedTab.Text.Substring(0, selectedTab.Text.Length - 1),
                        selectedTab.Font,
                        tabRect.Size,
                        TextFormatFlags.Default |
                        TextFormatFlags.NoPadding);
                    const float DefaultDpi = 96f;
                    var offset = graphics.DpiX / DefaultDpi;
                    var margin = (tabRect.Width - textSize.Width) / 2;
                    var buttonWidth = textSize.Width - padSize.Width;
                    var buttonRight = tabRect.Right - margin;
                    var buttonLeft = buttonRight - buttonWidth;
                    var buttonTop = tabRect.Top + 2 * selectedTab.Margin.Top;
                    var buttonBottom = tabRect.Bottom - 2 * selectedTab.Margin.Bottom;
                    var buttonHeight = buttonBottom - buttonTop;
                    var buttonBounds = new Rectangle(buttonLeft, buttonTop, (int)(buttonWidth + offset), buttonHeight);
                    if (buttonBounds.Contains(e.Location) && activeTab.CloseScript())
                    {
                        activeTab = null;
                        editorTabControl.TabPages.Remove(selectedTab);
                        UpdateUndoStatus();
                    }
                }
            }
        }

        private void editorTabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (e.Action == TabControlAction.Selected)
            {
                ActivateTabPage(e.TabPage);
            }
        }

        class TabPageController
        {
            const string ModifiedMark = "*";
            const string CloseSuffix = "    \u2A2F";

            bool modified;
            string fileName;
            string displayText;
            Scintilla editor;
            int maxLineNumberLength;

            public TabPageController(TabPage tabPage, Scintilla scintilla)
            {
                if (tabPage == null)
                {
                    throw new ArgumentNullException("tabPage");
                }

                if (scintilla == null)
                {
                    throw new ArgumentNullException("scintilla");
                }

                TabPage = tabPage;
                editor = scintilla;
                modified = editor.Modified;
            }

            public TabPage TabPage { get; private set; }

            public ShaderType? ScriptType { get; private set; }

            public string Text
            {
                get { return displayText; }
                set
                {
                    displayText = value;
                    UpdateDisplayText();
                    var extension = Path.GetExtension(displayText);
                    switch (extension)
                    {
                        case ".comp": ScriptType = ShaderType.ComputeShader; break;
                        case ".frag": ScriptType = ShaderType.FragmentShader; break;
                        case ".geom": ScriptType = ShaderType.GeometryShader; break;
                        case ".tesc": ScriptType = ShaderType.TessControlShader; break;
                        case ".tese": ScriptType = ShaderType.TessEvaluationShader; break;
                        case ".vert": ScriptType = ShaderType.VertexShader; break;
                        default: ScriptType = null; break;
                    }
                }
            }

            public string FileName
            {
                get { return fileName; }
                set
                {
                    fileName = value;
                    TabPage.Name = fileName;
                    Text = GetProjectFileName(fileName);
                }
            }

            public Scintilla Editor
            {
                get { return editor; }
            }

            static string GetProjectFileName(string fileName)
            {
                if (fileName != null && Path.IsPathRooted(fileName))
                {
                    fileName = PathConvert.GetProjectPath(fileName);
                }

                return fileName;
            }

            void ShowMessage(string message, string caption)
            {
                MessageBox.Show(TabPage, message, caption);
            }

            void ShowError(string message, string caption)
            {
                MessageBox.Show(TabPage, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            public void SaveScript()
            {
                SaveScript(fileName);
            }

            public void SaveScript(string fileName)
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    try
                    {
                        var script = editor.Text;
                        File.WriteAllText(fileName, script);
                        FileName = fileName;
                        editor.SetSavePoint();
                    }
                    catch (SystemException ex)
                    {
                        ShowError(ex.Message, "Save Error");
                    }
                }
            }

            public bool CloseScript()
            {
                if (editor.Modified)
                {
                    var result = MessageBox.Show(
                        TabPage,
                        string.Format("{0} has unsaved changes. Save script file?", displayText),
                        "Unsaved Changes",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        SaveScript();
                        return !editor.Modified;
                    }
                    else return result == DialogResult.No;
                }

                return true;
            }

            void UpdateDisplayText()
            {
                TabPage.Text = editor.Modified
                    ? displayText + ModifiedMark + CloseSuffix
                    : displayText + CloseSuffix;
            }

            public void UpdateDirtyStatus()
            {
                if (modified != editor.Modified) UpdateDisplayText();
                modified = editor.Modified;
            }

            public void UpdateLineNumberMargin()
            {
                const int Padding = 2;
                var maxLineDigits = editor.Lines.Count.ToString(CultureInfo.InvariantCulture).Length;
                if (maxLineNumberLength == maxLineDigits) return;

                var largestDigitString = new string('9', maxLineDigits + 1);
                editor.Margins[0].Width = editor.TextWidth(Style.LineNumber, largestDigitString) + Padding;
                maxLineNumberLength = maxLineDigits;
            }

            public void ValidateScript()
            {
                const string ErrorCaption = "Validation Error";
                const string SuccessCaption = "Validation Completed";
                var shaderType = ScriptType;
                if (!shaderType.HasValue)
                {
                    var message = "No specified shader type. Unable to validate source code.";
                    ShowError(message, ErrorCaption);
                    return;
                }

                int status;
                int shader = 0;
                try
                {
                    shader = GL.CreateShader(shaderType.Value);
                    GL.ShaderSource(shader, editor.Text);
                    GL.CompileShader(shader);
                    GL.GetShader(shader, ShaderParameter.CompileStatus, out status);
                    if (status == 0)
                    {
                        var message = string.Format(
                            "Failed to compile shader.\nShader name: {0}\n{1}",
                            displayText,
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

        private void editorTabControl_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                for (int i = 0; i < fileNames.Length; i++)
                {
                    OpenScript(fileNames[i]);
                }
            }
        }

        private void editorTabControl_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
    }
}
