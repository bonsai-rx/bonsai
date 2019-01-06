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
        readonly DrawingCall[] drawing;

        internal Canvas(Func<IplImage> generator)
        {
            this.generator = generator;
        }

        internal Canvas(Canvas canvas, DrawingCall draw)
        {
            generator = canvas.generator;
            drawing = SubCanvas.AppendCommand(canvas.drawing, draw);
        }

        internal Canvas(Canvas source, Canvas other)
        {
            if (source.generator != other.generator)
            {
                throw new InvalidOperationException("Unable to merge drawing operators targeting different image sources.");
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
                    try { drawing[i].Action(image); }
                    catch (Exception ex)
                    {
                        drawing[i].Observer.OnError(ex);
                        throw;
                    }
                }
            }
            return image;
        }

        public static Canvas operator +(Canvas left, Canvas right)
        {
            return Merge(left, right);
        }

        public static Canvas Merge(Canvas source, Canvas other)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return new Canvas(source, other);
        }
    }

    struct DrawingCall
    {
        public Action<IplImage> Action;
        public IObserver<Canvas> Observer;
    }

    class SubCanvas
    {
        internal readonly Func<IplImage, IplImage> generator;
        internal readonly DrawingCall[] drawing;

        public SubCanvas(Func<IplImage, IplImage> generator)
        {
            this.generator = generator;
        }

        public SubCanvas(SubCanvas canvas, DrawingCall draw)
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

        internal static DrawingCall[] AppendCommand(DrawingCall[] drawing, DrawingCall draw)
        {
            if (drawing == null) return new[] { draw };
            else
            {
                var subCanvas = drawing[drawing.Length - 1].Action.Target as SubCanvas;
                if (subCanvas != null)
                {
                    subCanvas = new SubCanvas(subCanvas, draw);
                    var result = new DrawingCall[drawing.Length];
                    Array.Copy(drawing, result, drawing.Length - 1);
                    draw.Action = subCanvas.Draw;
                    result[drawing.Length - 1] = draw;
                    return result;
                }
                else
                {
                    var result = new DrawingCall[drawing.Length + 1];
                    Array.Copy(drawing, result, drawing.Length);
                    result[drawing.Length] = draw;
                    return result;
                }
            }
        }

        internal static DrawingCall[] MergeCommands(DrawingCall[] source, DrawingCall[] other, bool excludeSource = false)
        {
            if (source == other) return source;
            else if (source == null) return other;
            else if (other == null) return source;
            else
            {
                var commandList = new List<DrawingCall>();
                var hashSet = new Dictionary<Action<IplImage>, int>(SubCanvasEqualityComparer.Default);
                for (int i = 0; i < source.Length; i++)
                {
                    hashSet.Add(source[i].Action, i);
                    if (!excludeSource) commandList.Add(source[i]);
                }

                for (int i = 0; i < other.Length; i++)
                {
                    int index;
                    if (hashSet.TryGetValue(other[i].Action, out index))
                    {
                        var sourceDraw = source[index];
                        if (object.ReferenceEquals(sourceDraw.Action, other[i].Action)) continue;

                        var subCanvas = other[i].Action.Target as SubCanvas;
                        if (subCanvas != null)
                        {
                            if (!excludeSource && i == 0 && index == hashSet.Count - 1)
                            {
                                // actions are adjacent in command list so we can merge the two subcanvases
                                subCanvas = new SubCanvas((SubCanvas)sourceDraw.Action.Target, subCanvas, excludeSource: false);
                                sourceDraw.Action = subCanvas.Draw;
                                hashSet.Remove(sourceDraw.Action);
                                hashSet.Add(sourceDraw.Action, index);
                                commandList[index] = sourceDraw;
                            }
                            else
                            {
                                // remove redundant actions from other subcanvas
                                subCanvas = new SubCanvas((SubCanvas)sourceDraw.Action.Target, subCanvas, excludeSource: true);
                                sourceDraw.Action = subCanvas.Draw;
                                commandList.Add(sourceDraw);
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
            var subImage = generator(image);
            try
            {
                if (drawing != null)
                {
                    for (int i = 0; i < drawing.Length; i++)
                    {
                        try { drawing[i].Action(subImage); }
                        catch (Exception ex)
                        {
                            drawing[i].Observer.OnError(ex);
                            throw;
                        }
                    }
                }
            }
            finally
            {
                if (subImage != image)
                {
                    subImage.Dispose();
                }
            }
        }
    }
}
