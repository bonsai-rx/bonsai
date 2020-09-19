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
    public class VideoTexture : Texture2D
    {
        [Category("TextureData")]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [FileNameFilter("Video Files|*.avi;*.mp4;*.ogg;*.ogv;*.wmv|AVI Files (*.avi)|*.avi|MP4 Files (*.mp4)|*.mp4|OGG Files (*.ogg;*.ogv)|*.ogg;*.ogv|WMV Files (*.wmv)|*.wmv")]
        [Description("The name of the movie file.")]
        public string FileName { get; set; }

        [Category("TextureData")]
        [Description("Specifies the optional flip mode applied to loaded video frames.")]
        public FlipMode? FlipMode { get; set; }

        [Category("TextureData")]
        [Description("The optional size of the pre-loading buffer for video frames.")]
        public int? BufferLength { get; set; }

        [Category("TextureData")]
        [Description("The optional offset into the video, in frames, at which the sequence should start.")]
        public int Offset { get; set; }

        [Category("TextureData")]
        [Description("Indicates whether to preload only the specified number of buffer video frames into texture memory.")]
        public bool Preload { get; set; }

        public override Texture CreateResource(ResourceManager resourceManager)
        {
            var fileName = FileName;
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException(string.Format(
                    "A valid movie file path was not specified for texture \"{0}\".",
                    Name));
            }

            var capture = Capture.CreateFileCapture(fileName);
            if (capture == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Failed to load movie file \"{0}\" from the specified path: \"{1}\".",
                    Name, fileName));
            }

            var offset = Offset;
            var flipMode = FlipMode;
            var preloaded = Preload;
            var fps = capture.GetProperty(CaptureProperty.Fps);
            var frameCount = (int)capture.GetProperty(CaptureProperty.FrameCount);
            var width = Width.GetValueOrDefault((int)capture.GetProperty(CaptureProperty.FrameWidth));
            var height = Height.GetValueOrDefault((int)capture.GetProperty(CaptureProperty.FrameHeight));
            var internalFormat = width > 0 && height > 0 ? (PixelInternalFormat?)null : InternalFormat;
            var bufferLength = BufferLength.GetValueOrDefault(1);
            if (preloaded) bufferLength = Math.Min(frameCount, bufferLength);
            var frames = new VideoEnumerator(capture, width, height, offset, flipMode, !preloaded);
            frames.Reset();

            ITextureSequence sequence;
            if (preloaded)
            {
                var texture = new TextureSequence(bufferLength);
                try
                {
                    for (int i = 0; i < bufferLength; i++)
                    {
                        frames.MoveNext();
                        texture.SetActiveTexture(i++);
                        ConfigureTexture(texture, width, height);
                        TextureHelper.UpdateTexture(TextureTarget.Texture2D, internalFormat, frames.Current);
                    }
                }
                finally { frames.Dispose(); }
                sequence = texture;
            }
            else
            {
                var texture = new TextureStream(frames, internalFormat, bufferLength);
                ConfigureTexture(texture, width, height);
                sequence = texture;
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
            sequence.PlaybackRate = fps;
            sequence.Reset();
            return (Texture)sequence;
        }

        class VideoEnumerator : IEnumerator<IplImage>
        {
            readonly Capture capture;
            readonly int width;
            readonly int height;
            readonly double offset;
            readonly FlipMode? flipMode;
            readonly bool clone;

            public VideoEnumerator(Capture capture, int width, int height, double offset, FlipMode? flipMode, bool clone)
            {
                this.capture = capture;
                this.width = width;
                this.height = height;
                this.offset = offset;
                this.flipMode = flipMode;
                this.clone = clone;
            }

            public IplImage Current { get; private set; }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                var frame = capture.QueryFrame();
                if (frame == null)
                {
                    Current = null;
                    return false;
                }

                if (width > 0 && height > 0 && (frame.Width != width || frame.Height != height))
                {
                    var resized = new IplImage(new Size(width, height), frame.Depth, frame.Channels);
                    CV.Resize(frame, resized);
                    frame = resized;
                }
                else if (clone)
                {
                    frame = frame.Clone();
                }

                if (flipMode.HasValue) CV.Flip(frame, null, flipMode.Value);
                Current = frame;
                return true;
            }

            public void Reset()
            {
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
