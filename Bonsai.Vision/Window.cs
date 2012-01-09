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

        public override IDisposable Load()
        {
            window = new NamedWindow(Name);
            return base.Load();
        }

        protected override void Unload()
        {
            window.Dispose();
            base.Unload();
        }
    }
}
