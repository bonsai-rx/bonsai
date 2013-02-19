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
            var validContour = connectedComponent.Contour != null && !connectedComponent.Contour.IsInvalid;
            var boundingBox = validContour ? connectedComponent.Contour.Rect : new CvRect(0, 0, 1, 1);
            var output = new IplImage(new CvSize(boundingBox.Width, boundingBox.Height), 8, 3);
            output.SetZero();

            if (validContour)
            {
                DrawingHelper.DrawConnectedComponent(output, connectedComponent, new CvPoint2D32f(-boundingBox.X, -boundingBox.Y));
            }
            base.Show(output);
        }
    }
}
