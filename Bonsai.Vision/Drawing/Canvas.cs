using OpenCV.Net;
using System;
using System.Collections.Generic;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Provides support for lazy initialization and rendering of dynamic bitmaps.
    /// </summary>
    /// <remarks>
    /// Each canvas stores a generator function, used to allocate the bitmap memory,
    /// and an immutable sequence of drawing operations to be applied to the bitmap
    /// in order to produce the final image.
    /// </remarks>
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
            generator = source.generator;
            drawing = SubCanvas.MergeCommands(source.drawing, other.drawing);
        }

        /// <summary>
        /// Allocates the bitmap memory and applies the sequence of operations
        /// to create a new drawing.
        /// </summary>
        /// <returns>
        /// An <see cref="IplImage"/> object representing the result of the
        /// cumulative application of all the drawing operations to the canvas
        /// bitmap.
        /// </returns>
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

        /// <summary>
        /// Concatenates the drawing operations of two specified canvas.
        /// </summary>
        /// <param name="left">The first canvas to concatenate.</param>
        /// <param name="right">The second canvas to concatenate.</param>
        /// <returns>
        /// A new <see cref="Canvas"/> object representing the application
        /// of the operations of the <paramref name="left"/> canvas,
        /// followed by the operations of the <paramref name="right"/> canvas.
        /// </returns>
        public static Canvas operator +(Canvas left, Canvas right)
        {
            return Merge(left, right);
        }

        /// <summary>
        /// Combines the drawing operations of two specified canvas.
        /// </summary>
        /// <param name="source">The first canvas object to merge.</param>
        /// <param name="other">
        /// The second canvas object to merge. The bitmap allocators for both
        /// canvas objects must be identical for drawing operations to be
        /// composable.
        /// </param>
        /// <returns>
        /// A new <see cref="Canvas"/> object representing the application
        /// of the operations of the <paramref name="source"/> canvas,
        /// followed by the operations of the <paramref name="other"/> canvas.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> or <paramref name="other"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The bitmap allocator of the <paramref name="other"/> canvas is not
        /// the same as the allocator for the <paramref name="source"/> canvas.
        /// </exception>
        public static Canvas Merge(Canvas source, Canvas other)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (source.generator != other.generator)
            {
                throw new ArgumentException(
                    "Unable to merge drawing operations targeting different image sources.",
                    nameof(other));
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
                    if (hashSet.TryGetValue(other[i].Action, out int index))
                    {
                        var sourceDraw = source[index];
                        if (ReferenceEquals(sourceDraw.Action, other[i].Action)) continue;

                        if (other[i].Action.Target is SubCanvas subCanvas)
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
                if (ReferenceEquals(x, y)) return true;
                else if (x != null && y != null)
                {
                    if (x.Target is SubCanvas subCanvas1 &&
                        y.Target is SubCanvas subCanvas2 &&
                        subCanvas1.generator == subCanvas2.generator)
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
