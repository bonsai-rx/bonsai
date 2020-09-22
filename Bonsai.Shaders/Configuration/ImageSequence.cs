using Bonsai.Resources;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class ImageSequence : Texture2D
    {
        [Category("TextureData")]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [FileNameFilter("Video Files|*.avi;*.mp4;*.ogg;*.ogv;*.wmv|AVI Files (*.avi)|*.avi|MP4 Files (*.mp4)|*.mp4|OGG Files (*.ogg;*.ogv)|*.ogg;*.ogv|WMV Files (*.wmv)|*.wmv")]
        [Description("The path to a movie file or image sequence search pattern.")]
        public string FileName { get; set; }

        [Category("TextureData")]
        [Description("Specifies the optional flip mode applied to individual frames.")]
        public FlipMode? FlipMode { get; set; } = OpenCV.Net.FlipMode.Vertical;

        [Category("TextureData")]
        [Description("The optional maximum number of frames to include in the image sequence.")]
        public int? FrameCount { get; set; }

        [Category("TextureData")]
        [Description("The offset, in frames, at which the image sequence should start.")]
        public int StartPosition { get; set; }

        public override Texture CreateResource(ResourceManager resourceManager)
        {
            var frames = GetVideoEnumerator(FileName, clone: false, out PixelInternalFormat? internalFormat);
            if (frames.FourCC > 0) frames.Reset();

            var sequence = new TextureSequence(frames.Count);
            using var enumerator = sequence.GetEnumerator(false);
            try
            {
                while (enumerator.MoveNext())
                {
                    frames.MoveNext();
                    ConfigureTexture(sequence, frames.Width, frames.Height);
                    TextureHelper.UpdateTexture(TextureTarget.Texture2D, internalFormat, frames.Current);
                }
            }
            finally { frames.Dispose(); }
            GL.BindTexture(TextureTarget.Texture2D, 0);
            sequence.PlaybackRate = frames.PlaybackRate;
            return (Texture)sequence;
        }

        internal VideoEnumerator GetVideoEnumerator(string fileName, bool clone, out PixelInternalFormat? internalFormat)
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

        public override string ToString()
        {
            var name = Name;
            var fileName = FileName;
            var typeName = GetType().Name;
            if (string.IsNullOrEmpty(name)) return typeName;
            else if (string.IsNullOrEmpty(fileName)) return name;
            else return string.Format("{0} [{1}]", name, fileName);
        }
    }
}
