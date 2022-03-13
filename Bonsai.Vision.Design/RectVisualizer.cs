using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System;

[assembly: TypeVisualizer(typeof(RectVisualizer), Target = typeof(Rect))]
[assembly: TypeVisualizer(typeof(RectVisualizer), Target = typeof(Rect[]))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer for rectangle parameters. If the input is a sequence
    /// of images, the visualizer will overlay each rectangle on top of the original image.
    /// </summary>
    public class RectVisualizer : IplImageVisualizer
    {
        const float DefaultHeight = 480;
        const int DefaultThickness = 2;
        ObjectTextVisualizer textVisualizer;
        IDisposable inputHandle;
        IplImage input;
        IplImage canvas;

        static void Draw(IplImage image, Rect rect)
        {
            var color = image.Channels == 1 ? Scalar.Real(255) : Scalar.Rgb(255, 0, 0);
            var thickness = DefaultThickness * (int)Math.Ceiling(image.Height / DefaultHeight);
            CV.Rectangle(image, rect, color, thickness);
        }

        internal static void Draw(IplImage image, object value)
        {
            if (image != null)
            {
                if (value is Rect[] array)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        Draw(image, array[i]);
                    }
                }
                else Draw(image, (Rect)value);
            }
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            if (textVisualizer != null) textVisualizer.Show(value);
            else
            {
                if (input != null)
                {
                    canvas = IplImageHelper.EnsureColorCopy(canvas, input);
                    Draw(canvas, value);
                    base.Show(canvas);
                }
            }
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            var imageInput = VisualizerHelper.ImageInput(provider);
            if (imageInput != null)
            {
                inputHandle = imageInput.Subscribe(value => input = (IplImage)value);
                base.Load(provider);
            }
            else
            {
                textVisualizer = new ObjectTextVisualizer();
                textVisualizer.Load(provider);
            }
        }

        /// <inheritdoc/>
        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            if (textVisualizer != null) return textVisualizer.Visualize(source, provider);
            else return base.Visualize(source, provider);
        }

        /// <inheritdoc/>
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

            if (textVisualizer != null)
            {
                textVisualizer.Unload();
                textVisualizer = null;
            }
            else base.Unload();
        }
    }
}
