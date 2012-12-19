using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;
using Bonsai.Vision;

[assembly: TypeVisualizer(typeof(ConnectedComponentVisualizer), Target = typeof(ConnectedComponent))]

namespace Bonsai.Vision.Design
{
    public class ConnectedComponentVisualizer : IplImageVisualizer
    {
        public override void Show(object value)
        {
            var connectedComponent = (ConnectedComponent)value;
            var boundingBox = connectedComponent.Contour.Rect;
            var boundingBoxCenter = new CvPoint2D32f(0.5f * boundingBox.Width, 0.5f * boundingBox.Height);
            var output = new IplImage(new CvSize(boundingBox.Width, boundingBox.Height), 8, 3);
            output.SetZero();

            DrawingHelper.DrawConnectedComponent(output, connectedComponent, boundingBoxCenter - connectedComponent.Centroid);
            base.Show(output);
        }
    }
}
