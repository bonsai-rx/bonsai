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
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (workflow != null && editorState != null && editorState.WorkflowRunning)
                {
                    var editorForm = editorForms.FirstOrDefault(form => form.Tag == component);
                    if (editorForm == null)
                    {
                        editorForm = new TypeVisualizerDialog();
                        var videoPlayer = new VideoPlayer { Dock = DockStyle.Fill };
                        editorForm.AddControl(videoPlayer);

                        var captureNode = workflow.FirstOrDefault(node => ExpressionBuilder.GetWorkflowElement(node.Value) == component);
                        var captureInspector = captureNode != null ? captureNode.Value as InspectBuilder : null;
                        if (captureInspector == null) return false;

                        var captureOutput = captureInspector.Output.Merge();
                        var capture = (FileCapture)component;
                        var captureFrame = captureOutput
                            .Select(image => Tuple.Create((IplImage)image, (int)capture.Capture.GetProperty(CaptureProperty.PosFrames)))
                            .Do(frame => videoPlayer.Update(frame.Item1, frame.Item2 - 1));

                        var frameRate = 0.0;
                        IDisposable captureFrameHandle = null;
                        EventHandler workflowStoppedHandler = (sender, e) => editorForm.Close();
                        editorState.WorkflowStopped += workflowStoppedHandler;
                        editorForm.HandleCreated += (sender, e) =>
                        {
                            videoPlayer.Loop = capture.Loop;
                            videoPlayer.Playing = capture.Playing;
                            frameRate = capture.Capture.GetProperty(CaptureProperty.Fps);
                            videoPlayer.PlaybackRate = capture.PlaybackRate == 0 ? frameRate : capture.PlaybackRate;
                            videoPlayer.FrameCount = (int)capture.Capture.GetProperty(CaptureProperty.FrameCount);
                            captureFrameHandle = captureFrame.Subscribe();
                        };

                        editorForm.FormClosed += (sender, e) =>
                        {
                            captureFrameHandle.Dispose();
                            editorState.WorkflowStopped -= workflowStoppedHandler;
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
