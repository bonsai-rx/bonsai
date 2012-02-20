using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.Scripting.Hosting;
using System.Drawing.Design;
using Bonsai.Scripting;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class CanvasScript : Sink<object>
    {
        ScriptEngine engine;
        CompiledCode script;
        ScriptScope scope;
        IplImage canvas;
        NamedWindow window;

        public CanvasScript()
        {
            CanvasName = "Output";
            CanvasSize = new CvSize(640, 480);
        }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        public string Script { get; set; }

        public CvSize CanvasSize { get; set; }

        public string CanvasName { get; set; }

        public override void Process(object input)
        {
            scope.SetVariable("input", input);
            script.Execute(scope);
            window.ShowImage(canvas);
        }

        public override IDisposable Load()
        {
            engine = IronPython.Hosting.Python.CreateEngine();
            scope = engine.CreateScope();

            engine.Runtime.LoadAssembly(typeof(IplImage).Assembly);
            engine.Execute("from OpenCV.Net import *", scope);
            var source = engine.CreateScriptSourceFromString(Script);
            script = source.Compile();

            canvas = new IplImage(CanvasSize, 8, 3);
            window = new NamedWindow(CanvasName);
            scope.SetVariable("canvas", canvas);
            canvas.SetZero();
            return base.Load();
        }

        protected override void Unload()
        {
            window.Dispose();
            canvas.Close();
            base.Unload();
        }
    }
}
