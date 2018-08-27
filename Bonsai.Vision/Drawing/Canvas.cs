using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision.Drawing
{
    public class Canvas
    {
        readonly Func<IplImage> generator;
        readonly Action<IplImage>[] drawing;

        internal Canvas(Func<IplImage> generator)
        {
            this.generator = generator;
        }

        internal Canvas(Canvas canvas, Action<IplImage> draw)
        {
            generator = canvas.generator;
            drawing = SubCanvas.AppendCommand(canvas.drawing, draw);
        }

        internal Canvas(Canvas source, Canvas other)
        {
            if (source.generator != other.generator)
            {
                throw new InvalidOperationException();
            }

            generator = source.generator;
            drawing = SubCanvas.MergeCommands(source.drawing, other.drawing);
        }

        public IplImage Draw()
        {
            var image = generator();
            if (drawing != null)
            {
                for (int i = 0; i < drawing.Length; i++)
                {
                    drawing[i](image);
                }
            }
            return image;
        }
    }

    class SubCanvas
    {
        internal readonly Func<IplImage, IplImage> generator;
        internal readonly Action<IplImage>[] drawing;

        public SubCanvas(Func<IplImage, IplImage> generator)
        {
            this.generator = generator;
        }

        public SubCanvas(SubCanvas canvas, Action<IplImage> draw)
        {
            generator = canvas.generator;
            drawing = AppendCommand(canvas.drawing, draw);
        }

        public SubCanvas(SubCanvas source, SubCanvas other, bool excludeSource)
        {
            if (source.generator != other.generator)
            {
                throw new InvalidOperationException();
            }

            generator = source.generator;
            drawing = MergeCommands(source.drawing, other.drawing, excludeSource);
        }

        internal static Action<IplImage>[] AppendCommand(Action<IplImage>[] drawing, Action<IplImage> draw)
        {
            if (drawing == null) return new[] { draw };
            else
            {
                var subCanvas = drawing[drawing.Length - 1].Target as SubCanvas;
                if (subCanvas != null)
                {
                    subCanvas = new SubCanvas(subCanvas, draw);
                    var result = new Action<IplImage>[drawing.Length];
                    Array.Copy(drawing, result, drawing.Length - 1);
                    result[drawing.Length - 1] = subCanvas.Draw;
                    return result;
                }
                else
                {
                    var result = new Action<IplImage>[drawing.Length + 1];
                    Array.Copy(drawing, result, drawing.Length);
                    result[drawing.Length] = draw;
                    return result;
                }
            }
        }

        internal static Action<IplImage>[] MergeCommands(Action<IplImage>[] source, Action<IplImage>[] other, bool excludeSource = false)
        {
            if (source == other) return source;
            else if (source == null) return other;
            else if (other == null) return source;
            else
            {
                var commandList = new List<Action<IplImage>>();
                var hashSet = new Dictionary<Action<IplImage>, int>(SubCanvasEqualityComparer.Default);
                for (int i = 0; i < source.Length; i++)
                {
                    hashSet.Add(source[i], i);
                    if (!excludeSource) commandList.Add(source[i]);
                }

                for (int i = 0; i < other.Length; i++)
                {
                    int index;
                    if (hashSet.TryGetValue(other[i], out index))
                    {
                        var sourceAction = source[index];
                        if (object.ReferenceEquals(sourceAction, other[i])) continue;

                        var subCanvas = other[i].Target as SubCanvas;
                        if (subCanvas != null)
                        {
                            if (!excludeSource && i == 0 && index == hashSet.Count - 1)
                            {
                                // actions are adjacent in command list so we can merge the two subcanvases
                                subCanvas = new SubCanvas((SubCanvas)sourceAction.Target, subCanvas, excludeSource: false);
                                sourceAction = subCanvas.Draw;
                                hashSet.Remove(sourceAction);
                                hashSet.Add(sourceAction, index);
                                commandList[index] = sourceAction;
                            }
                            else
                            {
                                // remove redundant actions from other subcanvas
                                subCanvas = new SubCanvas((SubCanvas)sourceAction.Target, subCanvas, excludeSource: true);
                                sourceAction = subCanvas.Draw;
                                commandList.Add(sourceAction);
                            }
                        }
                    }
                    else commandList.Add(other[i]);
                }

                return commandList.ToArray();
            }
        }

        class SubCanvasEqualityComparer : IEqualityComparer<Action<IplImage>>
        {
            public static readonly SubCanvasEqualityComparer Default = new SubCanvasEqualityComparer();

            public bool Equals(Action<IplImage> x, Action<IplImage> y)
            {
                if (object.ReferenceEquals(x, y)) return true;
                else if (x != null && y != null)
                {
                    var subCanvas1 = x.Target as SubCanvas;
                    var subCanvas2 = y.Target as SubCanvas;
                    if (subCanvas1 != null && subCanvas2 != null && subCanvas1.generator == subCanvas2.generator)
                    {
                        return true;
                    }
                }

                return false;
            }

            public int GetHashCode(Action<IplImage> obj)
            {
                var subCanvas = obj != null ? obj.Target as SubCanvas : null;
                if (subCanvas != null)
                {
                    return subCanvas.generator.GetHashCode();
                }

                return EqualityComparer<Action<IplImage>>.Default.GetHashCode(obj);
            }
        }

        public void Draw(IplImage image)
        {
            using (var subImage = generator(image))
            {
                if (drawing != null)
                {
                    for (int i = 0; i < drawing.Length; i++)
                    {
                        drawing[i](subImage);
                    }
                }
            }
        }
    }
}
