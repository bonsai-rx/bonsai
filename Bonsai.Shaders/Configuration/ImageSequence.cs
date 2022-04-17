using Bonsai.Resources;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for initializing texture
    /// sequences from a movie file or image sequence search pattern.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class ImageSequence : Texture2D
    {
        static readonly string[] ImageExtensions = new[] { ".png", ".bmp", ".jpg", ".jpeg", ".tif" };

        /// <summary>
        /// Gets or sets the path to a movie file or image sequence search pattern.
        /// </summary>
        [Category("TextureData")]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [FileNameFilter("Video Files|*.avi;*.mp4;*.ogg;*.ogv;*.wmv|AVI Files (*.avi)|*.avi|MP4 Files (*.mp4)|*.mp4|OGG Files (*.ogg;*.ogv)|*.ogg;*.ogv|WMV Files (*.wmv)|*.wmv")]
        [Description("The path to a movie file or image sequence search pattern.")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the flip mode applied to individual frames.
        /// </summary>
        [Category("TextureData")]
        [Description("Specifies the flip mode applied to individual frames.")]
        public FlipMode? FlipMode { get; set; } = OpenCV.Net.FlipMode.Vertical;

        /// <summary>
        /// Gets or sets the maximum number of frames to include in the image sequence.
        /// </summary>
        /// <remarks>
        /// If no value is specified, all frames in the video will be loaded in the
        /// image sequence.
        /// </remarks>
        [Category("TextureData")]
        [Description("The maximum number of frames to include in the image sequence.")]
        public int? FrameCount { get; set; }

        /// <summary>
        /// Gets or sets the offset, in frames, at which the image sequence should start.
        /// </summary>
        [Category("TextureData")]
        [Description("The offset, in frames, at which the image sequence should start.")]
        public int StartPosition { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Texture"/> class containing
        /// all loaded texture resources in the image sequence.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="Texture"/> class representing
        /// the image texture sequence.
        /// </returns>
        /// <inheritdoc/>
        public override Texture CreateResource(ResourceManager resourceManager)
        {
            var frames = GetFrames(FileName, clone: false, out bool video, out PixelInternalFormat? internalFormat);
            if (video) frames.Reset();

            var sequence = new TextureSequence(frames.Count);
            using var enumerator = sequence.GetEnumerator(false);
            try
            {
                while (enumerator.MoveNext())
                {
                    ConfigureTexture(sequence, frames.Width, frames.Height);
                    if (!frames.MoveNext()) continue;
                    TextureHelper.UpdateTexture(TextureTarget.Texture2D, internalFormat, frames.Current);
                }
            }
            finally { frames.Dispose(); }
            GL.BindTexture(TextureTarget.Texture2D, 0);
            sequence.PlaybackRate = frames.PlaybackRate;
            return sequence;
        }

        internal VideoEnumerator GetFrames(string fileName, bool clone, out bool video, out PixelInternalFormat? internalFormat)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException(string.Format(
                    "A valid movie file or image sequence path was not specified for texture \"{0}\".",
                    Name));
            }

            var capture = Capture.CreateFileCapture(fileName);
            if (capture == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Failed to load image sequence \"{0}\" from the specified path: \"{1}\".",
                    Name, fileName));
            }

            var extension = Path.GetExtension(fileName);
            video = Array.FindIndex(ImageExtensions, ext => extension.Contains(ext)) < 0;

            var flipMode = FlipMode;
            var offset = StartPosition;
            var frameCount = (int)capture.GetProperty(CaptureProperty.FrameCount);
            frameCount = FrameCount.HasValue ? Math.Min(FrameCount.Value, frameCount) : frameCount;
            var width = Width.GetValueOrDefault((int)capture.GetProperty(CaptureProperty.FrameWidth));
            var height = Height.GetValueOrDefault((int)capture.GetProperty(CaptureProperty.FrameHeight));
            internalFormat = width > 0 && height > 0 ? (PixelInternalFormat?)null : InternalFormat;
            return new VideoEnumerator(capture, width, height, offset, frameCount, flipMode, clone);
        }

        internal class VideoEnumerator : IEnumerator<IplImage>
        {
            readonly Capture capture;
            readonly int offset;
            readonly FlipMode? flipMode;
            readonly bool clone;
            int index;

            public VideoEnumerator(Capture capture, int width, int height, int offset, int count, FlipMode? flipMode, bool clone)
            {
                PlaybackRate = capture.GetProperty(CaptureProperty.Fps);
                FourCC = (int)capture.GetProperty(CaptureProperty.FourCC);
                Width = width;
                Height = height;
                Count = count;
                this.capture = capture;
                this.offset = offset;
                this.flipMode = flipMode;
                this.clone = clone;
            }

            public int Width { get; }

            public int Height { get; }

            public int Count { get; }

            public double PlaybackRate { get; }

            public int FourCC { get; }

            public IplImage Current { get; private set; }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                IplImage frame;
                if (index >= Count || (frame = capture.QueryFrame()) == null)
                {
                    Current = null;
                    return false;
                }

                if (Width > 0 && Height > 0 && (frame.Width != Width || frame.Height != Height))
                {
                    var resized = new IplImage(new Size(Width, Height), frame.Depth, frame.Channels);
                    CV.Resize(frame, resized);
                    frame = resized;
                }
                else if (clone)
                {
                    frame = frame.Clone();
                }

                if (flipMode.HasValue) CV.Flip(frame, null, flipMode.Value);
                Current = frame;
                index += 1;
                return true;
            }

            public void Reset()
            {
                index = 0;
                capture.SetProperty(CaptureProperty.PosFrames, offset);
            }

            public void Dispose()
            {
                capture.Close();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var name = Name;
            var fileName = FileName;
            var typeName = GetType().Name;
            if (string.IsNullOrEmpty(name)) return typeName;
            else if (string.IsNullOrEmpty(fileName)) return name;
            else return $"{name} [Sequence: {fileName}]";
        }
    }
}
