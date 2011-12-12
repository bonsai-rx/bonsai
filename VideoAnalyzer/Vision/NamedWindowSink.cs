using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace VideoAnalyzer.Vision
{
    public class NamedWindowSink : Sink<IplImage>
    {
        NamedWindow window;

        public string Name { get; set; }

        public override void Process(IplImage input)
        {
            window.ShowImage(input);
        }

        public override void Load(WorkflowContext context)
        {
            window = new NamedWindow(Name);
        }

        public override void Unload()
        {
            window.Dispose();
        }
    }
}
