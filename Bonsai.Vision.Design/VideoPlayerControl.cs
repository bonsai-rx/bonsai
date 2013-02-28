using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCV.Net;

namespace Bonsai.Vision.Design
{
    public partial class VideoPlayerControl : UserControl
    {
        bool playing;
        double playbackRate;
        bool allowUpdate;
        bool canvasInvalidated;
        ToolStripButton loopButton;
        ToolStripStatusLabel statusLabel;
        ToolStripStatusLabel frameNumberLabel;

        public VideoPlayerControl()
        {
            InitializeComponent();
            allowUpdate = true;
            canvasInvalidated = false;
            var playButton = new ToolStripButton(">");
            var pauseButton = new ToolStripButton("| |");
            var slowerButton = new ToolStripButton("<<");
            var fasterButton = new ToolStripButton(">>");
            loopButton = new ToolStripButton("loop");
            loopButton.CheckOnClick = true;
            statusLabel = new ToolStripStatusLabel();
            frameNumberLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(playButton);
            statusStrip.Items.Add(pauseButton);
            statusStrip.Items.Add(slowerButton);
            statusStrip.Items.Add(fasterButton);
            statusStrip.Items.Add(loopButton);
            statusStrip.Items.Add(frameNumberLabel);
            statusStrip.Items.Add(statusLabel);

            if (!DesignMode) statusStrip.Visible = false;
            imageControl.Canvas.MouseClick += new MouseEventHandler(imageControl_MouseClick);
            playButton.Click += (sender, e) => Playing = true;
            pauseButton.Click += (sender, e) => Playing = false;
            slowerButton.Click += (sender, e) => DecreasePlaybackRate();
            fasterButton.Click += (sender, e) => IncreasePlaybackRate();
            imageControl.SwapBuffers += (sender, e) => SwapBuffers();
            imageControl.Canvas.MouseMove += (sender, e) =>
            {
                var image = imageControl.Image;
                if (image != null)
                {
                    var cursorPosition = imageControl.Canvas.PointToClient(Form.MousePosition);
                    if (imageControl.ClientRectangle.Contains(cursorPosition))
                    {
                        var imageX = (int)(cursorPosition.X * ((float)image.Width / imageControl.Width));
                        var imageY = (int)(cursorPosition.Y * ((float)image.Height / imageControl.Height));
                        var cursorColor = Core.cvGet2D(image, imageY, imageX);
                        statusLabel.Text = string.Format("Cursor: ({0},{1}) Value: ({2},{3},{4})", imageX, imageY, cursorColor.Val0, cursorColor.Val1, cursorColor.Val2);
                    }
                }
            };

            imageControl.Canvas.DoubleClick += (sender, e) =>
            {
                var image = imageControl.Image;
                if (image != null)
                {
                    Parent.ClientSize = new Size(image.Width, image.Height);
                }
            };
        }

        private void SwapBuffers()
        {
            if (canvasInvalidated)
            {
                canvasInvalidated = false;
                allowUpdate = true;
            }
        }

        public int FrameCount
        {
            get { return seekBar.Maximum; }
            set { seekBar.Maximum = value; }
        }

        public double PlaybackRate
        {
            get { return playbackRate; }
            set
            {
                playbackRate = value;
                OnPlaybackRateChanged(EventArgs.Empty);
            }
        }

        public bool Loop
        {
            get { return loopButton.CheckState == CheckState.Checked; }
            set { loopButton.CheckState = value ? CheckState.Checked : CheckState.Unchecked; }
        }

        public bool Playing
        {
            get { return playing; }
            set
            {
                playing = value;
                OnPlayingChanged(EventArgs.Empty);
            }
        }

        public event EventHandler PlaybackRateChanged;

        public event EventHandler PlayingChanged;

        public event EventHandler LoopChanged
        {
            add { loopButton.CheckStateChanged += value; }
            remove { loopButton.CheckStateChanged -= value; }
        }

        public event ScrollEventHandler Seek
        {
            add { seekBar.Scroll += value; }
            remove { seekBar.Scroll -= value; }
        }

        protected virtual void OnPlayingChanged(EventArgs e)
        {
            var handler = PlayingChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPlaybackRateChanged(EventArgs e)
        {
            var handler = PlaybackRateChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Update(IplImage frame, int frameNumber)
        {
            if (!allowUpdate) return;
            else
            {
                allowUpdate = false;
                BeginInvoke((Action)(() =>
                {
                    imageControl.Image = frame;
                    seekBar.Value = frameNumber;
                    if (frame == null) statusLabel.Text = string.Empty;
                    frameNumberLabel.Text = string.Format("Frame: {0}", frameNumber);
                    canvasInvalidated = true;
                }));
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Space)
            {
                Playing = !Playing;
            }

            if (keyData == Keys.Add) IncreasePlaybackRate();
            if (keyData == Keys.Subtract) DecreasePlaybackRate();
            return base.ProcessCmdKey(ref msg, keyData);
        }

        void imageControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                statusStrip.Visible = !statusStrip.Visible;
            }
        }

        void IncreasePlaybackRate()
        {
            PlaybackRate *= 2;
        }

        void DecreasePlaybackRate()
        {
            PlaybackRate /= 2;
        }
    }
}
