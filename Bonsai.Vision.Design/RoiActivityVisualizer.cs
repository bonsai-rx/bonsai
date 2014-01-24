﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Vision.Design;
using Bonsai;
using Bonsai.Design;
using Bonsai.Dag;
using Bonsai.Expressions;
using OpenCV.Net;
using Bonsai.Vision;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Reactive.Linq;
using Font = OpenCV.Net.Font;
using Point = OpenCV.Net.Point;
using Size = OpenCV.Net.Size;

[assembly: TypeVisualizer(typeof(RoiActivityVisualizer), Target = typeof(RoiActivity))]

namespace Bonsai.Vision.Design
{
    public class RoiActivityVisualizer : IplImageVisualizer
    {
        const int RoiThickness = 1;
        static readonly Scalar InactiveRoi = Scalar.Rgb(255, 0, 0);
        static readonly Scalar ActiveRoi = Scalar.Rgb(0, 255, 0);

        Font font;
        IplImage input;
        IplImage canvas;
        IDisposable inputHandle;
        RegionActivityCollection regions;

        public override void Show(object value)
        {
            regions = (RegionActivityCollection)value;
            if (input != null)
            {
                canvas = IplImageHelper.EnsureColorCopy(canvas, input);
                for (int i = 0; i < regions.Count; i++)
                {
                    var rectangle = regions[i].Rect;
                    var color = regions[i].Activity.Val0 > 0 ? ActiveRoi : InactiveRoi;

                    int baseline;
                    Size labelSize;
                    var label = i.ToString();
                    CV.GetTextSize(label, font, out labelSize, out baseline);
                    CV.PutText(canvas, i.ToString(), new Point(rectangle.X + RoiThickness, rectangle.Y + labelSize.Height + RoiThickness), font, color);
                    CV.PutText(canvas, regions[i].Activity.Val0.ToString(), new Point(rectangle.X + RoiThickness, rectangle.Y - labelSize.Height - RoiThickness), font, color);
                }

                base.Show(canvas);
            }
        }

        protected override void RenderFrame()
        {
            GL.Color3(Color.White);
            base.RenderFrame();

            if (regions != null)
            {
                GL.Disable(EnableCap.Texture2D);
                foreach (var region in regions)
                {
                    var roi = region.Roi;
                    var color = region.Activity.Val0 > 0 ? Color.LimeGreen : Color.Red;

                    GL.Color3(color);
                    GL.Begin(PrimitiveType.LineLoop);
                    for (int i = 0; i < roi.Length; i++)
                    {
                        GL.Vertex2(DrawingHelper.NormalizePoint(roi[i], input.Size));
                    }
                    GL.End();
                }
            }
        }

        public override void Load(IServiceProvider provider)
        {
            var workflow = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            if (workflow != null && context != null)
            {
                var predecessorNode = workflow.Where(node => node.Value == context.Source)
                                              .Select(node => workflow.Predecessors(node).FirstOrDefault())
                                              .FirstOrDefault();
                if (predecessorNode != null)
                {
                    var inputInspector = (InspectBuilder)predecessorNode.Value;
                    inputHandle = inputInspector.Output.Merge().Subscribe(value => input = (IplImage)value);
                }
            }

            font = new Font(1);
            base.Load(provider);
        }

        public override void Unload()
        {
            if (canvas != null)
            {
                canvas.Close();
                canvas = null;
            }

            if (inputHandle != null)
            {
                inputHandle.Dispose();
                inputHandle = null;
            }
            base.Unload();
        }
    }
}
