using Bonsai;
using Bonsai.Design;
using Bonsai.Expressions;
using Bonsai.Vision;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

[assembly: TypeVisualizer(typeof(FileCaptureVisualizer), Target = typeof(FileCapture))]

namespace Bonsai.Vision.Design
{
    public class FileCaptureVisualizer : ImageMashupVisualizer
    {
        FileCapture capture;
        VideoPlayer videoPlayer;
        Capture captureCache;
        double frameRate;

        public override VisualizerCanvas VisualizerCanvas
        {
            get { return videoPlayer?.Canvas; }
        }

        public override void Load(IServiceProvider provider)
        {
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var visualizerElement = ExpressionBuilder.GetVisualizerElement(context.Source);
            capture = (FileCapture)ExpressionBuilder.GetWorkflowElement(visualizerElement.Builder);
            videoPlayer = new VideoPlayer { Dock = DockStyle.Fill };
            videoPlayer.LoopChanged += (sender, e) => capture.Loop = videoPlayer.Loop;
            videoPlayer.PlayingChanged += (sender, e) => capture.Playing = videoPlayer.Playing;
            videoPlayer.PlaybackRateChanged += (sender, e) => capture.PlaybackRate = videoPlayer.PlaybackRate == frameRate ? 0 : Math.Max(1, videoPlayer.PlaybackRate);
            videoPlayer.Seek += (sender, e) => capture.Seek(e.FrameNumber);

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(videoPlayer);
            }

            base.Load(provider);
        }

        public override void Show(object value)
        {
            var input = (Tuple<IplImage, int>)value;
            var image = input.Item1;
            base.Show(image);
        }

        protected override void ShowMashup(IList<object> values)
        {
            if (captureCache != capture.Capture)
            {
                captureCache = capture.Capture;
                videoPlayer.Loop = capture.Loop;
                videoPlayer.Playing = capture.Playing;
                frameRate = captureCache.GetProperty(CaptureProperty.Fps);
                videoPlayer.PlaybackRate = capture.PlaybackRate == 0 ? frameRate : capture.PlaybackRate;
                videoPlayer.FrameCount = (int)captureCache.GetProperty(CaptureProperty.FrameCount);
            }

            var input = (Tuple<IplImage, int>)values[0];
            var frameNumber = input.Item2;
            base.ShowMashup(values);
            videoPlayer.Update(VisualizerImage, frameNumber - 1);
        }

        protected override void UpdateValues(IList<object> values)
        {
            var image = (IplImage)values[0];
            var frameNumber = (int)capture.Capture.GetProperty(CaptureProperty.PosFrames);
            values[0] = Tuple.Create(image, frameNumber);
        }

        public override void Unload()
        {
            base.Unload();
            videoPlayer.Dispose();
            videoPlayer = null;
            capture = null;
            captureCache = null;
        }
    }
}
