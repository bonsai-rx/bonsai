using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reactive.Linq;
using OpenCV.Net;
using Bonsai.Expressions;
using Bonsai.Dag;

namespace Bonsai.Vision.Design
{
    public class FileCaptureEditor : WorkflowComponentEditor
    {
        readonly List<TypeVisualizerDialog> editorForms = new List<TypeVisualizerDialog>();

        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var workflow = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
                var editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
                if (workflow != null && editorService != null && editorService.WorkflowRunning)
                {
                    var editorForm = editorForms.FirstOrDefault(form => form.Tag == component);
                    if (editorForm == null)
                    {
                        editorForm = new TypeVisualizerDialog();
                        var videoPlayer = new VideoPlayerControl { Dock = DockStyle.Fill };
                        editorForm.AddControl(videoPlayer);
                        var captureNode = (from node in workflow
                                           let builder = node.Value as SourceBuilder
                                           where builder != null && builder.Generator == component
                                           select node)
                                           .FirstOrDefault();

                        var captureOutput = ((InspectBuilder)workflow.Successors(captureNode).First().Value).Output.Merge();
                        var capture = (FileCapture)component;
                        var captureFrame = captureOutput
                            .Select(image => Tuple.Create((IplImage)image, (int)capture.Capture.GetProperty(CaptureProperty.POS_FRAMES)))
                            .Do(frame => videoPlayer.Update(frame.Item1, frame.Item2 - 1));

                        var frameRate = 0.0;
                        IDisposable captureFrameHandle = null;
                        EventHandler workflowStoppedHandler = (sender, e) => editorForm.Close();
                        editorService.WorkflowStopped += workflowStoppedHandler;
                        editorForm.HandleCreated += (sender, e) =>
                        {
                            videoPlayer.Loop = capture.Loop;
                            videoPlayer.Playing = capture.Playing;
                            frameRate = capture.Capture.GetProperty(CaptureProperty.FPS);
                            videoPlayer.PlaybackRate = capture.PlaybackRate == 0 ? frameRate : capture.PlaybackRate;
                            videoPlayer.FrameCount = (int)capture.Capture.GetProperty(CaptureProperty.FRAME_COUNT);
                            captureFrameHandle = captureFrame.Subscribe();
                        };

                        editorForm.FormClosed += (sender, e) =>
                        {
                            captureFrameHandle.Dispose();
                            editorService.WorkflowStopped -= workflowStoppedHandler;
                            editorForms.Remove(editorForm);
                        };

                        videoPlayer.LoopChanged += (sender, e) => capture.Loop = videoPlayer.Loop;
                        videoPlayer.PlayingChanged += (sender, e) => capture.Playing = videoPlayer.Playing;
                        videoPlayer.PlaybackRateChanged += (sender, e) => capture.PlaybackRate = videoPlayer.PlaybackRate == frameRate ? 0 : Math.Max(1, videoPlayer.PlaybackRate);
                        videoPlayer.Seek += (sender, e) => capture.Seek(e.FrameNumber);
                        editorForm.Tag = capture;
                        editorForm.Text = capture.FileName;
                        editorForm.Show(owner);
                        editorForms.Add(editorForm);
                    }

                    editorForm.Activate();
                    return true;
                }
            }

            return false;
        }
    }
}
