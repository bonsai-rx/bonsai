using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;
using Bonsai.Vision;

[assembly: TypeVisualizer(typeof(ConnectedComponentCollectionVisualizer), Target = typeof(ConnectedComponentCollection))]

namespace Bonsai.Vision.Design
{
    public class ConnectedComponentCollectionVisualizer : IplImageVisualizer
    {
        public override void Show(object value)
        {
            var connectedComponents = (ConnectedComponentCollection)value;
            var output = new IplImage(connectedComponents.ImageSize, IplDepth.U8, 3);
            output.SetZero();

            foreach (var component in connectedComponents)
            {
                DrawingHelper.DrawConnectedComponent(output, component);
            }

            base.Show(output);
        }
    }
}
