using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Window : Sink<IplImage>
    {
        NamedWindow window;

        public Window()
        {
            Name = "Output";
        }

        public string Name { get; set; }

        public override void Process(IplImage input)
        {
            window.ShowImage(input);
        }

        public override void Load(WorkflowContext context)
        {
            window = new NamedWindow(Name);
            base.Load(context);
        }

        public override void Unload(WorkflowContext context)
        {
            window.Dispose();
            base.Unload(context);
        }
    }
}
